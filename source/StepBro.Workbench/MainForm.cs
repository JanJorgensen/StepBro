using StepBro.Core;
using StepBro.Core.Controls;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Workbench
{
    public partial class MainForm : Form
    {
        private readonly string DockConfigFile = "DockPanel.config";
        private bool m_bSaveLayout = true;
        private DeserializeDockContent m_deserializeDockContent;
        private FileExplorer m_solutionExplorer;
        private DummyPropertyWindow m_propertyWindow;
        private DummyToolbox m_toolbox;
        private OutputWindow m_outputWindow;
        private ErrorsWindow m_errorListWindow;
        private DummyTaskList m_taskList;
        private bool _showSplash;
        private SplashScreen _splashScreen;
        private object m_resourceUserObject = new object();
        private CommandLineOptions m_commandLineOptions = null;
        private IScriptExecution m_mainScriptExecution = null;

        public MainForm()
        {
            Instance = this;
            this.InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;

            StepBro.Core.Main.Initialize();

            //this.SeSBPlashScreen();
            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, Environment.GetCommandLineArgs());

            this.CreateStandardControls();

            m_solutionExplorer.RightToLeftLayout = this.RightToLeftLayout;
            m_deserializeDockContent = new DeserializeDockContent(this.GetContentFromPersistString);

            vsToolStripExtender.DefaultRenderer = _toolStripProfessionalRenderer;
            dockPanel.DocumentStyle = DocumentStyle.DockingMdi;
        }

        #region Interface

        internal static MainForm Instance { get; private set; }

        public void ShowFileEditor(ILoadedFile file, int line, int selectionStart, int selectionEnd)
        {
            foreach (var doc in dockPanel.Documents)
            {
                if (doc.DockHandler.Content is DocumentViewDockContent)
                {
                    var view = doc.DockHandler.Content as DocumentViewDockContent;
                    if (view.File == file)
                    {
                        this.ShowDocView(view);
                        if (line >= 0)
                        {
                            view.ShowLine(line, selectionStart, selectionEnd);
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private IDockContent FindDocument(string text)
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in this.MdiChildren)
                    if (form.Text == text)
                        return form as IDockContent;

                return null;
            }
            else
            {
                foreach (IDockContent content in dockPanel.Documents)
                    if (content.DockHandler.TabText == text)
                        return content;

                return null;
            }
        }

        private TextDocView CreateNewDocument()
        {
            TextDocView dummyDoc = new TextDocView();

            int count = 1;
            string text = $"Document{count}";
            while (this.FindDocument(text) != null)
            {
                count++;
                text = $"Document{count}";
            }

            dummyDoc.Text = text;
            return dummyDoc;
        }

        private TextDocView CreateNewDocument(string text)
        {
            TextDocView dummyDoc = new TextDocView();
            dummyDoc.Text = text;
            return dummyDoc;
        }

        private void CloseAllDocuments()
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                foreach (Form form in this.MdiChildren)
                    form.Close();
            }
            else
            {
                foreach (IDockContent document in dockPanel.DocumentsToArray())
                {
                    // IMPORANT: dispose all panes.
                    document.DockHandler.DockPanel = null;
                    document.DockHandler.Close();
                }
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            var found = AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(a => a.GetExportedTypes()).
                Where(t => String.Equals(t.FullName, persistString, StringComparison.InvariantCulture));
            if (persistString == typeof(FileExplorer).ToString())
                return m_solutionExplorer;
            else if (persistString == typeof(DummyPropertyWindow).ToString())
                return m_propertyWindow;
            else if (persistString == typeof(DummyToolbox).ToString())
                return m_toolbox;
            else if (persistString == typeof(OutputWindow).ToString())
                return m_outputWindow;
            else if (persistString == typeof(ErrorsWindow).ToString())
                return m_errorListWindow;
            else if (persistString == typeof(DummyTaskList).ToString())
                return m_taskList;
            else
            {
                // DummyDoc overrides GetPersistString to add extra information into persistString.
                // Any DockContent may override this value to add any needed information for deserialization.

                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length != 3)
                    return null;

                if (parsedStrings[0] == typeof(TextDocView).ToString())
                {
                    TextDocView doc = new TextDocView(m_resourceUserObject);
                    if (parsedStrings[1] != string.Empty)
                    {
                        if (!doc.OpenFile(parsedStrings[1]))
                        {
                            return null;
                        }
                    }
                    if (parsedStrings[2] != string.Empty)
                        doc.Text = parsedStrings[2];

                    return doc;
                }
                else if (parsedStrings[0] == typeof(StepBroScriptDocView).ToString())
                {
                    StepBroScriptDocView doc = new StepBroScriptDocView(m_resourceUserObject);
                    if (parsedStrings[1] != string.Empty)
                    {
                        if (!doc.OpenFile(parsedStrings[1]))
                        {
                            return null;
                        }
                    }
                    if (parsedStrings[2] != string.Empty)
                        doc.Text = parsedStrings[2];

                    return doc;
                }
                else if (parsedStrings[0] == ObjectPanelDockWindow.PersistTitle)
                {
                    var view = new ObjectPanelDockWindow(StepBroMain.ServiceManager);
                    view.SetupFromLoadSpecification(parsedStrings.Skip(1).ToArray());
                    return view;
                }
                else
                {
                    return null;
                }
            }
        }

        private void CloseAllContents()
        {
            // we don't want to create another instance of tool window, set DockPanel to null
            m_solutionExplorer.DockPanel = null;
            m_propertyWindow.DockPanel = null;
            m_toolbox.DockPanel = null;
            m_outputWindow.DockPanel = null;
            m_errorListWindow.DockPanel = null;
            m_taskList.DockPanel = null;

            // Close all other document windows
            this.CloseAllDocuments();

            // IMPORTANT: dispose all float windows.
            foreach (var window in dockPanel.FloatWindows.ToList())
                window.Dispose();

            System.Diagnostics.Debug.Assert(dockPanel.Panes.Count == 0);
            System.Diagnostics.Debug.Assert(dockPanel.Contents.Count == 0);
            System.Diagnostics.Debug.Assert(dockPanel.FloatWindows.Count == 0);
        }

        private readonly ToolStripRenderer _toolStripProfessionalRenderer = new ToolStripProfessionalRenderer();

        private void EnableVSRenderer(VisualStudioToolStripExtender.VsVersion version, ThemeBase theme)
        {
            vsToolStripExtender.SetStyle(mainMenu, version, theme);
            vsToolStripExtender.SetStyle(toolBar, version, theme);
            vsToolStripExtender.SetStyle(toolStripExecution, version, theme);
            vsToolStripExtender.SetStyle(statusBar, version, theme);
        }

        #endregion

        #region Event Handlers

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private void menuItemExit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void menuItemSolutionExplorer_Click(object sender, System.EventArgs e)
        {
            m_solutionExplorer.Show(dockPanel);
        }

        private void menuItemPropertyWindow_Click(object sender, System.EventArgs e)
        {
            m_propertyWindow.Show(dockPanel);
        }

        private void menuItemToolbox_Click(object sender, System.EventArgs e)
        {
            m_toolbox.Show(dockPanel);
        }

        private void menuItemErrorList_Click(object sender, EventArgs e)
        {
            m_errorListWindow.Show(dockPanel);
        }

        private void menuItemOutputWindow_Click(object sender, System.EventArgs e)
        {
            m_outputWindow.Show(dockPanel);
        }

        private void menuItemTaskList_Click(object sender, System.EventArgs e)
        {
            m_taskList.Show(dockPanel);
        }

        private void menuItemAbout_Click(object sender, System.EventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog(this);
        }

        private void menuItemNew_Click(object sender, System.EventArgs e)
        {
            TextDocView dummyDoc = this.CreateNewDocument();
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                dummyDoc.MdiParent = this;
                dummyDoc.Show();
            }
            else
            {
                dummyDoc.Show(dockPanel);
            }
        }

        private void menuItemOpen_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.InitialDirectory = Application.ExecutablePath;
            openFile.Filter = "StepBro script files (*." + Main.StepBroFileExtension + ")|*." + Main.StepBroFileExtension + "|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFile.FilterIndex = 1;
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                var fullName = openFile.FileName;
                var fileName = Path.GetFileName(fullName);
                var extension = Path.GetExtension(fullName);

                if (this.FindDocument(fileName) != null)
                {
                    MessageBox.Show("The document: " + fileName + " is already opened!");
                    return;
                }

                if (extension == "." + Main.StepBroFileExtension)
                {
                    var docView = new StepBroScriptDocView(m_resourceUserObject)
                    {
                        Text = fileName
                    };
                    this.ShowDocView(docView);
                    try
                    {
                        docView.OpenFile(fullName);
                    }
                    catch (Exception exception)
                    {
                        docView.Close();
                        MessageBox.Show(exception.Message);
                    }
                }
                else
                {
                    var docView = new TextDocView
                    {
                        Text = fileName
                    };
                    this.ShowDocView(docView);
                    try
                    {
                        docView.OpenFile(fullName);
                    }
                    catch (Exception exception)
                    {
                        docView.Close();
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }

        private void ShowDocView(DockContent view)
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                view.MdiParent = this;
                view.Show();
            }
            else
            {
                view.Show(dockPanel);
            }
        }

        private void menuItemFile_Popup(object sender, System.EventArgs e)
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                menuItemClose.Enabled =
                    menuItemCloseAll.Enabled =
                    menuItemCloseAllButThisOne.Enabled = (this.ActiveMdiChild != null);
            }
            else
            {
                menuItemClose.Enabled = (dockPanel.ActiveDocument != null);
                menuItemCloseAll.Enabled =
                    menuItemCloseAllButThisOne.Enabled = (dockPanel.DocumentsCount > 0);
            }
        }

        private void menuItemClose_Click(object sender, System.EventArgs e)
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
                this.ActiveMdiChild.Close();
            else if (dockPanel.ActiveDocument != null)
                dockPanel.ActiveDocument.DockHandler.Close();
        }

        private void menuItemCloseAll_Click(object sender, System.EventArgs e)
        {
            this.CloseAllDocuments();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            dockPanel.Theme = vS2015BlueTheme;
            this.EnableVSRenderer(VisualStudioToolStripExtender.VsVersion.Vs2015, vS2015BlueTheme);

            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DockConfigFile);

            if (File.Exists(configFile))
                dockPanel.LoadFromXml(configFile, m_deserializeDockContent);
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DockConfigFile);
            if (m_bSaveLayout)
            {
                dockPanel.SaveAsXml(configFile);
            }
            //else if (File.Exists(configFile))
            //    File.Delete(configFile);
        }

        private void menuItemToolBar_Click(object sender, System.EventArgs e)
        {
            toolBar.Visible = menuItemToolBar.Checked = !menuItemToolBar.Checked;
        }

        private void executionToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripExecution.Visible = menuItemExecutionToolBar.Checked = !menuItemExecutionToolBar.Checked;
        }

        private void menuItemStatusBar_Click(object sender, System.EventArgs e)
        {
            statusBar.Visible = menuItemStatusBar.Checked = !menuItemStatusBar.Checked;
        }

        private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == toolBarButtonNew)
                this.menuItemNew_Click(null, null);
            else if (e.ClickedItem == toolBarButtonOpen)
                this.menuItemOpen_Click(null, null);
            else if (e.ClickedItem == toolBarButtonSolutionExplorer)
                this.menuItemSolutionExplorer_Click(null, null);
            else if (e.ClickedItem == toolBarButtonPropertyWindow)
                this.menuItemPropertyWindow_Click(null, null);
            else if (e.ClickedItem == toolBarButtonToolbox)
                this.menuItemToolbox_Click(null, null);
            else if (e.ClickedItem == toolBarButtonOutputWindow)
                this.menuItemOutputWindow_Click(null, null);
            else if (e.ClickedItem == toolBarButtonTaskList)
                this.menuItemTaskList_Click(null, null);
            else if (e.ClickedItem == toolBarButtonLayoutByCode)
                this.menuItemLayoutByCode_Click(null, null);
            else if (e.ClickedItem == toolBarButtonLayoutByXml)
                this.menuItemLayoutByXml_Click(null, null);
        }

        private void menuItemNewWindow_Click(object sender, System.EventArgs e)
        {
            MainForm newWindow = new MainForm();
            newWindow.Text = newWindow.Text + " - New";
            newWindow.Show();
        }

        private void menuItemTools_Popup(object sender, System.EventArgs e)
        {
            menuItemLockLayout.Checked = !dockPanel.AllowEndUserDocking;
        }

        private void menuItemLockLayout_Click(object sender, System.EventArgs e)
        {
            dockPanel.AllowEndUserDocking = !dockPanel.AllowEndUserDocking;
        }

        private void menuItemLayoutByCode_Click(object sender, System.EventArgs e)
        {
            dockPanel.SuspendLayout(true);

            this.CloseAllContents();

            this.CreateStandardControls();

            m_solutionExplorer.Show(dockPanel, DockState.DockRight);
            m_propertyWindow.Show(m_solutionExplorer.Pane, m_solutionExplorer);
            m_toolbox.Show(dockPanel, new Rectangle(98, 133, 200, 383));
            m_outputWindow.Show(m_solutionExplorer.Pane, DockAlignment.Bottom, 0.35);
            m_taskList.Show(m_toolbox.Pane, DockAlignment.Left, 0.4);

            TextDocView doc1 = this.CreateNewDocument("Document1");
            TextDocView doc2 = this.CreateNewDocument("Document2");
            TextDocView doc3 = this.CreateNewDocument("Document3");
            TextDocView doc4 = this.CreateNewDocument("Document4");
            doc1.Show(dockPanel, DockState.Document);
            doc2.Show(doc1.Pane, null);
            doc3.Show(doc1.Pane, DockAlignment.Bottom, 0.5);
            doc4.Show(doc3.Pane, DockAlignment.Right, 0.5);

            dockPanel.ResumeLayout(true, true);
        }

        private void SeSBPlashScreen()
        {

            _showSplash = true;
            _splashScreen = new SplashScreen();

            this.ResizeSplash();
            _splashScreen.Visible = true;
            _splashScreen.TopMost = true;

            Timer _timer = new Timer();
            _timer.Tick += (sender, e) =>
            {
                _splashScreen.Visible = false;
                _timer.Enabled = false;
                _showSplash = false;
            };
            _timer.Interval = 4000;
            _timer.Enabled = true;
        }

        private void ResizeSplash()
        {
            if (_showSplash)
            {

                var centerXMain = (this.Location.X + this.Width) / 2.0;
                var LocationXSplash = Math.Max(0, centerXMain - (_splashScreen.Width / 2.0));

                var centerYMain = (this.Location.Y + this.Height) / 2.0;
                var LocationYSplash = Math.Max(0, centerYMain - (_splashScreen.Height / 2.0));

                _splashScreen.Location = new Point((int)Math.Round(LocationXSplash), (int)Math.Round(LocationYSplash));
            }
        }

        private void CreateStandardControls()
        {
            m_solutionExplorer = new FileExplorer();
            m_propertyWindow = new DummyPropertyWindow();
            m_toolbox = new DummyToolbox();
            m_errorListWindow = new ErrorsWindow();
            m_outputWindow = new OutputWindow();
            m_taskList = new DummyTaskList();
        }

        private void menuItemLayoutByXml_Click(object sender, System.EventArgs e)
        {
            dockPanel.SuspendLayout(true);

            // In order to load layout from XML, we need to close all the DockContents
            this.CloseAllContents();

            this.CreateStandardControls();

            Assembly assembly = Assembly.GetAssembly(typeof(MainForm));
            Stream xmlStream = assembly.GetManifestResourceStream("DockSample.Resources.DockPanel.xml");
            dockPanel.LoadFromXml(xmlStream, m_deserializeDockContent);
            xmlStream.Close();

            dockPanel.ResumeLayout(true, true);
        }

        private void menuItemCloseAllButThisOne_Click(object sender, System.EventArgs e)
        {
            if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
            {
                Form activeMdi = this.ActiveMdiChild;
                foreach (Form form in this.MdiChildren)
                {
                    if (form != activeMdi)
                        form.Close();
                }
            }
            else
            {
                foreach (IDockContent document in dockPanel.DocumentsToArray())
                {
                    if (!document.DockHandler.IsActivated)
                        document.DockHandler.Close();
                }
            }
        }

        private void menuItemShowDocumentIcon_Click(object sender, System.EventArgs e)
        {
            dockPanel.ShowDocumentIcon = menuItemShowDocumentIcon.Checked = !menuItemShowDocumentIcon.Checked;
        }

        private void exitWithoutSavingLayout_Click(object sender, EventArgs e)
        {
            m_bSaveLayout = false;
            this.Close();
            m_bSaveLayout = true;
        }

        #endregion

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            this.ResizeSplash();
        }

        private void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.StartButtonPressed();
        }

        private void StartButtonPressed()
        {
            if (m_mainScriptExecution == null)
            {
                if (toolStripComboBoxExecutionTarget.SelectedIndex >= 0)
                {
                    var parsing = StepBroMain.StartFileParsing(false);
                    if (parsing != null)
                    {
                        Task.Run(() =>
                        {
                            parsing.AsyncWaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                            this.BeginInvoke(new Action(this.OnReadyForExecuteSelectedProcedure));
                        });
                    }
                    else
                    {
                        this.OnReadyForExecuteSelectedProcedure();
                    }
                }
            }
            else
            {
                if (m_mainScriptExecution.Task.CurrentState == StepBro.Core.Tasks.TaskExecutionState.Paused)
                {
                    m_mainScriptExecution.Task.RequestContinue();
                }
            }
        }

        private void OnReadyForExecuteSelectedProcedure()
        {
            if (StepBroMain.LastParsingErrorCount == 0)
            {
                this.ExecuteSelectedProcedure();
            }
            else
            {
                MessageBox.Show(
                    this,
                    "Could not start execution because of one or more parsing errors.",
                    "Error starting execution", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnReadyForExecuteProcedureUnderCursor()
        {
            if (StepBroMain.LastParsingErrorCount == 0)
            {
                this.ExecuteProcedureUnderCursor();
            }
            else
            {
                MessageBox.Show(
                    this,
                    "Could not start execution because of one or more parsing errors.",
                    "Error starting execution", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteSelectedProcedure()
        {
            var selected = toolStripComboBoxExecutionTarget.SelectedItem as ComboboxItem;
            if (selected != null)
            {
                IFileElement element = Main.TryFindFileElement(selected.Value as string);
                if (element != null)
                {
                    m_mainScriptExecution = StepBro.Core.Main.StartProcedureExecution(element as IFileProcedure);
                    m_mainScriptExecution.Task.CurrentStateChanged += this.CurrentScriptExeccution_CurrentStateChanged;
                }
            }
        }

        private void ExecuteProcedureUnderCursor()
        {
            // Temporary solution !!!!
            // Get the file element closest to the current selection (or the cursor), and execute that.
            var active = dockPanel.ActiveDocument.DockHandler.Content;
            if (active != null && active is StepBroScriptDocView)
            {
                var view = active as StepBroScriptDocView;
                var file = view.ScriptFile;
                IFileElement element = null;
                var selectionLine = view.SelectionStartLine + 1;    // Make line number 1-based
                foreach (var fe in file.ListElements())
                {
                    if (fe.Line <= selectionLine && (element == null || element.Line < fe.Line))
                    {
                        element = fe;
                    }
                }
                if (element != null && element is IFileProcedure)
                {
                    m_mainScriptExecution = StepBro.Core.Main.StartProcedureExecution(element as IFileProcedure);
                    m_mainScriptExecution.Task.CurrentStateChanged += this.CurrentScriptExeccution_CurrentStateChanged;
                }
            }
        }

        private void CurrentScriptExeccution_CurrentStateChanged(object sender, EventArgs e)
        {
            switch (m_mainScriptExecution.Task.CurrentState)
            {
                case StepBro.Core.Tasks.TaskExecutionState.Created:
                    this.BeginInvoke(new Action(this.OnScriptExecutionRunning));
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.Started:
                case StepBro.Core.Tasks.TaskExecutionState.AwaitingStartCondition:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.Running:
                    this.BeginInvoke(new Action(this.OnScriptExecutionRunning));
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.RunningNotResponding:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.PauseRequested:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.Paused:
                    this.BeginInvoke(new Action(this.OnScriptExecutionPaused));
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.StopRequested:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.KillRequested:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.Terminating:
                    break;
                case StepBro.Core.Tasks.TaskExecutionState.Ended:
                    this.BeginInvoke(new Action(this.OnScriptExecutionEnded));
                    break;
                default:
                    break;
            }
        }

        private void toolStripButtonRun_Click(object sender, EventArgs e)
        {
            this.StartButtonPressed();
        }

        private void startElementSelectedInEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_mainScriptExecution == null)
            {
                var parsing = StepBroMain.StartFileParsing(false);
                if (parsing != null)
                {
                    Task.Run(() =>
                    {
                        parsing.AsyncWaitHandle.WaitOne(TimeSpan.FromMinutes(5));
                        this.BeginInvoke(new Action(this.OnReadyForExecuteProcedureUnderCursor));
                    });
                }
                else
                {
                    this.ExecuteProcedureUnderCursor();
                }
            }
            else
            {
                if (m_mainScriptExecution.Task.CurrentState == StepBro.Core.Tasks.TaskExecutionState.Paused)
                {
                    m_mainScriptExecution.Task.RequestContinue();
                }
            }
        }

        private void OnScriptExecutionRunning()
        {
            toolStripButtonRun.Enabled = false;
            toolStripButtonStop.Enabled = true;
            toolStripButtonPause.Enabled = true;
            toolStripButtonStepInto.Enabled = false;
            toolStripButtonStepOver.Enabled = false;
            toolStripButtonStepOut.Enabled = false;
        }
        private void OnScriptExecutionPaused()
        {
            toolStripButtonRun.Enabled = true;
            toolStripButtonStop.Enabled = true;
            toolStripButtonPause.Enabled = false;
            toolStripButtonStepInto.Enabled = true;
            toolStripButtonStepOver.Enabled = true;
            toolStripButtonStepOut.Enabled = true;
        }
        private void OnScriptExecutionEnded()
        {
            m_mainScriptExecution.Task.CurrentStateChanged -= this.CurrentScriptExeccution_CurrentStateChanged;
            m_mainScriptExecution = null;
            toolStripButtonRun.Enabled = true;
            toolStripButtonStop.Enabled = false;
            toolStripButtonPause.Enabled = false;
            toolStripButtonStepInto.Enabled = false;
            toolStripButtonStepOver.Enabled = false;
            toolStripButtonStepOut.Enabled = false;
        }

        private void parseAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StepBroMain.StartFileParsing(false);
        }

        private void toolStripMenuItemListCurrentObjects_Click(object sender, EventArgs e)
        {
            StepBroMain.DumpCurrentObjectsToLog();
        }

        private void toolStripMenuItemCreateAllClassPanels_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItemCreateAllObjectPanels_Click(object sender, EventArgs e)
        {
            var objects = StepBroMain.GetService<Core.Data.IDynamicObjectManager>().ListKnownObjects().ToList();
            var panelManager = StepBroMain.GetService<Core.Data.IObjectPanelManager>();
            foreach (var panelInfo in panelManager.ListPanelTypes().Where(t => t.IsObjectPanel))
            {
                foreach (var objContainer in objects)
                {
                    if (objContainer.Object != null && panelInfo.IsCompatibleWithObject(objContainer.Object))
                    {
                        if (panelManager.GetPanelCreationOption(panelInfo, objContainer) == Core.Data.PanelCreationOption.Possible)
                        {
                            var panel = panelManager.CreatePanel(panelInfo, objContainer);
                            if (panel != null)
                            {
                                var window = new ObjectPanelDockWindow(StepBroMain.ServiceManager);
                                window.SetPanel(panel);
                                window.Show(dockPanel);
                            }
                        }
                    }
                }
            }
        }
    }
}