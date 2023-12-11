using StepBro.Sidekick;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class MainForm : Form
    {
        private Control m_topControl = null;
        private nint m_consoleWindow = 0;
        private SideKickPipe m_pipe = null;
        private Rect m_lastConsolePosition = new Rect();

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
            Rect rectConsole = new Rect();
            GetWindowRect(m_consoleWindow, ref rectConsole);

            if (!rectConsole.Equals(m_lastConsolePosition))
            {
                MoveWindow(m_consoleWindow, rectConsole.Left, 0, rectConsole.Right - rectConsole.Left, rectConsole.Bottom - rectConsole.Top, true);
                GetWindowRect(m_consoleWindow, ref rectConsole);
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
            System.Diagnostics.Debug.WriteLine("comboBoxCommand_TextUpdate");

        }

        #endregion

        #region USER INTERACTION - COMMANDS

        private void comboBoxConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("comboBoxConnection_SelectedIndexChanged");
        }

        private void comboBoxCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("comboBoxCommand_SelectedIndexChanged");

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

        }

        private void comboBoxFileElement_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxPartner_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonRunScript_Click(object sender, EventArgs e)
        {

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

        #endregion

        private void timerMasterPull_Tick(object sender, EventArgs e)
        {
            Tuple<string, string> received;
            while ((received = m_pipe.TryGetReceived()) != null)
            {
                if (received.Item1 == "ShortCommand")
                {
                    var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                    if (cmd != null && cmd.Command == "CLOSE")
                    {
                        m_pipe.Send(cmd);   // Send back, to make console continue the closing process.
                        m_pipe.Dispose();   // Maybe too pushy...
                        this.Close();
                    }
                }
                else if (received.Item1 == "CommandObjectsList")
                {
                    var commandObjects = JsonSerializer.Deserialize<CommandObjectsList>(received.Item2);
                    var objects = commandObjects.Objects;
                    comboBoxConnection.Items.Clear();
                    comboBoxConnection.Items.AddRange(objects);
                    comboBoxConnection.SelectedIndex = 0;
                    comboBoxConnection.Enabled = true;
                    comboBoxCommand.Enabled = objects.Length > 0;
                }
                else if (received.Item1 == "LoadedFiles")
                {
                    var loadedFiles = JsonSerializer.Deserialize<LoadedFiles>(received.Item2);
                }
                else if (received.Item1 == "FileElements")
                {
                    var fileElements = JsonSerializer.Deserialize<FileElements>(received.Item2);
                }
                else if (received.Item1 == "ElementInfo")
                {
                    var info = JsonSerializer.Deserialize<ElementInfo>(received.Item2);
                }
            }

            MoveWindows();
        }
    }
}
