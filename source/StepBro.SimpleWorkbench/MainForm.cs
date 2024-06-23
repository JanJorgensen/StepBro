using ActiproSoftware.UI.WinForms.Controls.Docking;
using ActiproSoftware.UI.WinForms.Drawing;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.UI.WinForms.Controls;
using System.Text;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.SimpleWorkbench
{
    public partial class MainForm : Form
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

        private ToolWindow toolWindowExecutionLog = null;
        private LogViewer m_logviewer = null;
        //private Core.Controls.ParsingErrorListView parsingErrorListView;

        public MainForm()
        {
            InitializeComponent();

            toolStripMainMenu.Text = "\u2630";

            //toolWindowProperties.Close();
            //toolWindowHelp.Close();

            this.CreateTextDocument(null, "This is a read-only document.  Notice the lock context image in the tab.", true).Activate();
            this.CreateTextDocument(null, null, false).Activate();

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
            toolWindowExecutionLog = new ToolWindow(dockManager, "Execution Log", "Execution Log", null, m_logviewer);
            toolWindowExecutionLog.DockTo(dockManager, DockOperationType.RightOuter);
            toolWindowExecutionLog.State = ToolWindowState.TabbedDocument;
            m_logviewer.Setup();

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

        #region View Menu

        private void toolStripMenuItemView_DropDownOpened(object sender, EventArgs e)
        {
            //viewExecutionLogToolStripMenuItem.Checked = toolWindowExecutionLog.Active;
            //viewPropertiesToolStripMenuItem.Checked = toolWindowProperties.Active;
        }

        private void viewExecutionLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewExecutionLogToolStripMenuItem.Checked)
            {
                toolWindowExecutionLog.Activate();
            }
            else
            {
                toolWindowExecutionLog.Close();
            }
        }

        private void viewErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void viewObjectCommandPromptToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void viewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewPropertiesToolStripMenuItem.Checked)
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
    }
}
