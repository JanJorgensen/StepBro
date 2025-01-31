using ActiproSoftware.UI.WinForms.Controls.Docking;
using ActiproSoftware.UI.WinForms.Drawing;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Controls;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System.Collections.ObjectModel;
using StepBro.UI.WinForms;
using StepBro.UI.WinForms.Controls;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static StepBro.Core.Host.HostApplicationTaskHandler;
using static StepBro.SimpleWorkbench.Shortcuts;
using StepBroMain = StepBro.Core.Main;
using StepBro.Core.Logging;
using FastColoredTextBoxNS;
using StepBro.UI.WinForms.Dialogs;
using StepBro.Core.Addons;
using StepBro.Core.DocCreation;
using Antlr4.Runtime;
using Lexer = StepBro.Core.Parser.Grammar.StepBroLexer;
using StepBro.Core.Host.Presentation;
using StepBro.HostSupport.Models;
using System.IO;

namespace StepBro.SimpleWorkbench
{
    public partial class MainForm : FormWithHostApplicationTaskHandling, ICoreAccess
    {
        private bool m_appLoadFinished = false;
        private int documentWindowIndex = 1;
        //private int toolWindowIndex = 1;

        //private bool showDarkThemeDisclaimer = true;
        //private bool ignoreModifiedDocumentClose = false;
        private bool ignoreTextChangedEvent = false;

        private bool m_settingCommandCombo = false;

        private CommandLineOptions m_commandLineOptions = null;
        private HostAccess m_hostAccess = null;
        private HostAppModel m_appModel = null;
        private ILoadedFilesManager m_loadedFiles = null;
        private IDynamicObjectManager m_objectManager = null;
        private ILogger m_mainLogger = null;
        private ISymbolLookupService m_symbolLookupService = null;
        private ToolsInteractionModel m_toolsInteractionModel = null;

        private ToolWindow m_toolWindowExecutionLog = null;
        private LogViewer m_logviewer = null;
        private ToolWindow m_toolWindowFileExplorer = null;
        private FileExplorer m_fileExplorer = null;
        private ToolWindow m_toolWindowReportOverview = null;
        private TestReportOverview m_reportOverview = null;
        private ToolWindow m_toolWindowDocCommentPreview = null;
        private DocCommentsPreview m_selectionDocView = null;
        private ToolWindow m_toolWindowUserInteraction = null;
        private UserInteractionHostPanel m_userInteractionView = null;

        private ToolWindow m_toolWindowParsingErrors = null;
        private ParsingErrorListView m_errorsList = null;

        private object m_hostDependancyObject = new object();
        private object m_topScriptFileDependancyObject = new object();
        private object m_userShortcutItemTag = new object();

        private bool m_userFileRead = false;
        private ProjectUserData m_userDataProject = null;

        private string m_targetFile = null;
        private string m_targetFileFullPath = null;
        private IScriptFile m_file = null;

        private IFileElement m_focusElement = null;

        private ObservableCollection<IFileElement> m_fileElements = new ObservableCollection<IFileElement>();
        private Dictionary<string, TypeReference> m_variableTypes = new Dictionary<string, TypeReference>();

        private Queue<ScriptExecutionData> m_executionQueue = new Queue<ScriptExecutionData>();

        private Dictionary<string, List<string>> m_toolCommandHistories = new Dictionary<string, List<string>>();

        private string m_selectedOutputAddon = OutputSimpleCleartextAddon.Name;
        private static IOutputFormatterTypeAddon m_outputAddon = null;
        private IOutputFormatter m_outputFormatter = null;

        private UserInteraction m_currentUserInteractionData = null;

        public MainForm()
        {
            InitializeComponent();

            toolStripButtonRunCommand.Text = "\u23F5";
            toolStripButtonStopScriptExecution.Text = "\u23F9";
            toolStripButtonAddShortcut.Text = "\u2795";
            //this.StartUsingTaskHandlingTimer();
            panelCustomToolstrips.Setup(this);
            this.Size = new System.Drawing.Size(1200, 800);
            //toolWindowProperties.Close();
            //toolWindowHelp.Close();

            //this.CreateTextDocument(null, "This is a read-only document.  Notice the lock context image in the tab.", true).Activate();
            //this.CreateTextDocument(null, null, false).Activate();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UserDataStationManager.LoadUserSettingsOnStation();

            IService m_hostService = null;
            m_hostAccess = new HostAccess(this, out m_hostService);
            IService m_textFileSystemService = null;
            new TextFileSystem(out m_textFileSystemService);
            m_appModel = new HostAppModel();
            m_appModel.Initialize(null, m_hostService, m_textFileSystemService);
            m_mainLogger = StepBroMain.Logger.RootLogger.CreateSubLocation("StepBro.Workbench");
            m_loadedFiles = StepBroMain.GetLoadedFilesManager();
            m_objectManager = StepBroMain.ServiceManager.Get<IDynamicObjectManager>();
            m_symbolLookupService = StepBroMain.ServiceManager.Get<ISymbolLookupService>();
            m_userDataProject = StepBroMain.ServiceManager.Get<ProjectUserData>();
            StepBro.UI.WinForms.CustomToolBar.ToolBar.ToolBarSetup();
            m_toolsInteractionModel = m_appModel.ToolsInteraction;
            m_toolsInteractionModel.PropertyChanged += ToolsMenuModel_PropertyChanged;
            m_toolsInteractionModel.Synchronize();


            this.ParseCommandLineOptions();

            if (m_targetFile == null)
            {
                this.BeginInvoke(this.UserOpenFileDialog);
            }

            // If no tasks are started in ParseCommandLineOptions(), this task will make sure the activity indicator is stopped.
            this.AddTask<TaskNoState>(ApplicationStartupIndicationTask, Priority.Low, "Starting application", "Starting application");

            m_logviewer = new LogViewer();
            m_toolWindowExecutionLog = new ToolWindow(dockManager, "ExecutionLogView", "Execution Log", null, m_logviewer);
            m_toolWindowExecutionLog.DockTo(dockManager, DockOperationType.RightOuter);
            m_toolWindowExecutionLog.State = ToolWindowState.TabbedDocument;
            m_logviewer.Setup();

            m_fileExplorer = new FileExplorer();
            m_fileExplorer.HostDependancyObject = m_hostDependancyObject;
            m_fileExplorer.HostTopFileDependancyObject = m_topScriptFileDependancyObject;
            m_fileExplorer.SelectionChanged += FileExplorer_NodeSelectionChanged;
            m_toolWindowFileExplorer = new ToolWindow(dockManager, "FileExplorerView", "File Explorer", null, m_fileExplorer);
            m_toolWindowFileExplorer.DockedSize = new Size(320, 200);
            m_toolWindowFileExplorer.DockTo(dockManager, DockOperationType.RightOuter);

            dockManager.ImageList = FileExplorer.Images;
            dockManager.SelectedDocumentChanged += DockManager_SelectedDocumentChanged;

            m_errorsList = new ParsingErrorListView();
            m_errorsList.ParseFilesClicked += ErrorsList_ParseFilesClicked;
            m_errorsList.AutoParseFilesChanged += ErrorsList_AutoParseFilesChanged;
            m_errorsList.DoubleClickedLine += ErrorsList_DoubleClickedLine;
            m_toolWindowParsingErrors = new ToolWindow(dockManager, "ErrorsView", "Errors", null, m_errorsList);
            m_toolWindowParsingErrors.DockTo(dockManager, DockOperationType.BottomOuter);

            m_reportOverview = new TestReportOverview();
            m_reportOverview.DoubleClickedResult += ReportOverview_DoubleClickedResult;
            m_reportOverview.DoubleClickedGroup += ReportOverview_DoubleClickedGroup;
            m_toolWindowReportOverview = new ToolWindow(dockManager, "ReportOverviewView", "Report Overview", null, m_reportOverview);
            m_toolWindowReportOverview.DockTo(dockManager, DockOperationType.LeftOuter);
            m_toolWindowReportOverview.Close();

            m_selectionDocView = new DocCommentsPreview();
            m_toolWindowDocCommentPreview = new ToolWindow(dockManager, "SelectionDocView", "Selection Documentation", null, m_selectionDocView);
            m_toolWindowDocCommentPreview.DockTo(dockManager, DockOperationType.RightInner);
            m_toolWindowDocCommentPreview.Close();

            m_userInteractionView = new UserInteractionHostPanel();
            m_toolWindowUserInteraction = new ToolWindow(dockManager, "UserInteractionView", "User Interaction", null, m_userInteractionView);
            m_toolWindowUserInteraction.DockTo(dockManager, DockOperationType.RightInner);
            m_toolWindowUserInteraction.State = ToolWindowState.TabbedDocument;
            m_toolWindowUserInteraction.Close();

            //            private ToolWindow m_toolWindowUserInteraction = null;
            //private UserInteractionHostPanel m_userInteractionView = null;


            // TO BE DELETED
            dockManager.SaveCustomToolWindowLayoutData += DockManager_SaveCustomToolWindowLayoutData;


            m_outputAddon = StepBroMain.GetService<Core.Api.IAddonManager>().TryGetAddon<IOutputFormatterTypeAddon>(m_selectedOutputAddon);
            if (m_outputAddon == null)
            {
                //ConsoleWriteErrorLine("Error: Output format \'" + selectedOutputAddon + "\' was not found.");
                var available = String.Join(", ", StepBroMain.GetService<Core.Api.IAddonManager>().Addons.Where(a => a is IOutputFormatterTypeAddon).Select(a => a.ShortName));
                //ConsoleWriteErrorLine("    Available options: " + available);
                //retval = -1;
                //throw new ExitException();
            }

            m_appLoadFinished = true;


            toolStripDropDownButtonTool.DropDownItems.Clear();
        }

        private TaskAction ApplicationStartupIndicationTask(ref TaskNoState state, ref int index, ITaskStateReporting reporting)
        {
            if (!m_appLoadFinished)
            {
                return TaskAction.Delay100ms;
            }
            // else...
            return TaskAction.Finish;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            this.SaveUserSettingsOnProject();
            UserDataStationManager.SaveUserSettingsOnStation(); // TODO: Remove when UserDataStationManager changed to a service.
            StepBroMain.Deinitialize();
        }

        private void DockManager_SaveCustomToolWindowLayoutData(object sender, DockSaveCustomToolWindowLayoutDataEventArgs e)
        {
            e.Writer.WriteAttributeString("mogens", "egypten");
        }

        private void ParseCommandLineOptions()
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            var commandline = Environment.GetCommandLineArgs().Skip(1).ToArray();
            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, commandline, sw);
            // Print output from the command line parsing, and skip empty lines.
            foreach (var line in sb.ToString().Split(System.Environment.NewLine))
            {
                //if (!String.IsNullOrWhiteSpace(line)) ConsoleWriteLine(line);
            }


            if (!String.IsNullOrEmpty(m_commandLineOptions.InputFile))
            {
                this.OpenMainFile(
                    m_commandLineOptions.InputFile,
                    m_commandLineOptions.TargetElement,
                    m_commandLineOptions.TargetModel,
                    m_commandLineOptions.TargetInstance,
                    (m_commandLineOptions.Arguments != null) ? m_commandLineOptions.Arguments.ToList() : null);
            }
            else
            {
                // TODO: Check if unexpected arguments, and report error if any.
            }
        }

        #region User Settings Persistance

        private void SaveUserSettingsOnProject()
        {
            if (m_file == null) return;

            var shortcuts = new List<ProjectUserData.Shortcut>();
            foreach (var shortcut in toolStripMain.Items.Cast<ToolStripItem>().Where(o => object.Equals(m_userShortcutItemTag, o.Tag)))
            {
                if (shortcut is ScriptExecutionToolStripMenuItem)
                {
                    var typed = shortcut as ScriptExecutionToolStripMenuItem;
                    var shortcutData = new ProjectUserData.ScriptElementShortcut();
                    shortcutData.Text = typed.Text;
                    shortcutData.Element = typed.FileElement;
                    shortcutData.Partner = typed.Partner;
                    shortcutData.Instance = typed.InstanceObject;
                    shortcuts.Add(shortcutData);
                }
                else if (shortcut is ObjectCommandToolStripMenuItem)
                {
                    var typed = shortcut as ObjectCommandToolStripMenuItem;
                    var shortcutData = new ProjectUserData.ObjectCommandShortcut();
                    shortcutData.Text = typed.Text;
                    shortcutData.Instance = typed.Instance;
                    shortcutData.Command = typed.Command;
                    shortcuts.Add(shortcutData);
                }
            }
            m_userDataProject.SaveShortcuts(shortcuts);

            var hiddenToolbars = panelCustomToolstrips.ListHiddenToolbars().ToArray();
            m_userDataProject.SaveElementSettingValue(ProjectUserData.ELEMENT_TOOLBARS, ProjectUserData.ELEMENT_TOOLBARS_HIDDEN, (hiddenToolbars.Length > 0) ? hiddenToolbars : null);
        }

        private void LoadUserSettingsOnProject()
        {
            if (!m_userFileRead && m_userDataProject.FileRead)
            {
                m_userFileRead = true;
                if (m_userDataProject.AnyShortcuts())
                {
                    foreach (var shortcut in m_userDataProject.ListShortcuts())
                    {
                        if (shortcut is ProjectUserData.ScriptElementShortcut)
                        {
                            var typed = shortcut as ProjectUserData.ScriptElementShortcut;
                            var found = m_fileElements.FirstOrDefault(e => String.Equals(typed.Element, e.FullName));
                            if (found != null)
                            {
                                var isPartnerModel = false;
                                if (!String.IsNullOrEmpty(typed.Partner))
                                {
                                    var partner = found.ListPartners().FirstOrDefault(p => p.Name == typed.Partner);
                                    if (partner != null)
                                    {
                                        isPartnerModel = partner.IsModel;
                                    }
                                    else
                                    {
                                        continue;   // Just throw away, then.
                                    }
                                }
                                this.AddProcedureShortcut(typed.Text, found.FullName, typed.Partner, isPartnerModel, typed.Instance);
                            }
                        }
                        else if (shortcut is ProjectUserData.ObjectCommandShortcut)
                        {
                            var typed = shortcut as ProjectUserData.ObjectCommandShortcut;
                            this.AddObjectCommandShortcut(typed.Text, typed.Instance, typed.Command);
                        }
                    }
                }

                var hiddenToolbars = (string[])m_userDataProject.TryGetElementSetting(ProjectUserData.ELEMENT_TOOLBARS, ProjectUserData.ELEMENT_TOOLBARS_HIDDEN);
                if (hiddenToolbars != null)
                {
                    foreach (var hidden in hiddenToolbars)
                    {
                        panelCustomToolstrips.SetToolbarVisibility(hidden, false);
                    }
                }
            }
        }

        #endregion


        #region ICoreAccess

        public IExecutionAccess StartExecution(string elementName, string partner, string objectVariable, object[] args)
        {
            if (String.IsNullOrEmpty(elementName))
            {
                m_mainLogger.LogError("Internal Error: File element name is empty.");
                return null;
            }
            var element = StepBroMain.TryFindFileElement(elementName);
            if (element != null)
            {
                return this.StartExecution(false, element, partner, objectVariable, args);
            }
            else
            {
                m_mainLogger.LogError("Could not find file element '" + elementName + "'.");
                return null;
            }
        }

        public bool ExecutionRunning
        {
            get
            {
                return m_executionQueue.Count > 1 || (m_executionQueue.Count == 1 && !m_executionQueue.Peek().State.HasEnded());
            }
        }

        public void ExecuteObjectCommand(string objectVariable, string command)
        {
            m_mainLogger.LogUserAction($"Request run '{objectVariable}' command \"{command}\"");
            var obj = m_objectManager.TryFindObject<ITextCommandInput>(objectVariable);
            if (obj != null && obj.AcceptingCommands())
            {
                obj.ExecuteCommand(command);
            }
            else
            {
                string errorMessage;
                if (obj != null)
                {
                    errorMessage = $"'{objectVariable}' is not accepting commands. Did you forget to open or connect?";
                }
                else
                {
                    errorMessage = $"No command object named'{objectVariable}' found. Is the name wrong or is the object not text command enabled?";
                }

                m_mainLogger.LogError(errorMessage);
            }
        }


        #endregion

        #region Main Toolbar

        #region Main Menu

        #region File Menu

        private void toolStripMenuItemFile_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemFileOpen.Enabled = (m_file == null);
        }

        private void toolStripMenuItemFileOpen_Click(object sender, EventArgs e)
        {
            this.UserOpenFileDialog();
        }

        private void UserOpenFileDialog()
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (var openProjectDialog = new DialogOpenProjectFile())
            {
                if (openProjectDialog.ShowDialog(this) == DialogResult.OK)
                {
                    m_mainLogger.LogUserAction($"Open file \"{openProjectDialog.SelectedFile}\"");

                    this.OpenMainFile(openProjectDialog.SelectedFile);
                }
                UserDataStationManager.SaveUserSettingsOnStation();     // Just in case anything has changed.
            }

            //using (var openFileDialog = new OpenFileDialog())
            //{
            //    openFileDialog.InitialDirectory = System.Environment.CurrentDirectory;
            //    openFileDialog.Filter = "StepBro Script files (*.sbs)|*.sbs|All files (*.*)|*.*";
            //    openFileDialog.FilterIndex = 1;
            //    openFileDialog.CheckFileExists = true;
            //    openFileDialog.RestoreDirectory = true;

            //    if (openFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        m_mainLogger.LogUserAction($"Open file \"{openFileDialog.FileName}\"");

            //        this.OpenMainFile(openFileDialog.FileName);
            //    }
            //}
        }

        private void OpenMainFile(string file, string targetElement = null, string targetModel = null, string targetInstance = null, List<string> arguments = null)
        {
            if (!String.IsNullOrEmpty(file) && File.Exists(file))
            {
                m_targetFile = file;
                this.StartScriptFileLoading();
                this.StartFileParsing();

                if (!String.IsNullOrEmpty(targetElement))
                {
                    var executionData = new ScriptExecutionData(
                        this,
                        null,
                        targetModel,
                        targetInstance,
                        null);
                    executionData.ElementName = m_commandLineOptions.TargetElement; // To be resolved.
                    if (arguments != null)
                    {
                        executionData.UnparsedArguments = arguments;
                    }
                    this.StartScriptExecution(executionData);
                }
            }
        }


        private void toolStripMenuItemFileSave_Click(object sender, EventArgs e)
        {
            if (dockManager.SelectedDocument is DocumentWindow window &&
                window.Controls[0] is FastColoredTextBoxNS.FastColoredTextBox editor &&
                editor.IsChanged &&
                !String.IsNullOrEmpty(window.FileName))
            {
                try
                {
                    File.WriteAllText(window.FileName, editor.Text);
                    editor.SetChanged(false);
                    window.Modified = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        $"Failed to save \"{window.FileName}\"\r\n\r\nDetails:\r\n{ex.GetType().Name}\r\n{ex.Message}",
                        "Save File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }


        private void toolStripMenuItemCreateDocumentationFiles_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemCreateDocForSelectedFile.Enabled =
                (dockManager.SelectedDocument is DocumentWindow window &&
                window.Controls[0] is FastColoredTextBoxNS.FastColoredTextBox editor &&
                dockManager.SelectedDocument.Tag is ILoadedFile file);
        }

        private void toolStripMenuItemCreateProjectOverview_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItemCreateDocForSelectedFile_Click(object sender, EventArgs e)
        {
            if (dockManager.SelectedDocument is DocumentWindow window &&
                window.Controls[0] is FastColoredTextBoxNS.FastColoredTextBox editor &&
                dockManager.SelectedDocument.Tag is IScriptFile file)
            {
                file.GenerateDocumentationFile();
            }
        }

        private void toolStripMenuItemSaveLogNow_Click(object sender, EventArgs e)
        {
            var folder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StepBro Log Files");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var dialog = new DialogSaveExecutionLog(folder);
            var result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                OutputFormatOptions options = new OutputFormatOptions { CreateHighLevelLogSections = true, UseLocalTime = true };

                var filename = "Log" + DateTime.Now.ToFileName() + "." + m_outputAddon.LogFileExtension;
                var filepath = Path.Combine(folder, filename);

                using (StreamWriter fileStream = new StreamWriter(filepath, false))
                {
                    using (var writer = new TextFileWriter(fileStream))
                    {
                        m_outputFormatter = m_outputAddon.Create(options, writer);

                        var logEntry = StepBroMain.Logger.GetFirst().Item2;
                        var zeroTime = logEntry.Timestamp;
                        while (logEntry != null)
                        {
                            m_outputFormatter.WriteLogEntry(logEntry, zeroTime);
                            logEntry = logEntry.Next;
                        }
                    }
                }
            }
        }

        #endregion

        #region View Menu

        private void toolStripMenuItemView_DropDownOpened(object sender, EventArgs e)
        {
            viewExecutionLogToolStripMenuItem.Checked = m_toolWindowExecutionLog.Active;
            viewErrorsToolStripMenuItem.Checked = m_toolWindowExecutionLog.Active;
            //viewPropertiesToolStripMenuItem.Checked = toolWindowProperties.Active;
        }

        private void viewExecutionLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!viewExecutionLogToolStripMenuItem.Checked)
            {
                m_toolWindowExecutionLog.Activate();
            }
            else
            {
                m_toolWindowExecutionLog.Close();
            }
        }

        private void viewErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!viewErrorsToolStripMenuItem.Checked)
            {
                m_toolWindowParsingErrors.Activate();
            }
            else
            {
                m_toolWindowParsingErrors.Close();
            }
        }

        private void viewReportOverviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_toolWindowReportOverview.Activate();
        }

        private void viewObjectCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void viewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!viewPropertiesToolStripMenuItem.Checked)
            {
                //toolWindowProperties.Activate();
            }
            else
            {
                //toolWindowProperties.Close();
            }
        }

        private void viewSelectionDocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_toolWindowDocCommentPreview.Activate();
        }

        private void viewToolbarsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            viewToolbarsToolStripMenuItem.DropDownItems.Clear();
            foreach (var toolbar in panelCustomToolstrips.ListToolbars())
            {
                var visibilityMenuItem = new ToolStripMenuItem();
                visibilityMenuItem.CheckOnClick = true;
                visibilityMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
                visibilityMenuItem.Size = new Size(180, 22);
                visibilityMenuItem.Text = toolbar.Name;
                visibilityMenuItem.Checked = !panelCustomToolstrips.IsToolbarHidden(toolbar.Name);
                visibilityMenuItem.CheckedChanged += (s, e) => { panelCustomToolstrips.SetToolbarVisibility(((ToolStripMenuItem)s).Text, ((ToolStripMenuItem)s).Checked); };
                visibilityMenuItem.Tag = toolbar;
                viewToolbarsToolStripMenuItem.DropDownItems.Insert(0, visibilityMenuItem);
            }
        }

        #endregion

        #region Help Menu

        private void viewDocumentationBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //toolWindowHelp.Activate();
        }

        #endregion

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Run Button and DropDown

        private void SetFocusedFileElement(IFileElement element)
        {
            if (!Object.ReferenceEquals(element, m_focusElement))
            {
                m_focusElement = element;
            }
        }

        private void toolStripTextBoxRunSearch_TextChanged(object sender, EventArgs e)
        {
            this.UpdateFileElementExecutionSearchResult();
        }

        private void toolStripTextBoxRunSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                // Make the textbox loose focus, to make the drop down items detect mouse hovering.
                toolStripTextBoxRunSearch.Enabled = false;
                toolStripTextBoxRunSearch.Enabled = true;
            }
        }

        private void toolStripSplitButtonRunScript_DropDownOpening(object sender, EventArgs e)
        {
            if (!Object.Equals(toolStripSplitButtonRunScript.DropDownItems[0], toolStripMenuItemRunByNamespace))
            {
                toolStripSplitButtonRunScript.DropDownItems.RemoveAt(0);
            }

            if (m_focusElement is IFileProcedure focusProc)
            {
                var entry = this.CreateProcedureShortcutMenu(focusProc);
                toolStripSplitButtonRunScript.DropDownItems.Insert(0, entry);
            }
            if (m_focusElement is ITestList focusTestList)
            {
                var entry = this.CreateTestListShortcutMenu(focusTestList);
                toolStripSplitButtonRunScript.DropDownItems.Insert(0, entry);
            }

            this.UpdateFileElementExecutionSearchResult(); // In case files have changed.
        }

        private void UpdateFileElementExecutionSearchResult()
        {
            var s = toolStripTextBoxRunSearch.Text;
            int first = toolStripSplitButtonRunScript.DropDownItems.IndexOf(toolStripTextBoxRunSearch) + 1;
            while (toolStripSplitButtonRunScript.DropDownItems.Count > first)
            {
                toolStripSplitButtonRunScript.DropDownItems.RemoveAt(first);
            }

            if (m_fileElements != null && toolStripTextBoxRunSearch.Text.Length > 2)
            {
                var matches = m_fileElements.Where(
                    e =>
                        (e.ElementType == FileElementType.ProcedureDeclaration || e.ElementType == FileElementType.TestList) &&
                        e.Name.Contains(s, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (matches.Count > 0)
                {
                    var startExactMatches = new List<IFileElement>();
                    var exactMatches = new List<IFileElement>();
                    var startMatches = new List<IFileElement>();
                    var otherExactMatches = new List<IFileElement>();
                    foreach (var item in matches)
                    {
                        if (item.Name.StartsWith(s, StringComparison.InvariantCulture))
                        {
                            startExactMatches.Add(item);
                        }
                        else if (item.Name.Contains(s, StringComparison.InvariantCulture))
                        {
                            exactMatches.Add(item);
                        }
                        else if (item.Name.StartsWith(s, StringComparison.InvariantCultureIgnoreCase))
                        {
                            startMatches.Add(item);
                        }
                        else
                        {
                            otherExactMatches.Add(item);
                        }
                    }

                    var sortedMatches = new List<IFileElement>();
                    sortedMatches.AddRange(startExactMatches);
                    sortedMatches.AddRange(exactMatches);
                    sortedMatches.AddRange(startMatches);
                    sortedMatches.AddRange(otherExactMatches);

                    // TODO: Make dropdown list update entries on hover, as the user would expect.
                    foreach (var element in sortedMatches)
                    {
                        if (element.ElementType == FileElementType.ProcedureDeclaration)
                        {
                            var entry = this.CreateProcedureShortcutMenu(element as IFileProcedure);
                            toolStripSplitButtonRunScript.DropDownItems.Add(entry);
                        }
                        else if (element.ElementType == FileElementType.TestList)
                        {
                            var entry = this.CreateTestListShortcutMenu(element as ITestList);
                            toolStripSplitButtonRunScript.DropDownItems.Add(entry);
                        }
                    }
                }
            }
        }

        ToolStripMenuItem CreateProcedureShortcutMenu(IFileProcedure procedure)
        {
            ToolStripMenuItem outputItem = null;
            var partners = procedure.ListPartners().ToList();
            var thisVariables = new List<string>();

            if (procedure.Parameters.Length > 0 && procedure.IsFirstParameterThisReference)
            {
                var par = procedure.Parameters[0];
                foreach (var v in m_objectManager.GetObjectCollection())
                {
                    if (par.Value.IsAssignableFrom(m_variableTypes[v.FullName]))
                    {
                        thisVariables.Add(v.FullName);
                    }
                }
            }

            if (partners.Count > 0 || thisVariables.Count > 0)
            {
                outputItem = new ToolStripMenuItem();
                outputItem.Name = "toolStripMenuProcedure" + procedure.Name;
                outputItem.Size = new Size(182, 22);
                outputItem.Text = procedure.Name;
                outputItem.ToolTipText = null; // $"Procedure '{procedure.FullName}'";

                if (partners != null && partners.Count > 0)
                {
                    var options = new List<IPartner>(partners);
                    options.Insert(0, null); // Add the 'no partner' option.
                    foreach (var partner in options)
                    {
                        var procedureExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(procedure, (partner != null) ? partner.Name : null, null);
                        procedureExecutionOptionMenu.Size = new Size(182, 22);
                        if (partner != null)
                        {
                            procedureExecutionOptionMenu.Name = "toolStripMenuProcedure" + procedure.Name + "Dot" + partner.Name;
                            if (partner.IsModel)
                            {
                                procedureExecutionOptionMenu.Text = procedure.Name + " using '" + partner.Name + "'";
                            }
                            else
                            {
                                procedureExecutionOptionMenu.Text = procedure.Name + "." + partner.Name;
                            }
                            procedureExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}' partner '{partner.Name}'";
                            var partnerProcedure = m_fileElements.Where(e => e is IFileProcedure).FirstOrDefault(p => p.FullName == partner.ProcedureReference.FullName) as IFileProcedure;
                            if (partnerProcedure == null ||
                                (partnerProcedure.Parameters != null && partnerProcedure.Parameters.Length > ((partnerProcedure.IsFirstParameterThisReference) ? 1 : 0)))   // TODO: Check whether that first parameter is the parent procedure.
                            {
                                procedureExecutionOptionMenu.Enabled = false;
                            }
                        }
                        else
                        {
                            procedureExecutionOptionMenu.Name = "toolStripMenuProcedureOptionDirect" + procedure.Name;
                            procedureExecutionOptionMenu.Text = procedure.Name;
                            procedureExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}'";
                            if (procedure.Parameters != null && procedure.Parameters.Length > 0)
                            {
                                procedureExecutionOptionMenu.Enabled = false;
                            }
                        }
                        procedureExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                        outputItem.DropDownItems.Add(procedureExecutionOptionMenu);
                    }
                }
                else if (thisVariables.Count > 0)
                {
                    foreach (var variable in thisVariables)
                    {
                        string shortName = variable.Split('.').Last();

                        var procedureExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(procedure, null, variable);
                        procedureExecutionOptionMenu.Size = new Size(182, 22);
                        procedureExecutionOptionMenu.Name = "toolStripMenuProcedure" + procedure.Name + "On" + variable.Replace(".", "Dot");
                        procedureExecutionOptionMenu.SetText();
                        procedureExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}' partner '{partner.Name}'";
                        procedureExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                        if (procedure.Parameters == null || procedure.Parameters.Length > 1)     // TODO: Enable user to input the arguments.
                        {
                            procedureExecutionOptionMenu.Enabled = false;
                        }
                        outputItem.DropDownItems.Add(procedureExecutionOptionMenu);
                    }
                }
                else
                {
                    var executionItem = outputItem as ScriptExecutionToolStripMenuItem;
                    executionItem.FileElement = procedure.FullName;
                    executionItem.SetText();
                    if (procedure.Parameters != null && procedure.Parameters.Length > 0)     // TODO: Enable user to input the arguments.
                    {
                        executionItem.Enabled = false;
                    }
                    executionItem.Click += FileElementExecutionEntry_Click;     // The procedure itself; not a partner or any instance variable.
                }
            }
            else
            {
                // No partners or instance object, just the direct procedure call.

                var procedureMenu = new ScriptExecutionToolStripMenuItem(procedure, null, null);
                procedureMenu.Size = new Size(182, 22);
                procedureMenu.SetText();
                procedureMenu.Name = "toolStripMenuProcedure" + procedure.FullName;
                procedureMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}'";
                if (procedure.Parameters != null && procedure.Parameters.Length > 0)
                {
                    procedureMenu.Enabled = false;
                }
                procedureMenu.Click += FileElementExecutionEntry_Click;
                outputItem = procedureMenu;
            }
            return outputItem;
        }

        ToolStripMenuItem CreateTestListShortcutMenu(ITestList testlist)
        {
            ToolStripMenuItem outputItem = null;
            var partners = testlist.ListPartners().ToList();

            if (partners.Count > 0)
            {
                outputItem = new ToolStripMenuItem();
                outputItem.Name = "toolStripMenuTestList" + testlist.Name;
                outputItem.Size = new Size(182, 22);
                outputItem.Text = testlist.Name;
                outputItem.ToolTipText = null; // $"Procedure '{procedure.FullName}'";

                if (partners != null && partners.Count > 0)
                {
                    foreach (var partner in partners)
                    {
                        var testlistExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(testlist, (partner != null) ? partner.Name : null, null);
                        testlistExecutionOptionMenu.Size = new Size(182, 22);
                        if (partner != null)
                        {
                            testlistExecutionOptionMenu.Name = "toolStripMenuTestList" + testlist.Name + "Dot" + partner.Name;
                            //procedureExecutionOptionMenu.Text = testlist.Name + "." + partner.Name;
                            testlistExecutionOptionMenu.PartnerIsModel = partner.IsModel;
                            testlistExecutionOptionMenu.SetText();
                            testlistExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}' partner '{partner.Name}'";
                            var partnerProcedure = partner.ProcedureReference; //m_fileElements.Where(e => e is ITestList).FirstOrDefault(p => p.FullName == partner.ProcedureReference.FullName) as IFileProcedure;
                            if (partnerProcedure == null ||
                                (partnerProcedure.Parameters != null && partnerProcedure.Parameters.Length > ((partnerProcedure.IsFirstParameterThisReference) ? 1 : 0)))   // TODO: Check whether that first parameter is the parent procedure.
                            {
                                testlistExecutionOptionMenu.Enabled = false;
                            }
                        }
                        testlistExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                        outputItem.DropDownItems.Add(testlistExecutionOptionMenu);
                    }
                }
            }
            return outputItem;
        }

        private void FileElementExecutionEntry_Click(object sender, EventArgs e)
        {
            var executionEntry = sender as ScriptExecutionToolStripMenuItem;
            var element = StepBroMain.TryFindFileElement(executionEntry.FileElement);
            if (element != null)
            {
                this.StartExecution(true, element, executionEntry.Partner, executionEntry.InstanceObject, null);
            }
        }

        private void toolStripSplitButtonRunScript_ButtonClick(object sender, EventArgs e)
        {
            if (toolStripSplitButtonRunScript.Tag != null)
            {
                var first = toolStripSplitButtonRunScript.DropDownItems.IndexOf(toolStripSeparatorRunBeforeHistory) + 1;
                var executionEntry = toolStripSplitButtonRunScript.DropDownItems[first] as ScriptExecutionToolStripMenuItem;
                var element = StepBroMain.TryFindFileElement(executionEntry.FileElement);
                if (element != null)
                {
                    this.StartExecution(true, element, executionEntry.Partner, executionEntry.InstanceObject, null);
                }
                else
                {
                    // TODO: Log the error or dialog?
                }
            }
        }

        private int GetIndexOfTopHistoryElement()
        {
            return toolStripSplitButtonRunScript.DropDownItems.IndexOf(toolStripSeparatorRunBeforeHistory) + 1;
        }

        private IEnumerable<ScriptExecutionToolStripMenuItem> ListHistoryEntries()
        {
            var index = this.GetIndexOfTopHistoryElement();
            int historyItemsCount = (toolStripSplitButtonRunScript.Tag != null) ? (int)toolStripSplitButtonRunScript.Tag : 0;
            if (historyItemsCount > 0)
            {
                for (int i = 0; i < historyItemsCount; i++)
                {
                    var exeItem = toolStripSplitButtonRunScript.DropDownItems[index] as ScriptExecutionToolStripMenuItem;
                    if (exeItem == null) break;     // Stop here...
                    yield return exeItem;
                    index++;
                }
            }
        }

        private void AddElementExecutionToHistory(IFileElement element, string partner, bool partnerIsModel, string objectVariable, object[] args)
        {
            var title = ScripExecutionButtonTitle(false, element.FullName, partner, partnerIsModel, objectVariable, args);

            var first = this.GetIndexOfTopHistoryElement();
            ScriptExecutionToolStripMenuItem found = null;
            int historyItemsCount = (toolStripSplitButtonRunScript.Tag != null) ? (int)toolStripSplitButtonRunScript.Tag : 0;
            if (historyItemsCount > 0)
            {
                var index = first;
                for (int i = 0; i < historyItemsCount; i++)
                {
                    var exeItem = toolStripSplitButtonRunScript.DropDownItems[index] as ScriptExecutionToolStripMenuItem;
                    if (exeItem == null) break;     // Stop here...
                    if (exeItem.Equals(element.FullName, partner, objectVariable))
                    {
                        found = exeItem;
                        toolStripSplitButtonRunScript.DropDownItems.RemoveAt(index);    // Remove it (to be inserted at the top).
                        historyItemsCount--;
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            toolStripSeparatorRunAfterHistory.Visible = true;

            if (found == null)
            {
                found = new ScriptExecutionToolStripMenuItem();
                found.FileElement = element.FullName;
                found.Partner = partner;
                found.PartnerIsModel = partnerIsModel;
                found.InstanceObject = objectVariable;
                found.ShowFullName = false;
                found.SetText();
                found.Tag = new object();
                found.Click += FileElementExecutionEntry_Click;
            }

            historyItemsCount++;
            toolStripSplitButtonRunScript.Text = found.Text;
            toolStripSplitButtonRunScript.DropDownItems.Insert(first, found);   // Insert (or re-insert) at the top of the history list.
            if (historyItemsCount > 25)
            {
                toolStripSplitButtonRunScript.DropDownItems.RemoveAt(first + historyItemsCount);
                historyItemsCount--;
            }
            toolStripSplitButtonRunScript.Tag = historyItemsCount;
            toolStripButtonAddShortcut.Enabled = true;
        }

        #endregion

        private void toolStripButtonStopScriptExecution_Click(object sender, EventArgs e)
        {
            if (m_executionQueue.Count > 0)
            {
                m_executionQueue.Peek().RequestStopExecution();
                toolStripButtonStopScriptExecution.Enabled = false;
            }
        }

        #region USER INTERACTION - COMMANDS


        private void toolStripDropDownButtonTool_DropDownOpening(object sender, EventArgs e)
        {
            //this.UpdateCommandObjectMenu_Objects();
        }

        private void toolStripDropDownButtonTool_DropDownOpened(object sender, EventArgs e)
        {
            toolStripDropDownButtonTool.HideDropDown();
        }

        private void toolStripDropDownButtonTool_Click(object sender, EventArgs e)
        {
            var dialog = new ToolInteractionPopup();
            dialog.DataContext = m_toolsInteractionModel;
            dialog.Location = toolStripDropDownButtonTool.GetCurrentParent().PointToScreen(new Point(toolStripDropDownButtonTool.Bounds.X + 30, toolStripDropDownButtonTool.Bounds.Bottom));
            dialog.Show();
        }

        private void UpdateToolButton()
        {
            m_toolsInteractionModel.Synchronize();
            //if (m_toolsMenuModel.CurrentTextCommandTool != null)
            //{
            //    toolStripDropDownButtonTool.Text = m_toolsMenuModel.CurrentTextCommandTool.PresentationName;
            //    toolStripComboBoxTool.Visible = true;
            //}
            //else
            //{
            //    toolStripDropDownButtonTool.Text = "Tool";
            //    toolStripComboBoxTool.Visible = false;
            //}

            //var selectedTool = toolStripComboBoxTool.SelectedItem as ComboboxItem;
            //toolStripComboBoxTool.Items.Clear();
            //int selection = 0;
            //int index = 0;

            //foreach (var v in objects)
            //{
            //    if (v.Object is ITextCommandInput)
            //    {
            //        var name = v.FullName.Split('.').Last();
            //        toolStripComboBoxTool.Items.Add(new ComboboxItem(name, v));
            //        if (selectedTool != null && name == selectedTool.Text)
            //        {
            //            selection = index;
            //        }
            //        index++;
            //    }
            //}
            //if (toolStripComboBoxTool.Items.Count > 0)
            //{
            //    toolStripComboBoxTool.Enabled = true;
            //    toolStripComboBoxTool.SelectedIndex = selection;
            //    toolStripComboBoxTool.SelectionLength = 0;
            //}
            //else
            //{
            //    toolStripComboBoxTool.Enabled = false;
            //    toolStripComboBoxToolCommand.Visible = false;
            //}

        }

        private void ToolsMenuModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(m_toolsInteractionModel.CurrentTextCommandTool))
            {
                if (m_toolsInteractionModel.CurrentTextCommandTool != null)
                {
                    toolStripDropDownButtonTool.Text = m_toolsInteractionModel.CurrentTextCommandTool.PresentationName;
                    toolStripDropDownButtonTool.ToolTipText = "Selected tool/object: " + m_toolsInteractionModel.CurrentTextCommandTool.PresentationFullName;
                    toolStripComboBoxToolCommand.Visible = true;
                    toolStripComboBoxToolCommand.Enabled = true;
                }
                else
                {
                    toolStripDropDownButtonTool.Text = "Tool";
                    toolStripComboBoxToolCommand.Visible = false;
                    toolStripComboBoxToolCommand.Enabled = false;
                }
            }
        }

        //private void UpdateCommandObjectMenu_Objects()
        //{
        //    var index = toolStripDropDownButtonTool.DropDownItems.IndexOf(toolStripMenuItemNoToolsYet) + 1;
        //    while (index < toolStripDropDownButtonTool.DropDownItems.Count)
        //    {
        //        toolStripDropDownButtonTool.DropDownItems.RemoveAt(index);
        //    }

        //    // Update list of variables containing objects with the ITextCommandInput interface.
        //    var objects = m_objectManager.GetObjectCollection();
        //    var commandObjectsContainers = objects.Where(o => o.Object is ITextCommandInput).ToList();
        //    if (commandObjectsContainers.Count > 0)
        //    {
        //        toolStripMenuItemNoToolsYet.Visible = false;
        //        foreach (var o in commandObjectsContainers)
        //        {
        //            var item = new ToolStripMenuItem();
        //            item.Name = "toolStripMenuItem_" + o.FullName.Replace('.', '_');
        //            item.Size = new Size(206, 22);
        //            item.Text = o.FullName.Split('.').Last();
        //            item.Tag = o;
        //            item.Click += toolStripMenuItemToolSelection_Click;
        //            item.Checked = Object.ReferenceEquals(o, toolStripDropDownButtonTool.Tag);
        //            toolStripDropDownButtonTool.DropDownItems.Add(item);
        //        }
        //    }
        //    else
        //    {
        //        toolStripMenuItemNoToolsYet.Visible = true;
        //    }
        //}

        //private void UpdateCommandObjectMenu_Commands()
        //{
        //    while (true)
        //    {
        //        if (toolStripDropDownButtonTool.DropDownItems[0] is ToolStripSeparator)
        //        {
        //            break;
        //        }
        //        toolStripDropDownButtonTool.DropDownItems.RemoveAt(0);
        //    }

        //    IObjectContainer tool = toolStripDropDownButtonTool as IObjectContainer;

        //    toolStripSeparatorToolSelection.Visible = false;

        //    //foreach (var o in commandObjectsContainers)
        //    //{
        //    //    m_commandObjectDictionary[o.FullName] = o.Object as ITextCommandInput;    // Add or override.

        //    //    var item = new ToolStripMenuItem();
        //    //    item.BackColor = Color.RosyBrown;
        //    //    item.Name = "toolStripMenuItem2";
        //    //    item.Size = new Size(206, 22);
        //    //    item.Text = "tool command";
        //    //    item.ToolTipText = "Script procedure \"Bla\"";
        //    //    item.Click += toolStripMenuItemToolCommandExecute_Click;

        //    //}

        //}

        //private void toolStripMenuItemToolSelection_Click(object sender, EventArgs e)
        //{
        //    var menu = (ToolStripMenuItem)sender;
        //    if (!Object.ReferenceEquals(menu.Tag, toolStripDropDownButtonTool.Tag))
        //    {
        //        toolStripDropDownButtonTool.Tag = menu.Tag;
        //        toolStripDropDownButtonTool.Text = (menu.Tag as IObjectContainer).FullName.Split('.').Last();
        //        toolStripDropDownButtonTool.ToolTipText = "Selected tool/object: " + (menu.Tag as IObjectContainer).FullName;
        //        toolStripComboBoxToolCommand.Visible = true;
        //        toolStripComboBoxToolCommand.Enabled = true;
        //    }
        //}

        //private void toolStripMenuItemToolCommandExecute_Click(object sender, EventArgs e)
        //{

        //}

        //private void toolStripComboBoxTool_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    toolStripComboBoxTool.ToolTipText =
        //        "Select tool/object to use for the command prompt. Selected object: '" +
        //        ((IObjectContainer)(toolStripComboBoxTool.SelectedItem as ComboboxItem).Value).FullName + "'";
        //}

        private void toolStripComboBoxToolCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                this.ExecuteCommandFromGUI();
            }
        }

        private void toolStripComboBoxToolCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_settingCommandCombo) return;
        }

        private void toolStripComboBoxToolCommand_TextChanged(object sender, EventArgs e)
        {
            toolStripButtonRunCommand.Enabled = !String.IsNullOrEmpty(toolStripComboBoxToolCommand.Text);
        }

        private void toolStripButtonRunCommand_Click(object sender, EventArgs e)
        {
            this.ExecuteCommandFromGUI();
        }

        private void ExecuteCommandFromGUI()
        {
            if (!String.IsNullOrEmpty(toolStripComboBoxToolCommand.Text))
            {
                string command = toolStripComboBoxToolCommand.Text;
                this.ExecuteCommand(command);
                m_settingCommandCombo = true;
                int i = 0;
                foreach (string known in toolStripComboBoxToolCommand.Items)
                {
                    if (string.Equals(command, known))
                    {
                        toolStripComboBoxToolCommand.Items.RemoveAt(i);
                        break;
                    }
                    i++;
                }
                toolStripComboBoxToolCommand.Items.Insert(0, command);
                toolStripComboBoxToolCommand.SelectedIndex = 0;
                toolStripComboBoxToolCommand.Select(0, command.Length);
                m_settingCommandCombo = false;
            }
        }

        private void ExecuteCommand(string command)
        {
            var container = m_toolsInteractionModel.CurrentTextCommandTool.ToolContainer;
            this.ExecuteObjectCommand(container.FullName, command);
        }

        #endregion

        #region Execution Shortcuts

        private void toolStripButtonAddShortcut_Click(object sender, EventArgs e)
        {
            bool procAvailable = toolStripSplitButtonRunScript.Tag != null;
            bool commandAvailable = !String.IsNullOrEmpty(toolStripComboBoxToolCommand.Text);
            if (procAvailable || commandAvailable)
            {
                ScriptExecutionToolStripMenuItem executionEntry = null;
                string procDescription = "";
                string procButtonText = "";
                string commandDescription = "";
                string commandButtonText = "";

                if (procAvailable)
                {
                    var index = GetIndexOfTopHistoryElement();
                    executionEntry = toolStripSplitButtonRunScript.DropDownItems[index] as ScriptExecutionToolStripMenuItem;
                    procButtonText = ScripExecutionButtonTitle(false, executionEntry.FileElement, executionEntry.Partner, executionEntry.PartnerIsModel, executionEntry.InstanceObject, null);
                    procDescription = procButtonText;
                }
                if (commandAvailable)
                {
                    commandButtonText = toolStripComboBoxToolCommand.Text;
                    commandDescription = "On " + m_toolsInteractionModel.CurrentTextCommandTool.PresentationName + ": " + commandButtonText;
                }

                var dialog = new DialogAddShortcut("Adding Shortcut", procDescription, commandDescription, procButtonText, commandButtonText);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.ProcedureExecutionSelected)
                    {
                        this.AddProcedureShortcut(
                            dialog.ButtonText,
                            executionEntry.FileElement,
                            executionEntry.Partner,
                            executionEntry.PartnerIsModel,
                            executionEntry.InstanceObject);
                    }
                    else
                    {
                        var tool = m_toolsInteractionModel.CurrentTextCommandTool.ToolContainer.FullName;
                        this.AddObjectCommandShortcut(dialog.ButtonText, tool, toolStripComboBoxToolCommand.Text);
                    }
                }
            }
        }

        private void AddProcedureShortcut(string text, string element, string partner, bool partnerIsModel, string instanceObject)
        {
            var shortcut = new ScriptExecutionToolStripMenuItem();
            shortcut.Text = text;
            shortcut.FileElement = element;
            shortcut.Partner = partner;
            shortcut.PartnerIsModel = partnerIsModel;
            shortcut.InstanceObject = instanceObject;
            shortcut.Name = "toolStripMenuProcedure" + shortcut.Text.Replace(".", "Dot");
            shortcut.Size = new Size(182, 22);
            shortcut.Margin = new System.Windows.Forms.Padding(1, shortcut.Margin.Top, 1, shortcut.Margin.Bottom);
            shortcut.BackColor = Color.PeachPuff;
            shortcut.ToolTipText = null; // $"Run " + target;
            shortcut.Tag = m_userShortcutItemTag;
            shortcut.Click += FileElementExecutionEntry_ShortcutClick;

            toolStripMenuItemDeleteShortcut.Enabled = true;
            toolStripMenuItemDeleteAllShortcuts.Enabled = true;

            toolStripMain.Items.Add(shortcut);
        }

        private void AddObjectCommandShortcut(string text, string instance, string command)
        {
            var shortcut = new ObjectCommandToolStripMenuItem();
            shortcut.Text = text;
            shortcut.Instance = instance;
            shortcut.Command = command;
            shortcut.Name = "toolStripMenuCommand" + text.Replace(".", "Dot");
            shortcut.Size = new Size(182, 22);
            shortcut.Margin = new System.Windows.Forms.Padding(1, shortcut.Margin.Top, 1, shortcut.Margin.Bottom);
            shortcut.BackColor = Color.Lavender;
            shortcut.ToolTipText = null; // $"Run " + target;
            shortcut.Tag = m_userShortcutItemTag;
            shortcut.Click += ObjectCommandExecutionEntry_ShortcutClick;

            toolStripMenuItemDeleteShortcut.Enabled = true;
            toolStripMenuItemDeleteAllShortcuts.Enabled = true;

            toolStripMain.Items.Add(shortcut);
        }

        private void FileElementExecutionEntry_ShortcutClick(object sender, EventArgs e)
        {
            var executionEntry = sender as ScriptExecutionToolStripMenuItem;
            if (toolStripMenuItemDeleteShortcut.Checked)
            {
                var choise = MessageBox.Show(
                    this,
                    "Should the shortcut\r\n\r\n\"" + executionEntry.Text + "\"\r\n\r\nbe deleted?",
                    "StepBro - Deleting shortcut",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choise == DialogResult.Yes)
                {
                    toolStripMain.Items.Remove(executionEntry);
                }
                toolStripMenuItemDeleteShortcut.Checked = false;
            }
            else
            {
                var element = StepBroMain.TryFindFileElement(executionEntry.FileElement);
                if (element != null)
                {
                    this.StartExecution(false, element, executionEntry.Partner, executionEntry.InstanceObject, null);
                }
            }
        }

        private void ObjectCommandExecutionEntry_ShortcutClick(object sender, EventArgs e)
        {
            var executionEntry = sender as ObjectCommandToolStripMenuItem;
            if (toolStripMenuItemDeleteShortcut.Checked)
            {
                var choise = MessageBox.Show(
                    this,
                    "Should the shortcut\r\n\r\n\"" + executionEntry.Text + "\"\r\n\r\nbe deleted?",
                    "StepBro - Deleting shortcut",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choise == DialogResult.Yes)
                {
                    toolStripMain.Items.Remove(executionEntry);
                }
                toolStripMenuItemDeleteShortcut.Checked = false;
            }
            else
            {
                this.ExecuteObjectCommand(executionEntry.Instance, executionEntry.Command);
            }
        }


        private void toolStripMenuItemDeleteAllShortcuts_Click(object sender, EventArgs e)
        {

        }

        #endregion

        private void toolStripMenuItemExeNoteInput_CheckedChanged(object sender, EventArgs e)
        {
            toolStripTextBoxExeNote.Visible = toolStripMenuItemExeNoteInput.Checked;
            this.SetExtraFieldsSeparatorVisibility();
        }

        private void SetExtraFieldsSeparatorVisibility()
        {
            toolStripSeparatorExtraFields.Visible = toolStripTextBoxExeNote.Visible;
        }

        #endregion

        #region Editor

        private DocumentWindow OpenLoadedFile(ILoadedFile file)
        {
            FastColoredTextBoxNS.FastColoredTextBox editor = new FastColoredTextBoxNS.FastColoredTextBox();
            editor.Font = new Font("Consolas", 9.75f);
            editor.BorderStyle = BorderStyle.None;
            editor.ReadOnly = false;
            editor.ShowScrollBars = true;
            editor.WordWrap = false;

            DocumentWindow window = null;
            try
            {
                editor.Language = Language.StepBro;
                editor.OpenFile(file.FilePath);

                file.RegisterDependant(m_hostDependancyObject); // Used by editor now.

                window = new DocumentWindow(dockManager, file.FilePath, Path.GetFileName(file.FilePath), 2, editor);
                window.Tag = file;
                window.FileName = file.FilePath;
                window.FileType = String.Format("StepBro Script File (*{0})", Path.GetExtension(file.FilePath).ToLower());

                editor.MouseMove += Editor_MouseMove;
                editor.TextChanged += TextBox_TextChanged;
                editor.SelectionChanged += TextBox_SelectionChanged;
                editor.SelectionChangedDelayed += TextBox_SelectionChangedDelayed;
                editor.TextChangedDelayed += TextBox_TextChangedDelayed;
            }
            catch (Exception)
            {

            }

            return window;
        }

        private ILoadedFile GetCurrentDocumentFile()
        {
            if (dockManager.SelectedDocument is DocumentWindow window &&
                window.Controls[0] is FastColoredTextBoxNS.FastColoredTextBox &&
                dockManager.SelectedDocument.Tag is ILoadedFile file)
            {
                return file;
            }
            else
            {
                return null;
            }
        }

        private DocumentWindow OpenOrActivateFileEditor(ILoadedFile file, int line = -1)
        {
            DocumentWindow window = null;

            foreach (TabbedMdiWindow docWindow in dockManager.ActiveDocuments)
            {
                if (docWindow.Tag != null && Object.ReferenceEquals(file, docWindow.Tag))
                {
                    window = docWindow as DocumentWindow;
                    break;
                }
            }

            if (window == null)
            {
                // Not found; so open the file now, please!
                window = this.OpenLoadedFile(file as ILoadedFile);
                m_fileExplorer.UpdateNodeStates();
            }

            if (window != null)
            {
                window.Activate();
                window.Select();

                if (line >= 0)
                {
                    var editor = window.Controls[0] as FastColoredTextBox;
                    editor.SetSelectedLine(line);
                }
            }
            return window;
        }

        private DocumentWindow CreateTextDocument(string fileName, string text, bool readOnly)
        {
            DocumentWindow documentWindow;

            // Is the document already open?
            if (dockManager.DocumentWindows[fileName] != null)
            {
                dockManager.DocumentWindows[fileName].Activate();
                //MessageBox.Show(this, String.Format("The file '{0}' is already open.", fileName), "File Already Open", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return dockManager.DocumentWindows[fileName];
            }

            // Determine the type of file
            string fileType = "Text";
            if (fileName != null)
            {
                switch (Path.GetExtension(fileName).ToLower())
                {
                    case ".bmp":
                    case ".gif":
                    case ".ico":
                    case ".jpg":
                    case ".png":
                        fileType = "Image";
                        readOnly = true;
                        break;
                }
            }

            switch (fileType)
            {
                case "Image":
                    {
                        // Create a PictureBox for the document
                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Image = Image.FromFile(fileName);

                        // Create the document window
                        documentWindow = new DocumentWindow(dockManager, fileName, Path.GetFileName(fileName), 4, pictureBox);
                        if (fileName != null)
                        {
                            documentWindow.FileName = fileName;
                            documentWindow.FileType = String.Format("Image File (*{0})", Path.GetExtension(fileName).ToLower());
                        }
                        break;
                    }
                default:
                    {

                        FastColoredTextBoxNS.FastColoredTextBox textBoxFast = new FastColoredTextBoxNS.FastColoredTextBox();
                        textBoxFast.Font = new Font("Consolas", 9.75f);
                        textBoxFast.BorderStyle = BorderStyle.None;
                        textBoxFast.ReadOnly = readOnly;
                        textBoxFast.ShowScrollBars = true;
                        textBoxFast.WordWrap = false;
                        textBoxFast.MouseMove += Editor_MouseMove;

                        // If no data was passed in, generate some
                        if (fileName == null)
                        {
                            fileName = String.Format("Document{0}.txt", documentWindowIndex++);
                            if (text == null)
                                text = "Visit our web site to learn more about Actipro WinForms Studio or our other controls:\r\nhttps://www.actiprosoftware.com/\r\n\r\nThis document was created at " + DateTime.Now.ToString() + ".";
                        }
                        else
                        {
                            textBoxFast.OpenFile(fileName);
                        }

                        // Create the document window
                        textBoxFast.Text = text;
                        textBoxFast.TextChanged += TextBox_TextChanged;
                        documentWindow = new DocumentWindow(dockManager, fileName, Path.GetFileName(fileName), 3, textBoxFast);
                        if (fileName != null)
                        {
                            documentWindow.FileName = fileName;
                            documentWindow.FileType = String.Format("Text File (*{0})", Path.GetExtension(fileName).ToLower());
                            documentWindow.Tag = fileName;
                        }

                        documentWindow.Activate();

                        break;
                    }
            }

            if (readOnly)
            {
                // Load a read-only context image
                documentWindow.ContextImage = ActiproSoftware.Products.Docking.AssemblyInfo.Instance.GetImage(
                    ActiproSoftware.Products.Docking.ImageResource.ContextReadOnly,
                    DpiHelper.GetDpiScale(this));
            }

            return documentWindow;
        }

        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            if (tb.Selection.TextLength == 0)
            {
                System.Diagnostics.Debug.WriteLine("TextBox_SelectionChanged - cursor only");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("TextBox_SelectionChanged - selection size: " + tb.Selection.TextLength.ToString());
            }
        }

        private void TextBox_SelectionChangedDelayed(object sender, EventArgs e)
        {
            var tb = sender as FastColoredTextBox;

            if (m_toolWindowDocCommentPreview.Active && tb.Selection.IsEmpty)
            {
                try
                {
                    int line = tb.Selection.Start.iLine;
                    string lineText = tb.GetLine(line).Text;
                    if (ScriptDocumentation.IsDocLine(lineText.Trim()))
                    {
                        var lines = new List<string>();
                        lineText = lineText.Trim().TrimStart('/').TrimStart();
                        lines.Add(lineText);

                        int first = line;
                        int last = line;
                        while (first > 0)
                        {
                            string text = tb.GetLine(first - 1).Text.Trim();
                            if (ScriptDocumentation.IsDocLine(text))
                            {
                                lines.Insert(0, text.TrimStart('/').TrimStart());
                                first--;
                            }
                            else break;
                        }
                        while (last < (tb.LinesCount - 1))
                        {
                            string text = tb.GetLine(last + 1).Text.Trim();
                            if (ScriptDocumentation.IsDocLine(text))
                            {
                                lines.Add(text.TrimStart('/').TrimStart());
                                last++;
                            }
                            else break;
                        }
                        m_selectionDocView.ShowDocCommentsPreview(lines);
                        m_toolWindowDocCommentPreview.Activate(false);
                        System.Diagnostics.Debug.WriteLine($"Do update the preview; {lines.Count} lines, first: {first + 1}, last: {last + 1}");
                    }
                    else
                    {
                        var lexer = new Lexer(new AntlrInputStream(lineText));
                        var tokens = lexer.GetAllTokens();
                        int i = 0, start = -1;
                        foreach (var token in tokens)
                        {
                            if (tb.Selection.Start.iChar > token.Column && tb.Selection.Start.iChar <= (token.Column + token.Text.Length))
                            {
                                System.Diagnostics.Debug.WriteLine("Selected token: " + token.Text + " (" + token.Type.ToString() + ")");
                                start = i;
                                break;
                            }
                            i++;
                        }
                        if (start >= 0)
                        {
                            if (tokens[start].Type == Lexer.IDENTIFIER || tokens[start].Type == Lexer.AT_IDENTIFIER)
                            {
                                var end = start;
                                while (start >= 2 && tokens[start - 1].Type == Lexer.DOT && (tokens[start - 2].Type == Lexer.IDENTIFIER || tokens[start - 2].Type == Lexer.AT_IDENTIFIER))
                                {
                                    start -= 2;
                                }
                                string qualifiedIdentifier = "";
                                for (i = start; i <= end; i++)
                                {
                                    qualifiedIdentifier += tokens[i].Text;
                                }

                                System.Diagnostics.Debug.WriteLine("------------- Selected token: " + qualifiedIdentifier);
                                var symbol = m_symbolLookupService.TryResolveSymbol(this.GetCurrentDocumentFile() as IScriptFile, qualifiedIdentifier);
                                if (symbol != null)
                                {
                                    if (symbol is TypeReference tr)
                                    {
                                        symbol = tr.DynamicType ?? tr.Type;
                                    }
                                    System.Diagnostics.Debug.WriteLine("------------- Found Symbol: " + symbol.GetType().FullName);
                                    m_selectionDocView.ShowObjectDocumentation(symbol);
                                    if (symbol is IFileElement fileElement)
                                    {
                                        this.SetFocusedFileElement(fileElement);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_mainLogger.LogUserAction($"{ex.GetType().Name} in \"Selection Documentation\" view. {ex.Message}");
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            if (ignoreTextChangedEvent)
                return;
            System.Diagnostics.Debug.WriteLine("TextBox_TextChanged");

            DocumentWindow documentWindow = tb.Parent as DocumentWindow;
            if (documentWindow != null && documentWindow.Modified != tb.IsChanged)
            {
                documentWindow.Modified = tb.IsChanged;
               UpdateFromDocumentSelection();
            }
        }

        private void TextBox_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("TextBox_TextChangedDelayed");
        }

        void Editor_MouseMove(object sender, MouseEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            var place = tb.PointToPlace(e.Location);
            var r = new FastColoredTextBoxNS.Range(tb, place, place);

            string text = r.GetFragment("[a-zA-Z._]").Text;
            //lbWordUnderMouse.Text = text;
        }

        #endregion

        #region Views

        #region Docking Administration

        private void DockManager_SelectedDocumentChanged(object sender, TabbedMdiWindowEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DockManager_SelectedDocumentChanged");
            this.UpdateFromDocumentSelection();
        }

        private void UpdateFromDocumentSelection()
        {
            if (dockManager.SelectedDocument is DocumentWindow window &&
                window.Controls[0] is FastColoredTextBoxNS.FastColoredTextBox editor)
            {
                toolStripMenuItemFileSave.Enabled = editor.IsChanged;
            }
            else
            {
                toolStripMenuItemFileSave.Enabled = false;
            }
        }

        #endregion

        #region Tools and Documents

        private void dockManager_WindowClosing(object sender, TabbedMdiWindowClosingEventArgs e)
        {
            //// If a document is being closed and it has been modified...
            //if ((!ignoreModifiedDocumentClose) && (e.TabbedMdiWindow is DocumentWindow documentWindow))
            //{
            //    if (!HandleDocumentClosing(documentWindow))
            //        e.Cancel = true;
            //}

            System.Diagnostics.Debug.WriteLine("WindowClosing: Key={0}; Type={1}; Reason={2}; Cancel={3}",
                e.TabbedMdiWindow.Key, e.TabbedMdiWindow.DockObjectType, e.Reason, e.Cancel);
        }

        private void dockManager_WindowClosed(object sender, TabbedMdiWindowEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("WindowClosed: Key={0}; Type={1}", e.TabbedMdiWindow.Key, e.TabbedMdiWindow.DockObjectType);

            if (e.TabbedMdiWindow.DockObjectType == DockObjectType.DocumentWindow)
            {
                if (e.TabbedMdiWindow.Tag is ILoadedFile file)
                {
                    file.UnregisterDependant(m_hostDependancyObject);
                    m_fileExplorer.UpdateNodeStates();
                }
            }
        }

        #endregion

        #region Errors View

        private void ErrorsList_ParseFilesClicked(object sender, EventArgs e)
        {
            this.StartFileParsing();
        }

        private void ErrorsList_AutoParseFilesChanged(object sender, EventArgs e)
        {
        }

        private void ErrorsList_DoubleClickedLine(object sender, ParsingErrorListView.DoubleClickLineEventArgs args)
        {
            this.OpenOrActivateFileEditor(args.File, args.Line + 1);
        }

        #endregion

        #region File Explorer View

        private void FileExplorer_NodeSelectionChanged(object sender, FileExplorer.SelectionEventArgs e)
        {
            if (e.NodeData != null)
            {
                System.Diagnostics.Debug.WriteLine("FileSelectionChanged: " + e.NodeData.ToString() + " - " + e.Selection.ToString());
            }

            if (e.Selection == FileExplorer.SelectionEventArgs.SelectionType.Selected)
            {
                if (e.NodeData is IFileElement element)
                {
                    this.SetFocusedFileElement(element);
                }
            }
            if (e.Selection == FileExplorer.SelectionEventArgs.SelectionType.Activated)
            {
                if (e.NodeData is ILoadedFile file)
                {
                    this.OpenOrActivateFileEditor(file);
                }
                else if (e.NodeData is IFileElement element)
                {
                    this.OpenOrActivateFileEditor(element.ParentFile, element.Line);
                }
                else if (e.NodeData is FileExplorer.StationPropertiesData stationPropsData)
                {
                    this.CreateTextDocument(stationPropsData.File, null, false);
                }
            }
        }

        #endregion

        #region Test Report Overview View

        private void ReportOverview_DoubleClickedGroup(object sender, TestReportOverview.DoubleClickGroupEventArgs args)
        {
            m_logviewer.JumpTo(args.Group.LogStart, true);
        }

        private void ReportOverview_DoubleClickedResult(object sender, TestReportOverview.DoubleClickResultEventArgs args)
        {
            var log = args.Data.Group.LogStart;

            if (args.Data.Result.Verdict >= Verdict.Fail)
            {
                var failEntry = log;
                while (failEntry != null)
                {
                    if (failEntry.EntryType == LogEntry.Type.Error)
                    {
                        log = failEntry;    // Use this entry.
                        break;
                    }
                    failEntry = failEntry.Next;
                }
            }

            m_logviewer.JumpTo(log, true);
        }

        #endregion

        #endregion

        #region Application Tasks

        protected override void OnTaskHandlingStateChanged(StateChange change, string workingText)
        {
            toolStripStatusLabelApplicationTaskState.Text = workingText;
        }

        private enum TaskNoState { First, Next }

        #region Script File Loading

        //private void LoadedFiles_FileLoaded(object sender, LoadedFileEventArgs args)
        //{
        //}

        //private void LoadedFiles_FileClosed(object sender, LoadedFileEventArgs args)
        //{

        //}

        //private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ILoadedFile.RegisteredDependantsCount))
        //    {

        //    }
        //}

        private TaskAction ScriptFileLoadingTask(ref TaskNoState state, ref int index, ITaskStateReporting reporting)
        {
            m_targetFileFullPath = System.IO.Path.GetFullPath(m_targetFile);
            try
            {
                m_file = StepBroMain.LoadScriptFile(m_topScriptFileDependancyObject, m_targetFileFullPath);
                if (m_file == null)
                {
                    //retval = -1;
                    //ConsoleWriteErrorLine("Error: Loading script file failed ( " + m_targetFileFullPath + " )");
                }
                else
                {
                    UserDataStationManager.AddRecentFile(m_targetFileFullPath);

                    var shortcuts = ServiceManager.Global.Get<IFolderManager>();
                    var projectShortcuts = new FolderShortcutCollection(FolderShortcutOrigin.Project);
                    projectShortcuts.AddShortcut(StepBro.Core.Api.Constants.TOP_FILE_FOLDER_SHORTCUT, System.IO.Path.GetDirectoryName(m_file.FilePath), isResolved: true);
                    shortcuts.AddSource(projectShortcuts);
                }
            }
            catch (Exception ex)
            {
                m_mainLogger.LogError("Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
            }
            return TaskAction.Finish;
        }

        private void StartScriptFileLoading()
        {
            this.AddTask<TaskNoState>(ScriptFileLoadingTask, Priority.Normal, "Loading script file", "Load script file.");
        }

        #endregion

        #region File Parsing

        private enum FileParsingState { Init, Parse, Errors, Finish }

        private TaskAction FileParsingTask(ref FileParsingState state, ref int index, ITaskStateReporting reporting)
        {
            switch (state)
            {
                case FileParsingState.Init:
                    if (m_file == null)
                    {
                        return TaskAction.Cancel;
                    }

                    state = FileParsingState.Parse;
                    return TaskAction.ContinueOnWorkerThreadDomain;

                case FileParsingState.Parse:
                    var parsingSuccess = StepBroMain.ParseFiles(true);
                    if (parsingSuccess)
                    {
                        state = FileParsingState.Finish;
                    }
                    else
                    {
                        state = FileParsingState.Errors;
                    }
                    break;

                case FileParsingState.Errors:
                    {
                        m_toolWindowExecutionLog.Activate();
                    }
                    return TaskAction.Finish;

                case FileParsingState.Finish:
                    this.UpdateAfterSuccessfulFileParsing();
                    return TaskAction.Finish;

                default:
                    break;
            }
            return TaskAction.Continue;
        }

        private void StartFileParsing()
        {
            this.AddTask<FileParsingState>(FileParsingTask, Priority.Normal, "Parsing files", "Parse the script files.");
        }

        private void UpdateAfterSuccessfulFileParsing()
        {
            // Update the list of loaded script files.
            var fileManager = StepBroMain.ServiceManager.Get<ILoadedFilesManager>();
            var files = fileManager.ListFiles<IScriptFile>().ToList();

            m_toolsInteractionModel.Synchronize();

            // Update list of variables containing objects with the ITextCommandInput interface.
            var objects = m_objectManager.GetObjectCollection();
            //var commandObjectsContainers = objects.Where(o => o.Object is ITextCommandInput).ToList();
            //foreach (var o in commandObjectsContainers)
            //{
            //    m_commandObjectDictionary[o.FullName] = o.Object as ITextCommandInput;    // Add or override.
            //}


            m_fileElements.Clear();
            foreach (var e in files.SelectMany(f => f.ListElements()))
            {
                m_fileElements.Add(e);
            }

            foreach (var v in m_fileElements.Where(e => e.ElementType == FileElementType.FileVariable))
            {
                m_variableTypes[v.FullName] = v.DataType;
            }

            var namespaces = m_fileElements.Select(e => NamespaceFromFullName(e.FullName)).Distinct().ToList();

            foreach (var v in objects)
            {
                if (v.Object is StepBro.ToolBarCreator.ToolBar)
                {
                    var toolbar = v.Object as StepBro.ToolBarCreator.ToolBar;
                    panelCustomToolstrips.AddOrSet(v.FullName, toolbar, toolStripMain.Height);  // The height of the main toolbar is adjusted to the display scale factor.
                }
                else if (v.Object is StepBro.PanelCreator.Panel)
                {
                    var panel = v.Object as StepBro.PanelCreator.Panel;
                }
            }

            this.UpdateToolButton();

            this.LoadUserSettingsOnProject(); // Now ready to load the user settings (in case they have not been loaded yet).
        }

        #endregion

        #region Script Execution

        public IExecutionAccess StartExecution(bool addToHistory, IFileElement element, string partner, string objectVariable, object[] args)
        {
            if (this.ExecutionRunning)
            {
                var result = MessageBox.Show(
                    this,
                    "A script execution is already running." + Environment.NewLine + Environment.NewLine + "Would you like to put this new execution in queue?",
                    "StepBro - Starting script execution",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    return null;
                }
            }

            var executionData = new ScriptExecutionData(
                this,
                element,
                partner,
                objectVariable,
                args?.ToArray());
            executionData.AddToHistory = addToHistory;
            this.StartScriptExecution(executionData);

            string executionNote = null;
            if (toolStripTextBoxExeNote.Visible)
            {
                executionNote = toolStripTextBoxExeNote.Text;
            }

            string objectInstanceText = String.IsNullOrEmpty(objectVariable) ? "" : (objectVariable.Split('.').Last() + ".");
            string partnertext = String.IsNullOrEmpty(partner) ? "" : (" @ " + partner);
            string noteText = String.IsNullOrEmpty(executionNote) ? "" : (" - \"" + executionNote + "\"");
            string elementText = String.IsNullOrEmpty(objectInstanceText) ? element.FullName : element.FullName.Split('.').Last();
            m_mainLogger.LogUserAction("Request script execution: " + objectInstanceText + elementText + partnertext + noteText);

            return executionData;
        }

        private enum ScriptExecutionState { Init, Running, Finish }

        private TaskAction ScriptExecutionTask(ref ScriptExecutionState state, ref int index, ITaskStateReporting reporting)
        {
            switch (state)
            {
                case ScriptExecutionState.Init:
                    {
                        toolStripStatusLabelExecutionResult.Text = String.Empty;
                        if (m_file == null || StepBroMain.LastParsingErrorCount > 0)
                        {
                            return TaskAction.Cancel;
                        }
                        var data = m_executionQueue.Peek();
                        var element = data.Element;
                        if (element == null)
                        {
                            element = StepBroMain.TryFindFileElement(data.ElementName);
                        }
                        if (element != null)
                        {
                            data.Element = element;     // Save the reference.
                            IPartner partner = null;
                            List<object> targetArguments = null;
                            if (data.UnparsedArguments != null)
                            {
                                targetArguments =
                                    m_commandLineOptions?.Arguments.Select(
                                        (a) => StepBroMain.ParseExpression(element.ParentFile, a)).ToList();
                                data.Arguments = new List<object>(targetArguments); // Save a copy.
                            }
                            else if (data.Arguments != null && data.Arguments.Count > 0)
                            {
                                targetArguments = new List<object>(data.Arguments);
                            }
                            else
                            {
                                targetArguments = new List<object>();
                            }

                            if (!String.IsNullOrEmpty(data.Partner))
                            {
                                partner = element.ListPartners().First(p => String.Equals(data.Partner, p.Name, StringComparison.InvariantCultureIgnoreCase));
                                if (partner == null)
                                {
                                    data.Errors.Add($"Error: The specified file element does not have a partner named \"{data.Partner}\".");
                                    return TaskAction.Cancel;
                                }
                            }
                            else
                            {
                                if (element is IFileProcedure)
                                {
                                    var procedure = (IFileProcedure)element;

                                    // NOTE: targetObject might be set, even if it should not be used.

                                    if (!String.IsNullOrEmpty(data.Object) && procedure.IsFirstParameterThisReference)
                                    {
                                        var theObject = m_objectManager.GetObjectCollection().FirstOrDefault(v => string.Equals(v.FullName, data.Object, StringComparison.InvariantCulture));
                                        if (theObject != null)
                                        {
                                            targetArguments.Insert(0, theObject.Object);
                                        }
                                        else
                                        {
                                            data.Errors.Add($"Error: Target object '{data.Object}' was not found in the list of global variables.");
                                            return TaskAction.Cancel;
                                        }
                                    }
                                }
                                else
                                {
                                    data.Errors.Add($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                                    return TaskAction.Cancel;
                                }
                            }

                            try
                            {
                                var execution = StepBroMain.StartProcedureExecution(element, partner, targetArguments.ToArray());
                                if (execution != null)
                                {
                                    data.SetExecution(execution);
                                    if (data.AddToHistory)
                                    {
                                        this.AddElementExecutionToHistory(
                                            data.Element,
                                            data.Partner,
                                            (partner != null) ? partner.IsModel : false,
                                            data.Object,
                                            data.Arguments?.ToArray());
                                    }
                                    toolStripButtonStopScriptExecution.Enabled = true;
                                }
                            }
                            catch (TargetParameterCountException)
                            {
                                //ConsoleWriteErrorLine("Error: The number of arguments does not match the target procedure.");
                            }
                        }
                        else
                        {
                            //ConsoleWriteErrorLine($"Error: File element named '{targetElement} was not found.");
                        }

                        state = ScriptExecutionState.Running;
                    }
                    return TaskAction.ContinueOnWorkerThreadDomain;

                case ScriptExecutionState.Running:
                    while (!m_executionQueue.Peek().Execution.Task.Ended())
                    {
                        Thread.Sleep(200);
                    }
                    state = ScriptExecutionState.Finish;
                    break;

                case ScriptExecutionState.Finish:
                    {
                        var executionJob = m_executionQueue.Dequeue();
                        toolStripButtonStopScriptExecution.Enabled = false;

                        var result = executionJob.Execution.Result;
                        var createdReport = executionJob.Execution.Report;
                        if (result.ProcedureResult != null && !String.IsNullOrEmpty(result.ProcedureResult.Description))
                        {
                            toolStripStatusLabelExecutionResult.Text = result.ResultText();
                        }
                        else
                        {
                            if (executionJob.Partner == null &&
                                executionJob.Element is IFileProcedure &&
                                !(executionJob.Element as IFileProcedure).ReturnType.Equals(TypeReference.TypeVoid))
                            {
                                toolStripStatusLabelExecutionResult.Text = "Result: " + StringUtils.ObjectToString(result.ReturnValue, true);
                            }
                            else
                            {
                                toolStripStatusLabelExecutionResult.Text = "No execution result";
                            }
                        }
                    }
                    return TaskAction.Finish;

                default:
                    break;
            }
            return TaskAction.Continue;
        }

        private void StartScriptExecution(ScriptExecutionData executionData)
        {
            m_executionQueue.Enqueue(executionData);
            this.StartScriptExecution();
        }
        private void StartScriptExecution()
        {
            this.AddTask<ScriptExecutionState>(ScriptExecutionTask, Priority.Normal, "Executing script", "Execute script.");
        }


        #endregion

        #endregion

        #region Utils

        private static string NamespaceFromFullName(string name)
        {
            var parts = name.Split('.');
            if (parts.Length == 1) return "";
            else return parts[0];
        }

        private static string NameFromFullName(string name)
        {
            var parts = name.Split('.');
            if (parts.Length == 1) return name;
            else return string.Join('.', parts.Skip(1));
        }

        #endregion

        private void toolStripStatusLabelExecutionResult_Click(object sender, EventArgs e)
        {

        }

        internal void OpenUserInteraction(UserInteraction interaction)
        {
            if (m_currentUserInteractionData != null)
            {
                m_currentUserInteractionData.OnOpen -= CurrentUserInteractionData_OnOpen;
                m_currentUserInteractionData.OnClose -= CurrentUserInteractionData_OnClose;
                m_currentUserInteractionData = null;
            }

            m_currentUserInteractionData = interaction;
            m_currentUserInteractionData.OnOpen += CurrentUserInteractionData_OnOpen;
            m_currentUserInteractionData.OnClose += CurrentUserInteractionData_OnClose;
            m_userInteractionView.Setup(m_currentUserInteractionData);
        }

        private void CurrentUserInteractionData_OnOpen(object sender, EventArgs e)
        {
            this.BeginInvoke(this.ShowUserInteractionView);
        }

        private void CurrentUserInteractionData_OnClose(object sender, EventArgs e)
        {
            this.BeginInvoke(this.CloseUserInteractionView);
        }

        private void ShowUserInteractionView()
        {
            m_toolWindowUserInteraction.Activate();
        }

        private void CloseUserInteractionView()
        {
            m_toolWindowUserInteraction.Close();
            m_currentUserInteractionData.OnOpen -= CurrentUserInteractionData_OnOpen;
            m_currentUserInteractionData.OnClose -= CurrentUserInteractionData_OnClose;
            m_currentUserInteractionData = null;
        }
    }
}
