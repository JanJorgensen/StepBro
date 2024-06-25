using ActiproSoftware.UI.WinForms.Controls.Docking;
using ActiproSoftware.UI.WinForms.Drawing;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Controls;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.UI.WinForms;
using StepBro.UI.WinForms.Controls;
using System.Text;
using static StepBro.Core.Host.HostApplicationTaskHandler;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.SimpleWorkbench
{
    public partial class MainForm : FormWithHostApplicationTaskHandling
    {
        private int documentWindowIndex = 1;
        //private int toolWindowIndex = 1;

        //private bool showDarkThemeDisclaimer = true;
        //private bool ignoreModifiedDocumentClose = false;
        private bool ignoreTextChangedEvent = false;

        //private HostAccess m_hostAccess;
        private CommandLineOptions m_commandLineOptions = null;
        private HostAccess m_hostAccess = null;
        private ILoadedFilesManager m_loadedFiles = null;

        private ToolWindow m_toolWindowExecutionLog = null;
        private LogViewer m_logviewer = null;

        private ToolWindow m_toolWindowParsingErrors = null;
        private ParsingErrorListView m_errorsList = null;

        private object m_applicationResourceUserObject = new object();
        private Dictionary<string, ITextCommandInput> m_commandObjectDictionary = null;

        // Script Execution
        private IScriptExecution m_execution = null;

        private IScriptFile m_file = null;
        private IFileElement m_element = null;
        private IPartner m_partner = null;
        //private bool executionRequestSilent = false;
        private string m_targetFile = null;
        private string m_targetFileFullPath = null;
        private string m_targetElement = null;
        private string m_targetPartner = null;
        private string m_targetObject = null;
        private List<object> m_targetArguments = new List<object>();


        public MainForm()
        {
            InitializeComponent();

            toolStripMainMenu.Text = "\u2630";
            this.StartUsingTaskHandlingTimer();

            //toolWindowProperties.Close();
            //toolWindowHelp.Close();

            //this.CreateTextDocument(null, "This is a read-only document.  Notice the lock context image in the tab.", true).Activate();
            //this.CreateTextDocument(null, null, false).Activate();

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            StepBroMain.Initialize(m_hostService);
            m_loadedFiles = StepBroMain.GetLoadedFilesManager();
            m_loadedFiles.FileLoaded += LoadedFiles_FileLoaded;
            m_loadedFiles.FileClosed += LoadedFiles_FileClosed;
            m_loadedFiles.FilePropertyChanged += File_PropertyChanged;

            this.ParseCommandLineOptions();

            m_logviewer = new LogViewer();
            m_toolWindowExecutionLog = new ToolWindow(dockManager, "ExecutionLogView", "Execution Log", null, m_logviewer);
            m_toolWindowExecutionLog.DockTo(dockManager, DockOperationType.RightOuter);
            m_toolWindowExecutionLog.State = ToolWindowState.TabbedDocument;
            m_logviewer.Setup();

            m_errorsList = new ParsingErrorListView();
            m_toolWindowParsingErrors = new ToolWindow(dockManager, "ErrorsView", "Errors", null, m_errorsList);
            m_toolWindowParsingErrors.DockTo(dockManager, DockOperationType.BottomOuter);

            // TO BE DELETED
            dockManager.SaveCustomToolWindowLayoutData += DockManager_SaveCustomToolWindowLayoutData;
            timerTest.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            var assembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            var folder = System.IO.Path.GetDirectoryName(assembly);
            var docPersistFile = System.IO.Path.Combine(folder, "doclayout.xml");
            var toolPersistFile = System.IO.Path.Combine(folder, "toollayout.xml");
            dockManager.SaveDocumentLayoutToFile(docPersistFile);
            dockManager.SaveToolWindowLayoutToFile(toolPersistFile);
            var layout = dockManager.ToolWindowLayoutData;
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

            m_targetFile = m_commandLineOptions.InputFile;
            m_targetElement = m_commandLineOptions.TargetElement;
            m_targetObject = m_commandLineOptions.TargetInstance;
            m_targetPartner = m_commandLineOptions.TargetModel;
            m_targetArguments = m_commandLineOptions?.Arguments.Select((a) => StepBroMain.ParseExpression(m_element?.ParentFile, a)).ToList();

            if (!String.IsNullOrEmpty(m_targetFile))
            {
                this.StartFileLoading();
                this.StartFileParsing();

                if (!String.IsNullOrEmpty(m_targetElement))
                {
                    this.StartScriptExecution();
                }
            }
        }

        private void helpBrowser_Load(object sender, EventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, System.EventArgs e)
        {
            if (ignoreTextChangedEvent)
                return;

            DocumentWindow documentWindow = ((RichTextBox)sender).Parent as DocumentWindow;
            if (documentWindow != null)
                documentWindow.Modified = ((RichTextBox)sender).Modified;
        }

        private DocumentWindow CreateTextDocument(string fileName, string text, bool readOnly)
        {
            DocumentWindow documentWindow;

            // If the document is already open, show a message
            if (dockManager.DocumentWindows[fileName] != null)
            {
                dockManager.DocumentWindows[fileName].Activate();
                MessageBox.Show(this, String.Format("The file '{0}' is already open.", fileName), "File Already Open", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        // Create a TextBox for the document
                        RichTextBox textBox = new RichTextBox();
                        textBox.Multiline = true;
                        textBox.Font = new Font("Courier New", 10);
                        textBox.BorderStyle = BorderStyle.None;
                        textBox.HideSelection = false;
                        textBox.ReadOnly = readOnly;
                        textBox.ScrollBars = RichTextBoxScrollBars.Both;
                        textBox.WordWrap = false;

                        // If no data was passed in, generate some
                        if (fileName == null)
                            fileName = String.Format("Document{0}.txt", documentWindowIndex++);
                        if (text == null)
                            text = "Visit our web site to learn more about Actipro WinForms Studio or our other controls:\r\nhttps://www.actiprosoftware.com/\r\n\r\nThis document was created at " + DateTime.Now.ToString() + ".";

                        // Create the document window
                        textBox.Text = text;
                        textBox.TextChanged += new EventHandler(this.textBox_TextChanged);
                        documentWindow = new DocumentWindow(dockManager, fileName, Path.GetFileName(fileName), 3, textBox);
                        if (fileName != null)
                        {
                            documentWindow.FileName = fileName;
                            documentWindow.FileType = String.Format("Text File (*{0})", Path.GetExtension(fileName).ToLower());
                        }
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

        #region Application Tasks

        protected override void OnTaskHandlingStateChanged(StateChange change, string workingText)
        {
            toolStripStatusLabelApplicationTaskState.Text = workingText;
        }

        private enum TaskNoState { Do }

        private TaskAction FileLoadingTask(ref TaskNoState state, ref int index, ITaskStateReporting reporting)
        {
            m_targetFileFullPath = System.IO.Path.GetFullPath(m_targetFile);
            try
            {
                m_file = StepBroMain.LoadScriptFile(m_applicationResourceUserObject, m_targetFileFullPath);
                if (m_file == null)
                {
                    //retval = -1;
                    //ConsoleWriteErrorLine("Error: Loading script file failed ( " + m_targetFileFullPath + " )");
                }
                else
                {
                    var shortcuts = ServiceManager.Global.Get<IFolderManager>();
                    var projectShortcuts = new FolderShortcutCollection(FolderShortcutOrigin.Project);
                    projectShortcuts.AddShortcut(StepBro.Core.Api.Constants.TOP_FILE_FOLDER_SHORTCUT, System.IO.Path.GetDirectoryName(m_file.FilePath), isResolved: true);
                    shortcuts.AddSource(projectShortcuts);

                    //m_next.Enqueue(StateOrCommand.ParseFiles);  // File has been loaded; start the parsing.
                }
            }
            catch (Exception)
            {
                //retval = -1;
                //ConsoleWriteErrorLine("Error: Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
            }
            return TaskAction.Finish;
        }

        private void StartFileLoading()
        {
            this.AddTask<TaskNoState>(FileLoadingTask, "Loading file", "Load script file.");
        }

        private enum FileParsingState { Init, Parse, Finish }

        private TaskAction FileParsingTask(ref FileParsingState state, ref int index, ITaskStateReporting reporting)
        {
            switch (state)
            {
                case FileParsingState.Init:
                    // TODO: Check whether file is loaded.

                    state = FileParsingState.Parse;
                    return TaskAction.ContinueOnWorkerThreadDomain;

                case FileParsingState.Parse:
                    var parsingSuccess = StepBroMain.ParseFiles(true);
                    state = FileParsingState.Finish;
                    break;

                case FileParsingState.Finish:
                    return TaskAction.Finish;

                default:
                    break;
            }
            return TaskAction.Continue;
        }

        private void StartFileParsing()
        {
            this.AddTask<FileParsingState>(FileParsingTask, "Parsing files", "Parse the script files.");
        }

        private enum ScriptExecutionState { Init, Parse, Finish }

        private TaskAction ScriptExecutionTask(ref ScriptExecutionState state, ref int index, ITaskStateReporting reporting)
        {
            //switch (state)
            //{
            //    case ScriptExecutionState.Init:
            //        // TODO: Check whether file is loaded.

            //        state = FileParsingState.Parse;
            //        return TaskAction.ContinueOnWorkerThreadDomain;

            //    case FileParsingState.Parse:
            //        var parsingSuccess = StepBroMain.ParseFiles(true);
            //        state = FileParsingState.Finish;
            //        break;

            //    case FileParsingState.Finish:
            //        return TaskAction.Finish;

            //    default:
            //        break;
            //}
            return TaskAction.Continue;
        }

        private void StartScriptExecution()
        {
            this.AddTask<ScriptExecutionState>(ScriptExecutionTask, "Executing script", "Execute script.");
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
            if (!viewExecutionLogToolStripMenuItem.Checked)
            {
                m_toolWindowExecutionLog.Activate();
            }
            else
            {
                m_toolWindowExecutionLog.Close();
            }
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

        #endregion

        #region Help Menu

        private void viewDocumentationBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //toolWindowHelp.Activate();
        }

        #endregion

        #region Loaded Script Files

        private void LoadedFiles_FileLoaded(object sender, LoadedFileEventArgs args)
        {

        }

        private void LoadedFiles_FileClosed(object sender, LoadedFileEventArgs args)
        {

        }

        private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        #endregion

        private void timerTest_Tick(object sender, EventArgs e)
        {
            timerTest.Stop();
            //timerTest.Interval = 1500;
            //StepBroMain.Logger.RootLogger.Log("Ello!!  - " + DateTime.Now.ToLongTimeString());
        }

        private void toolStripMenuItemTestActionStartFileParsing_Click(object sender, EventArgs e)
        {
            this.StartFileParsing();
        }
    }
}
