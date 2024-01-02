using StepBro.Sidekick;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using static StepBro.ConsoleSidekick.WinForms.MainForm.FileData;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class MainForm : Form
    {
        private Control m_topControl = null;
        private nint m_consoleWindow = 0;
        private bool m_firstWindowMove = true;
        private SideKickPipe m_pipe = null;
        private Rect m_lastConsolePosition = new Rect();
        private List<FileData> m_files = new List<FileData>();
        private FileData m_selectedFile = null;
        private FileData.Element m_selectedElement = null;
        private string m_selectedPartner = null;
        private bool m_scriptExecuting = false;

        public class FileData
        {
            public class Element
            {
                public string Name { get; set; }
                public string Type { get; set; }
                public string[] Partners { get; set; }
            }

            public string File { get; set; }
            public List<Element> Elements { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        // TODO: https://stackoverflow.com/questions/1732140/displaying-tooltip-over-a-disabled-control

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            System.Diagnostics.Trace.WriteLine("Sidekick STARTING!!");

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 0)
            {

            }
            else
            {
                m_consoleWindow = nint.Parse(args[1], System.Globalization.NumberStyles.HexNumber);
            }

            m_topControl = this.TopLevelControl;


            //while (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    Thread.Sleep(100);
            //}

            m_pipe = SideKickPipe.StartClient(args[1]);
        }

        private void MoveWindows()
        {
            Rect rectConsole;
            if (DwmGetWindowAttribute(m_consoleWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out rectConsole, Marshal.SizeOf(typeof(Rect))) != 0)
            {
                GetWindowRect(m_consoleWindow, ref rectConsole);
            }

            if (!rectConsole.Equals(m_lastConsolePosition))
            {
                if (m_firstWindowMove)
                {
                    MoveWindow(m_consoleWindow, rectConsole.Left, 0, rectConsole.Right - rectConsole.Left, rectConsole.Bottom - rectConsole.Top, true);
                    if (DwmGetWindowAttribute(m_consoleWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out rectConsole, Marshal.SizeOf(typeof(Rect))) != 0)
                    {
                        GetWindowRect(m_consoleWindow, ref rectConsole);
                    }
                    m_firstWindowMove = false;
                }
                m_lastConsolePosition = rectConsole;

                m_topControl.Top = rectConsole.Bottom;
                m_topControl.Left = rectConsole.Left;
                m_topControl.Width = rectConsole.Right - rectConsole.Left;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Sidekick closing!!");
            if (m_pipe != null)
            {
                m_pipe.Dispose();
                m_pipe = null;
            }
            base.OnFormClosing(e);
            System.Diagnostics.Trace.WriteLine("Sidekick closing end");
        }

        #region CONSOLE INTERACTION

        private void ExecuteCommand(string command)
        {
            m_pipe.Send(new ObjectCommand(comboBoxConnection.Items[comboBoxConnection.SelectedIndex] as string, command));
        }

        #endregion

        #region GUI STUFF

        private void comboBoxCommand_TextUpdate(object sender, EventArgs e)
        {

        }

        #endregion

        #region USER INTERACTION - COMMANDS

        private void comboBoxConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void comboBoxCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void buttonMenu_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                if (!String.IsNullOrEmpty(comboBoxCommand.Text))
                {
                    ExecuteCommand(comboBoxCommand.Text);
                    comboBoxCommand.Select(0, comboBoxCommand.Text.Length);
                }
            }
        }

        private void comboBoxCommand_TextChanged(object sender, EventArgs e)
        {
            buttonExecute.Enabled = !String.IsNullOrEmpty(comboBoxCommand.Text);
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("buttonExecute_Click");
            if (!String.IsNullOrEmpty(comboBoxCommand.Text))
            {
                ExecuteCommand(comboBoxCommand.Text);
                comboBoxCommand.Select(0, comboBoxCommand.Text.Length);
            }
        }

        #endregion

        #region USER INTERACTION - EXECUTION

        private void comboBoxScriptFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateFromFileSelection();
        }

        private void UpdateFromFileSelection()
        {
            var file = m_files.FirstOrDefault(fi => fi.File == (string)comboBoxScriptFile.SelectedItem);
            if (file != null && comboBoxScriptFile.SelectedIndex >= 0 && (m_selectedFile == null || file.File != m_selectedFile.File))
            {
                m_selectedFile = file;
                m_selectedElement = null;
                m_selectedPartner = null;
                comboBoxFileElement.Items.Clear();
                comboBoxFileElement.SelectedIndex = -1;
                if (file != null && file.Elements != null)
                {
                    comboBoxFileElement.Items.AddRange(file.Elements.Select(e => e.Name).ToArray());
                }
                comboBoxFileElement.Enabled = comboBoxFileElement.Items.Count > 0;
                buttonRunScript.Enabled = comboBoxFileElement.Enabled;
                if (comboBoxFileElement.Items.Count > 0)
                {
                    comboBoxFileElement.SelectedIndex = 0;
                }
            }
        }

        private void comboBoxFileElement_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFromElementSelection();
        }

        private void UpdateFromElementSelection()
        {
            if (comboBoxFileElement.SelectedIndex >= 0 && (m_selectedElement == null || (string)comboBoxFileElement.SelectedItem != m_selectedElement.Name))
            {
                var file = m_files.FirstOrDefault(fi => fi.File == m_selectedFile.File);
                m_selectedElement = file.Elements.FirstOrDefault(fi => fi.Name == (string)comboBoxFileElement.SelectedItem);
                m_selectedPartner = null;
                comboBoxPartner.Items.Clear();
                comboBoxPartner.SelectedIndex = -1;
                comboBoxPartner.Items.Add("<direct / no partner>");
                if (m_selectedElement.Partners != null)
                {
                    comboBoxPartner.Items.AddRange(m_selectedElement.Partners);
                }
                comboBoxPartner.Enabled = comboBoxFileElement.Items.Count > 1;
                comboBoxPartner.SelectedIndex = 0;
            }
        }

        private void comboBoxPartner_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPartner.SelectedIndex > 0)
            {
                m_selectedPartner = (string)comboBoxPartner.SelectedItem;
            }
            else
            {
                m_selectedPartner = null;
            }
        }

        private void buttonRunScript_Click(object sender, EventArgs e)
        {
            if (m_scriptExecuting)
            {
                m_pipe.Send(ShortCommand.StopScriptExecution);
            }
            else
            {
                m_pipe.Send(new RunScriptRequest(m_selectedFile.File, m_selectedElement.Name, m_selectedPartner));
            }
        }

        #endregion

        #region WIN32

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public override bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj is Rect)
                {
                    var objRect = (Rect)obj;
                    return (objRect.Left == this.Left && objRect.Top == this.Top && objRect.Right == this.Right && objRect.Bottom == this.Bottom);
                }
                return base.Equals(obj);
            }
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Rect pvAttribute, int cbAttribute);

        #endregion

        private void timerMasterPull_Tick(object sender, EventArgs e)
        {
            Tuple<string, string> received;
            while ((received = m_pipe.TryGetReceived()) != null)
            {
                if (received.Item1 == "ShortCommand")
                {
                    var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                    if (cmd == ShortCommand.Close)
                    {
                        m_pipe.Send(cmd);   // Send back, to make console continue the closing process.
                        Thread.Sleep(100);
                        m_pipe.Dispose();
                        this.Close();
                    }
                    else if(cmd == ShortCommand.ExecutionStarted) 
                    {
                        buttonRunScript.Text = "Stop";
                        m_scriptExecuting = true;
                    }
                    else if (cmd == ShortCommand.ExecutionStopped)
                    {
                        buttonRunScript.Text = "Run";
                        m_scriptExecuting = false;
                    }
                }
                else if (received.Item1 == "CommandObjectsList")
                {
                    var commandObjects = JsonSerializer.Deserialize<CommandObjectsList>(received.Item2);
                    var objects = commandObjects.Objects;
                    comboBoxConnection.Items.Clear();
                    comboBoxConnection.Items.AddRange(objects);
                    comboBoxConnection.SelectedIndex = 0;
                    comboBoxConnection.Enabled = objects.Length > 0;
                    comboBoxCommand.Enabled = objects.Length > 0;
                }
                else if (received.Item1 == "LoadedFiles")
                {
                    var loadedFiles = JsonSerializer.Deserialize<LoadedFiles>(received.Item2);
                    var files = loadedFiles.Files;
                    string selected = (comboBoxScriptFile.Items.Count > 0 && comboBoxScriptFile.SelectedIndex >= 0) ? comboBoxScriptFile.SelectedItem.ToString() : null;
                    comboBoxScriptFile.Items.Clear();
                    comboBoxScriptFile.Items.AddRange(files);
                    comboBoxScriptFile.Enabled = files.Length > 0;
                    int index = 0;
                    if (selected != null)
                    {
                        int i = 0;
                        foreach (var f in files)
                        {
                            if (String.Equals(f, selected)) break;
                        }
                        if (i < files.Length) index = i;
                    }
                    comboBoxScriptFile.SelectedIndex = index;

                    var fileInfo = new List<FileData>();
                    foreach (var f in files)
                    {
                        var found = m_files.FirstOrDefault(fi => fi.File == f);
                        if (found == null)
                        {
                            found = new FileData();
                            found.File = f;
                        }
                        fileInfo.Add(found);
                    }
                    m_files = fileInfo; // Use this updated list.

                }
                else if (received.Item1 == "FileElements")
                {
                    var fileElements = JsonSerializer.Deserialize<FileElements>(received.Item2);

                    var file = m_files.FirstOrDefault(fi => fi.File == fileElements.File);
                    List<Element> updatedElements = new List<Element>();
                    for (int i = 0; i < fileElements.ElementNames.Length; i++)
                    {
                        var element = file.Elements?.FirstOrDefault(e => e.Name == fileElements.ElementNames[i]);
                        if (element == null)
                        {
                            element = new FileData.Element();
                        }
                        element.Name = fileElements.ElementNames[i];
                        element.Type = fileElements.ElementTypes[i];
                        element.Partners = fileElements.Partners[i];
                        updatedElements.Add(element);
                    }
                    file.Elements = updatedElements;

                    if ((string)comboBoxScriptFile.SelectedItem == file.File)
                    {
                        UpdateFromFileSelection();
                    }
                }
                //else if (received.Item1 == "ElementInfo")
                //{
                //    var info = JsonSerializer.Deserialize<ElementInfo>(received.Item2);
                //}
            }

            MoveWindows();
        }
    }
}
