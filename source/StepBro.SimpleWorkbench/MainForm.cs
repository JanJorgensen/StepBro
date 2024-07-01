using ActiproSoftware.UI.WinForms.Controls.Docking;
using ActiproSoftware.UI.WinForms.Drawing;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Controls;
using StepBro.Core.Data;
using StepBro.Core.Data.SerializationHelp;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.IPC;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System.Collections.ObjectModel;
using StepBro.UI.WinForms;
using StepBro.UI.WinForms.Controls;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static StepBro.Core.Host.HostApplicationTaskHandler;
using static StepBro.SimpleWorkbench.Shortcuts;
using StepBroMain = StepBro.Core.Main;
using Antlr4.Runtime.Misc;

namespace StepBro.SimpleWorkbench
{
    public partial class MainForm : FormWithHostApplicationTaskHandling, ICoreAccess
    {
        private int documentWindowIndex = 1;
        //private int toolWindowIndex = 1;

        //private bool showDarkThemeDisclaimer = true;
        //private bool ignoreModifiedDocumentClose = false;
        private bool ignoreTextChangedEvent = false;

        private bool m_settingCommandCombo = false;

        //private HostAccess m_hostAccess;
        private CommandLineOptions m_commandLineOptions = null;
        private HostAccess m_hostAccess = null;
        private ILoadedFilesManager m_loadedFiles = null;
        private IDynamicObjectManager m_objectManager = null;

        private ToolWindow m_toolWindowExecutionLog = null;
        private LogViewer m_logviewer = null;

        private ToolWindow m_toolWindowParsingErrors = null;
        private ParsingErrorListView m_errorsList = null;

        private object m_applicationResourceUserObject = new object();
        private Dictionary<string, ITextCommandInput> m_commandObjectDictionary = new Dictionary<string, ITextCommandInput>();

        // Script Execution
        //private IScriptExecution m_execution = null;

        private string m_targetFile = null;
        private string m_targetFileFullPath = null;
        private IScriptFile m_file = null;

        private ObservableCollection<IObjectContainer> m_objects = new ObservableCollection<IObjectContainer>();
        private ObservableCollection<IFileElement> m_fileElements = new ObservableCollection<IFileElement>();
        private Dictionary<string, TypeReference> m_variableTypes = new Dictionary<string, TypeReference>();

        private Queue<ScriptExecutionData> m_executionQueue = new Queue<ScriptExecutionData>();

        //private IFileElement m_element = null;
        //private IPartner m_partner = null;
        //private bool executionRequestSilent = false;
        //private string m_targetElement = null;
        //private string m_targetPartner = null;
        //private string m_targetObject = null;
        //private List<object> m_targetArguments = new List<object>();

        public class UserDataCurrent
        {
            [JsonDerivedType(typeof(ProcedureShortcut), typeDiscriminator: "procedure")]
            [JsonDerivedType(typeof(ObjectCommandShortcut), typeDiscriminator: "command")]
            public class Shortcut
            {
                public string Text { get; set; }
            }
            public class ProcedureShortcut : Shortcut
            {
                public string Element { get; set; } = null;
                public string Partner { get; set; } = null;
                public string Instance { get; set; } = null;
            }
            public class ObjectCommandShortcut : Shortcut
            {
                public string Instance { get; set; } = null;
                public string Command { get; set; } = null;
            }

            public class PanelSetting
            {
                public string Panel { get; set; }
                public string ID { get; set; }
                public string Value { get; set; }
            }

            public int version { get; set; } = 2;
            public Shortcut[] Shortcuts { get; set; } = null;
            public PanelSetting[] PanelSettings { get; set; } = null;
            public string[] HiddenToolbars { get; set; } = null;

        }


        public MainForm()
        {
            InitializeComponent();

            toolStripMainMenu.Text = "\u2630";
            toolStripButtonRunCommand.Text = "\u23F5";
            toolStripButtonStopScriptExecution.Text = "\u23F9";
            toolStripButtonAddShortcut.Text = "\u2795";
            this.StartUsingTaskHandlingTimer();
            panelCustomToolstrips.Setup(this);
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
            m_objectManager = StepBroMain.ServiceManager.Get<IDynamicObjectManager>();

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


            var userData = new UserDataCurrent();

            var shortcuts = new List<UserDataCurrent.Shortcut>();
            foreach (var shortcut in toolStripMain.Items.Cast<ToolStripItem>().Where(o => object.Equals(m_applicationResourceUserObject, o.Tag)))
            {
                if (shortcut is ScriptExecutionToolStripMenuItem)
                {
                    var typed = shortcut as ScriptExecutionToolStripMenuItem;
                    var shortcutData = new UserDataCurrent.ProcedureShortcut();
                    shortcutData.Text = typed.Text;
                    shortcutData.Element = typed.FileElement;
                    shortcutData.Partner = typed.Partner;
                    shortcutData.Instance = typed.InstanceObject;
                    shortcuts.Add(shortcutData);
                }
                else if (shortcut is ObjectCommandToolStripMenuItem)
                {
                    var typed = shortcut as ObjectCommandToolStripMenuItem;
                    var shortcutData = new UserDataCurrent.ObjectCommandShortcut();
                    shortcutData.Text = typed.Text;
                    shortcutData.Instance = typed.Instance;
                    shortcutData.Command = typed.Command;
                    shortcuts.Add(shortcutData);
                }
            }
            if (shortcuts.Count > 0)
            {
                userData.Shortcuts = shortcuts.ToArray();
            }

            userData.HiddenToolbars = (panelCustomToolstrips.ListHiddenToolbars().Count() > 0) ? panelCustomToolstrips.ListHiddenToolbars().ToArray() : null;

            if (userData.Shortcuts != null || userData.PanelSettings != null || userData.HiddenToolbars != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                // TODO: find solution for this ...
                //using (FileStream createStream = File.Create(m_userFile))
                //{
                //    JsonSerializer.Serialize(createStream, userData, options);
                //}
            }

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

            if (!String.IsNullOrEmpty(m_targetFile))
            {
                this.StartFileLoading();
                this.StartFileParsing();

                if (!String.IsNullOrEmpty(m_commandLineOptions.TargetElement))
                {
                    var executionData = new ScriptExecutionData(
                        this,
                        m_commandLineOptions.TargetElement,
                        m_commandLineOptions.TargetModel,
                        m_commandLineOptions.TargetInstance,
                        null);
                    if (m_commandLineOptions.Arguments != null)
                    {
                        executionData.UnparsedArguments = m_commandLineOptions.Arguments.ToList();
                    }
                    this.StartScriptExecution(executionData);
                }
            }
            else
            {
                // TODO: Check if unexpected arguments, and report error if any.
            }
        }

        public IExecutionAccess StartExecution(bool addToHistory, string element, string partner, string objectVariable, object[] args)
        {
            if (this.ExecutionRunning)
            {
                return null;
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
            string elementText = String.IsNullOrEmpty(objectInstanceText) ? element : element.Split('.').Last();
            StepBroMain.Logger.RootLogger.LogUserAction("Request script execution: " + objectInstanceText + elementText + partnertext + noteText);

            return executionData;
        }

        #region ICoreAccess

        public IExecutionAccess StartExecution(string element, string partner, string objectVariable, object[] args)
        {
            return this.StartExecution(true, element, partner, objectVariable, args);
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
            StepBroMain.Logger.RootLogger.LogUserAction($"Request run '{objectVariable}' command \"{command}\"");
            var obj = m_commandObjectDictionary[objectVariable];
            if (obj.AcceptingCommands())
            {
                obj.ExecuteCommand(command);
            }
            else
            {
                string errorMessage = $"'{objectVariable}' is not accepting commands. Did you forget to open or connect?";

                StepBroMain.Logger.RootLogger.LogError(errorMessage);
            }
        }


        #endregion

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

        #region File Loading

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

        #endregion

        #region File Parsing

        private enum FileParsingState { Init, Parse, Errors, Finish }

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
            this.AddTask<FileParsingState>(FileParsingTask, "Parsing files", "Parse the script files.");
        }

        private void UpdateAfterSuccessfulFileParsing()
        {
            // Update list of variables containing objects with the ITextCommandInput interface.
            var objects = m_objectManager.GetObjectCollection();
            var commandObjectsContainers = objects.Where(o => o.Object is ITextCommandInput).ToList();
            foreach (var o in commandObjectsContainers)
            {
                m_commandObjectDictionary[o.FullName] = o.Object as ITextCommandInput;    // Add or override.
            }
            // TODO: Udate the command target combo.


            // Update the list of loaded script files.
            var fileManager = StepBroMain.ServiceManager.Get<ILoadedFilesManager>();
            var files = fileManager.ListFiles<IScriptFile>().ToList();


            m_fileElements.Clear();
            foreach (var e in files.SelectMany(f => f.ListElements()))
            {
                m_fileElements.Add(e);
            }

            foreach (var v in m_fileElements.Where(e => e.ElementType == FileElementType.FileVariable))
            {
                m_variableTypes[v.FullName] = v.DataType;
            }


            //for (int i = 0; i < files.Count; i++)
            //{
            //    var f = files[i];

            //    m_fileElements = f.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration || e.ElementType == FileElementType.TestList).ToList();
            //    //foreach (var e in elements)
            //    //{
            //    //    switch (e.ElementType)
            //    //    {
            //    //        case FileElementType.ProcedureDeclaration:
            //    //            {
            //    //                //elementData = new StepBro.Sidekick.Messages.Procedure();
            //    //                //var procedureData = elementData as StepBro.Sidekick.Messages.Procedure;
            //    //                //var p = e as IFileProcedure;
            //    //                //if (p.Parameters.Length > 0 && p.IsFirstParameterThisReference)
            //    //                //{
            //    //                //    var par = p.Parameters[0];
            //    //                //    (elementData as StepBro.Sidekick.Messages.Procedure).FirstParameterIsInstanceReference = true;

            //    //                //    var instances = new List<string>();
            //    //                //    foreach (var v in objects)
            //    //                //    {
            //    //                //        if (par.Value.IsAssignableFrom(variableTypes[v.FullName]))
            //    //                //        {
            //    //                //            instances.Add(v.FullName);
            //    //                //        }
            //    //                //    }
            //    //                //    if (instances.Count > 0)
            //    //                //    {
            //    //                //        procedureData.CompatibleObjectInstances = instances.ToArray();
            //    //                //    }
            //    //                //}
            //    //                //(elementData as StepBro.Sidekick.Messages.Procedure).Parameters = p.Parameters.Select(p => new StepBro.Sidekick.Messages.Parameter(p.Name, p.Value.TypeName())).ToArray();
            //    //                //(elementData as StepBro.Sidekick.Messages.Procedure).ReturnType = p.ReturnType.TypeName();
            //    //            }
            //    //            break;
            //    //        case FileElementType.TestList:
            //    //            //elementData = new StepBro.Sidekick.Messages.TestList();
            //    //            break;
            //    //        default:
            //    //            break;
            //    //    }
            //    //}
            //}

            foreach (var v in objects)
            {
                if (v.Object is StepBro.ToolBarCreator.ToolBar)
                {
                    var toolbar = v.Object as StepBro.ToolBarCreator.ToolBar;
                    panelCustomToolstrips.AddOrSet(v.FullName, toolbar);
                }
                else if (v.Object is StepBro.PanelCreator.Panel)
                {
                    var panel = v.Object as StepBro.PanelCreator.Panel;
                }
            }

            #region Update Command Objects

            var selectedTool = toolStripComboBoxTool.SelectedItem as ComboboxItem;
            toolStripComboBoxTool.Items.Clear();
            int selection = 0;
            int index = 0;

            foreach (var v in objects)
            {
                if (v.Object is ITextCommandInput)
                {
                    var name = v.FullName.Split('.').Last();
                    toolStripComboBoxTool.Items.Add(new ComboboxItem(name, v));
                    if (selectedTool != null && name == selectedTool.Text)
                    {
                        selection = index;
                    }
                    index++;
                }
            }
            if (toolStripComboBoxTool.Items.Count > 0)
            {
                toolStripComboBoxTool.Enabled = true;
                toolStripComboBoxToolCommand.Enabled = true;
                toolStripComboBoxTool.SelectedIndex = selection;
                toolStripComboBoxTool.SelectionLength = 0;
            }
            else
            {
                toolStripComboBoxTool.Enabled = false;
                toolStripComboBoxToolCommand.Enabled = false;
            }

            #endregion

        }

        #endregion

        #region Script Execution

        private enum ScriptExecutionState { Init, Running, Finish }

        private TaskAction ScriptExecutionTask(ref ScriptExecutionState state, ref int index, ITaskStateReporting reporting)
        {
            switch (state)
            {
                case ScriptExecutionState.Init:
                    {
                        var data = m_executionQueue.Peek();
                        var element = StepBroMain.TryFindFileElement(data.Element);
                        if (element != null)
                        {
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
                                    data.Errors.Add($"Error: The specified file element does not have a model named \"{data.Partner}\".");
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
                                data.SetExecution(execution);

                                this.AddElementExecutionToHistory(data.Element, data.Partner, data.Object, data.Arguments?.ToArray());
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
                    while (!m_executionQueue.Peek().State.HasEnded())
                    {
                        Thread.Sleep(200);
                    }
                    state = ScriptExecutionState.Finish;
                    break;

                case ScriptExecutionState.Finish:
                    m_executionQueue.Dequeue();
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
            this.AddTask<ScriptExecutionState>(ScriptExecutionTask, "Executing script", "Execute script.");
        }


        #endregion

        #endregion

        private void AddElementExecutionToHistory(string element, string partner, string objectVariable, object[] args)
        {
            var title = ScripExecutionButtonTitle(false, element, partner, objectVariable, args);

            var first = toolStripSplitButtonRunScript.DropDownItems.IndexOf(toolStripSeparatorRunBeforeHistory) + 1;
            ScriptExecutionToolStripMenuItem found = null;
            int historyItemsCount = (toolStripSplitButtonRunScript.Tag != null) ? (int)toolStripSplitButtonRunScript.Tag : 0;
            if (historyItemsCount > 0)
            {
                for (int i = 0; i < toolStripSplitButtonRunScript.DropDownItems.Count; i++)
                {
                    var exeItem = toolStripSplitButtonRunScript.DropDownItems[i] as ScriptExecutionToolStripMenuItem;
                    if (exeItem == null) break;     // Stop here...
                    if (exeItem.Equals(element, partner, objectVariable))
                    {
                        found = exeItem;
                        toolStripSplitButtonRunScript.DropDownItems.RemoveAt(i);    // Remove it (to be inserted at the top).
                        historyItemsCount--;
                        break;
                    }
                }
            }

            toolStripSeparatorRunAfterHistory.Visible = true;

            if (found == null)
            {
                found = new ScriptExecutionToolStripMenuItem();
                found.FileElement = element;
                found.Partner = partner;
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

        #region USER INTERACTION - COMMANDS

        private void toolStripComboBoxTool_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripComboBoxTool.ToolTipText =
                "Select tool/object to use for the command prompt. Selected object: '" +
                ((IObjectContainer)(toolStripComboBoxTool.SelectedItem as ComboboxItem).Value).FullName + "'";
        }

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
            var container = (toolStripComboBoxTool.Items[toolStripComboBoxTool.SelectedIndex] as ComboboxItem).Value as IObjectContainer;
            this.ExecuteObjectCommand(container.FullName, command);
        }

        //private void ObjectCommandExecutionEntry_ShortcutClick(object sender, EventArgs e)
        //{
        //    var executionEntry = sender as ObjectCommandToolStripMenuItem;
        //    if (toolStripMenuItemDeleteShortcut.Checked)
        //    {
        //        var choise = MessageBox.Show(
        //            this,
        //            "Should the shortcut\r\n\r\n\"" + executionEntry.Text + "\"\r\n\r\nbe deleted?",
        //            "StepBro - Deleting shortcut",
        //            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        if (choise == DialogResult.Yes)
        //        {
        //            toolStripMain.Items.Remove(executionEntry);
        //        }
        //        toolStripMenuItemDeleteShortcut.Checked = false;
        //    }
        //    else
        //    {
        //        this.ExecuteCommand(executionEntry.Instance, executionEntry.Command);
        //    }
        //}

        #endregion

        #region USER INTERACTION - EXECUTION

        private void FileElementExecutionEntry_Click(object sender, EventArgs e)
        {
            var executionEntry = sender as ScriptExecutionToolStripMenuItem;
            this.StartExecution(true, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
        }

        private void toolStripTextBoxRunSearch_TextChanged(object sender, EventArgs e)
        {
            this.UpdateFileElementExecutionSearchResult();
        }

        private void toolStripSplitButtonRunScript_DropDownOpening(object sender, EventArgs e)
        {
            this.UpdateFileElementExecutionSearchResult();
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

                    foreach (var element in sortedMatches)
                    {
                        if (element.ElementType == FileElementType.ProcedureDeclaration)
                        {
                            toolStripSplitButtonRunScript.DropDownItems.Add(this.CreateProcedureShortcutMenu(element as IFileProcedure));
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
                foreach (var v in m_objects)
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
                        var procedureExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(procedure.FullName, (partner != null) ? partner.Name : null, null);
                        procedureExecutionOptionMenu.Size = new Size(182, 22);
                        if (partner != null)
                        {
                            procedureExecutionOptionMenu.Name = "toolStripMenuProcedure" + procedure.Name + "Dot" + partner.Name;
                            procedureExecutionOptionMenu.Text = procedure.Name + "." + partner.Name;
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

                        var procedureExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(procedure.FullName, null, variable);
                        procedureExecutionOptionMenu.Size = new Size(182, 22);
                        procedureExecutionOptionMenu.Name = "toolStripMenuProcedure" + procedure.Name + "On" + variable.Replace(".", "Dot");
                        procedureExecutionOptionMenu.SetText();
                        procedureExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}' partner '{partner.Name}'";
                                                                         //procedureExecutionOptionMenu.Click += FileElementExecutionEntry_Click; // TODO
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
                    executionItem.Click += FileElementExecutionEntry_Click;
                }
            }
            else
            {
                // No partners or instance object, just the direct procedure call.

                var procedureMenu = new ScriptExecutionToolStripMenuItem(procedure.FullName, null, null);
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

        #endregion

        private void toolStripMenuItemTestActionStartFileParsing_Click(object sender, EventArgs e)
        {
            this.StartFileParsing();
        }

        private void toolStripSplitButtonRunScript_ButtonClick(object sender, EventArgs e)
        {
            if (toolStripSplitButtonRunScript.Tag != null)
            {
                var first = toolStripSplitButtonRunScript.DropDownItems.IndexOf(toolStripSeparatorRunBeforeHistory) + 1;
                var executionEntry = toolStripSplitButtonRunScript.DropDownItems[first] as ScriptExecutionToolStripMenuItem;
                this.StartExecution(true, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
            }
        }
    }
}
