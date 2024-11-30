using CommandLine;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.IPC;
using StepBro.Core.Data.SerializationHelp;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBro.Sidekick.Messages;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using StepBro.UI.WinForms;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class MainForm : Form, ICoreAccess, ILogger
    {
        private Control m_topControl = null;
        private nint m_consoleWindow = 0;
        private bool m_isConsoleActive = false;
        private bool m_forceResize = false;
        private bool m_moveToTop = true;
        private bool m_closeRequestedByConsole = false;
        private bool m_shouldAttach = true;
        private Pipe m_pipe = null;
        private Rect m_lastConsolePosition = new Rect();
        private IExecutionAccess m_executingScript = null;
        private PanelsDialog m_panelsDialog = null;
        private bool m_settingCommandCombo = false;
        private List<WeakReference<ExecutionAccess>> m_activeExecutions = new List<WeakReference<ExecutionAccess>>();
        private string m_topScriptFile = null;
        private List<Element> m_fileElements = null;
        private bool m_userFileRead = false;
        private string m_userFile = null;
        private object m_userShortcutItemTag = new object();
        private List<Tuple<string, StepBro.UI.WinForms.CustomToolBar.ToolBar>> m_customToolStrips = new List<Tuple<string, StepBro.UI.WinForms.CustomToolBar.ToolBar>>();
        private List<string> m_hiddenToolbars = new List<string>();

        private class ScriptExecutionToolStripMenuItem : ToolStripMenuItem
        {
            public ScriptExecutionToolStripMenuItem() { }

            public ScriptExecutionToolStripMenuItem(string element, string partner, string instanceObject)
            {
                FileElement = element;
                Partner = partner;
                InstanceObject = instanceObject;
            }

            public bool ShowFullName { get; set; } = false;
            public string FileElement { get; set; } = null;
            public string Partner { get; set; } = null;
            public string InstanceObject { get; set; } = null;

            public void SetText()
            {
                this.Text = MainForm.ScripExecutionButtonTitle(this.ShowFullName, this.FileElement, this.Partner, this.InstanceObject, null);
            }

            public bool Equals(string element, string partner, string instanceObject)
            {
                if (!String.Equals(element, this.FileElement, StringComparison.InvariantCulture)) return false;
                if (String.IsNullOrEmpty(partner) != String.IsNullOrEmpty(this.Partner)) return false;
                if (!String.Equals(partner, this.Partner)) return false;
                if (!String.Equals(instanceObject, this.InstanceObject)) return false;
                return true;
            }
        }

        private class ObjectCommandToolStripMenuItem : ToolStripMenuItem
        {
            public ObjectCommandToolStripMenuItem() { }

            public ObjectCommandToolStripMenuItem(string text, string instance, string command)
            {
                this.Text = text;
                this.Instance = instance;
                this.Command = command;
            }

            public string Instance { get; set; } = null;
            public new string Command { get; set; } = null;

            public bool Equals(string text, string instance, string command)
            {
                if (!String.Equals(text, this.Text, StringComparison.InvariantCulture)) return false;
                if (!String.Equals(instance, this.Instance, StringComparison.InvariantCulture)) return false;
                if (!String.Equals(command, this.Command, StringComparison.InvariantCulture)) return false;
                return true;
            }
        }

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
            toolStripButtonRunCommand.Text = "\u23F5";
            //toolStripButtonRunCommand.Text = "\u25B6";
            toolStripButtonStopScriptExecution.Text = "\u23F9";
            toolStripButtonAddShortcut.Text = "\u2795";
            toolStripDropDownButtonMainMenu.Text = "\u2630";

            this.UpdateToolbarVisibility();
        }

        // TODO: https://stackoverflow.com/questions/1732140/displaying-tooltip-over-a-disabled-control

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_topControl = this.TopLevelControl;

            System.Diagnostics.Trace.WriteLine("Sidekick STARTING!!");

            string[] args = Environment.GetCommandLineArgs();

            //MessageBox.Show("Say when ...", "Waiting");

            if (args.Length >= 2)
            {
                if (args.Length == 3)
                {
                    if (args[2].Equals("--no_attach"))
                    {
                        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                        m_shouldAttach = false;
                    }
                }
                m_consoleWindow = nint.Parse(args[1], System.Globalization.NumberStyles.HexNumber);
                m_pipe = Pipe.StartClient("StepBroConsoleSidekick", args[1]);
                m_pipe.OnConnectionClosed += (sender, e) =>
                {
                    m_closeRequestedByConsole = true; // We consider a connection closed to be a request by console
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        // close the form on the forms thread
                        this.Close();
                    });
                };
            }
            else
            {
                return;
            }

            this.UpdateToolbarVisibility();
            m_forceResize = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (m_closeRequestedByConsole)
            {
                System.Diagnostics.Trace.WriteLine("Sidekick base.OnFormClosing() - as requested");

                var userData = new UserDataCurrent();

                var shortcuts = new List<UserDataCurrent.Shortcut>();
                foreach (var shortcut in toolStripMain.Items.Cast<ToolStripItem>().Where(o => object.Equals(m_userShortcutItemTag, o.Tag)))
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

                userData.HiddenToolbars = (m_hiddenToolbars.Count > 0) ? m_hiddenToolbars.ToArray() : null;

                if (userData.Shortcuts != null || userData.PanelSettings != null || userData.HiddenToolbars != null)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                    using (FileStream createStream = File.Create(m_userFile))
                    {
                        JsonSerializer.Serialize(createStream, userData, options);
                    }
                }


                base.OnFormClosing(e);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Sidekick base.OnFormClosing() - requesting console app");
                if (m_pipe.IsConnected())
                {
                    m_pipe.Send(ShortCommand.RequestClose);
                }
                e.Cancel = true;    // Don't close; wait for close request from console.
            }
            System.Diagnostics.Trace.WriteLine("Sidekick closing end");
        }

        private void MoveWindows()
        {
            bool consoleActive = (GetForegroundWindow() == m_consoleWindow);
            if (consoleActive != m_isConsoleActive)
            {
                if (consoleActive)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    SetForegroundWindow(m_consoleWindow);
                }
                m_isConsoleActive = consoleActive;
            }
            if (m_forceResize || consoleActive)
            {
                Rect rectConsole = new Rect();
                if (DwmGetWindowAttribute(m_consoleWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out rectConsole, Marshal.SizeOf(typeof(Rect))) != 0)
                {
                    GetWindowRect(m_consoleWindow, ref rectConsole);
                }
                if (m_forceResize || !rectConsole.Equals(m_lastConsolePosition))
                {
                    if (m_moveToTop)
                    {
                        MoveWindow(m_consoleWindow, rectConsole.Left, 0, rectConsole.Right - rectConsole.Left, rectConsole.Bottom - rectConsole.Top, true);
                        if (DwmGetWindowAttribute(m_consoleWindow, DWMWA_EXTENDED_FRAME_BOUNDS, out rectConsole, Marshal.SizeOf(typeof(Rect))) != 0)
                        {
                            GetWindowRect(m_consoleWindow, ref rectConsole);
                        }
                        m_lastConsolePosition = rectConsole;
                        m_moveToTop = false;
                    }

                    m_topControl.Top = rectConsole.Bottom;
                    m_topControl.Left = rectConsole.Left;
                    m_topControl.Width = rectConsole.Right - rectConsole.Left;
                    if (m_forceResize)
                    {
                        m_topControl.Height = toolStripMain.Height + 40;
                    }
                }
            }
            m_forceResize = false;
        }

        #region USER INTERACTION - COMMANDS

        private void toolStripComboBoxToolCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ExecuteCommandFromGUI();
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
            ExecuteCommandFromGUI();
        }

        private void ExecuteCommandFromGUI()
        {
            if (!String.IsNullOrEmpty(toolStripComboBoxToolCommand.Text))
            {
                string command = toolStripComboBoxToolCommand.Text;
                ExecuteCommand(command);
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
            var tool = (toolStripComboBoxTool.Items[toolStripComboBoxTool.SelectedIndex] as Variable).FullName;
            this.ExecuteCommand(tool, command);
        }

        private void ExecuteCommand(string instance, string command)
        {
            // TODO: Show MessageBox with error message.
            m_pipe.Send(new ObjectCommand(instance, command));
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
                this.ExecuteCommand(executionEntry.Instance, executionEntry.Command);
            }
        }

        #endregion

        #region USER INTERACTION - EXECUTION

        private void FileElementExecutionEntry_Click(object sender, EventArgs e)
        {
            var executionEntry = sender as ScriptExecutionToolStripMenuItem;
            MenuFileElementExecutionStart(true, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
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
                MenuFileElementExecutionStart(false, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
            }
        }

        private void toolStripSplitButtonRunScript_ButtonClick(object sender, EventArgs e)
        {
            if (toolStripSplitButtonRunScript.Tag != null)
            {
                var executionEntry = toolStripSplitButtonRunScript.DropDownItems[0] as ScriptExecutionToolStripMenuItem;
                MenuFileElementExecutionStart(true, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
            }
        }

        private void MenuFileElementExecutionStart(bool addToHistory, string element, string model, string objectVariable, object[] args)
        {
            if (m_executingScript != null && m_executingScript.State == TaskExecutionState.Running)
            {
                return;
            }
            m_executingScript = ((ICoreAccess)this).StartExecution(element, model, objectVariable, args);
            m_executingScript.CurrentStateChanged += ExecutingScript_CurrentStateChanged;
            toolStripSplitButtonRunScript.Enabled = false;
            toolStripButtonStopScriptExecution.Enabled = true;

            if (addToHistory)
            {
                var title = ScripExecutionButtonTitle(false, element, model, objectVariable, args);

                ScriptExecutionToolStripMenuItem found = null;
                int historyItems = (toolStripSplitButtonRunScript.Tag != null) ? (int)toolStripSplitButtonRunScript.Tag : 0;
                if (historyItems > 0)
                {
                    for (int i = 0; i < toolStripSplitButtonRunScript.DropDownItems.Count; i++)
                    {
                        var exeItem = toolStripSplitButtonRunScript.DropDownItems[i] as ScriptExecutionToolStripMenuItem;
                        if (exeItem == null) break;     // Stop here...
                        if (exeItem.Equals(element, model, objectVariable))
                        {
                            found = exeItem;
                            toolStripSplitButtonRunScript.DropDownItems.RemoveAt(i);    // Remove it (to be inserted at the top).
                            historyItems--;
                            break;
                        }
                    }
                }
                else
                {
                    var separator = new ToolStripSeparator();
                    separator.Tag = new object();   // Add tag to indicate 'don't remove'.
                    toolStripSplitButtonRunScript.DropDownItems.Insert(0, separator);
                }

                if (found == null)
                {
                    found = new ScriptExecutionToolStripMenuItem();
                    found.FileElement = element;
                    found.Partner = model;
                    found.InstanceObject = objectVariable;
                    found.ShowFullName = false;
                    found.SetText();
                    found.Tag = new object();
                    found.Click += FileElementExecutionEntry_Click;
                }

                historyItems++;
                toolStripSplitButtonRunScript.Text = found.Text;
                toolStripSplitButtonRunScript.DropDownItems.Insert(0, found);   // Insert (or re-insert) at the top.
                if (historyItems > 25)
                {
                    toolStripSplitButtonRunScript.DropDownItems.RemoveAt(historyItems);
                    historyItems--;
                }
                toolStripSplitButtonRunScript.Tag = historyItems;
                toolStripButtonAddShortcut.Enabled = true;
            }
        }

        private void ExecutingScript_CurrentStateChanged(object sender, EventArgs e)
        {
            if (m_executingScript.State.HasEnded())
            {
                m_executingScript.CurrentStateChanged -= ExecutingScript_CurrentStateChanged;
                toolStripSplitButtonRunScript.Enabled = true;
                toolStripButtonStopScriptExecution.Enabled = false;
                m_executingScript = null;
            }
        }

        private void toolStripButtonStopScriptExecution_Click(object sender, EventArgs e)
        {
            m_executingScript.RequestStopExecution();
            toolStripButtonStopScriptExecution.Enabled = false;
        }

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
                    executionEntry = toolStripSplitButtonRunScript.DropDownItems[0] as ScriptExecutionToolStripMenuItem;
                    procButtonText = ScripExecutionButtonTitle(false, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
                    procDescription = procButtonText;
                }
                if (commandAvailable)
                {
                    commandButtonText = toolStripComboBoxToolCommand.Text;
                    commandDescription = "On " + toolStripComboBoxTool.Text + ": " + commandButtonText;
                }

                var dialog = new DialogAddShortcut("Adding Shortcut", procDescription, commandDescription, procButtonText, commandButtonText);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.ProcedureExecutionSelected)
                    {
                        this.AddProcedureShortcut(dialog.ButtonText, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject);
                    }
                    else
                    {
                        var tool = (toolStripComboBoxTool.Items[toolStripComboBoxTool.SelectedIndex] as Variable).FullName;
                        this.AddObjectCommandShortcut(dialog.ButtonText, tool, toolStripComboBoxToolCommand.Text);
                    }
                }
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
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(IntPtr hWnd);

        const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Rect pvAttribute, int cbAttribute);

        #endregion

        private void AddProcedureShortcut(string text, string element, string partner, string instanceObject)
        {
            var shortcut = new ScriptExecutionToolStripMenuItem();
            shortcut.Text = text;
            shortcut.FileElement = element;
            shortcut.Partner = partner;
            shortcut.InstanceObject = instanceObject;
            shortcut.Name = "toolStripMenuProcedure" + shortcut.Text.Replace(".", "Dot");
            shortcut.Size = new Size(182, 22);
            shortcut.Margin = new Padding(1, shortcut.Margin.Top, 1, shortcut.Margin.Bottom);
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
            shortcut.Margin = new Padding(1, shortcut.Margin.Top, 1, shortcut.Margin.Bottom);
            shortcut.BackColor = Color.Lavender;
            shortcut.ToolTipText = null; // $"Run " + target;
            shortcut.Tag = m_userShortcutItemTag;
            shortcut.Click += ObjectCommandExecutionEntry_ShortcutClick;

            toolStripMenuItemDeleteShortcut.Enabled = true;
            toolStripMenuItemDeleteAllShortcuts.Enabled = true;

            toolStripMain.Items.Add(shortcut);
        }

        private void timerMasterPull_Tick(object sender, EventArgs e)
        {
            Tuple<string, string> received;
            while (m_pipe != null && (received = m_pipe.TryGetReceived()) != null)
            {
                if (received.Item1 == nameof(ShortCommand))
                {
                    var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                    if (cmd == ShortCommand.EndFileElements)
                    {
                        this.HandleReceivedFileElements();
                    }
                    if (cmd == ShortCommand.Close)
                    {
                        m_closeRequestedByConsole = true;
                        Thread.Sleep(100);
                        m_pipe.Dispose();
                        this.Close();
                    }
                    else if (cmd == ShortCommand.ExecutionStarted)
                    {
                        //buttonRunScript.Text = "Stop";
                        //m_scriptExecuting = true;
                    }
                    else if (cmd == ShortCommand.ExecutionStopped)
                    {
                        //buttonRunScript.Text = "Run";
                        //m_scriptExecuting = false;
                    }
                }
                else if (received.Item1 == nameof(StepBro.Sidekick.Messages.StartFileElements))
                {
                    var startData = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.StartFileElements>(received.Item2);
                    m_topScriptFile = startData.TopFile;
                    m_fileElements = new List<Element>();
                }
                else if (received.Item1 == nameof(StepBro.Sidekick.Messages.FileElement))
                {
                    m_fileElements.Add(JsonSerializer.Deserialize<StepBro.Sidekick.Messages.FileElement>(received.Item2).Data);
                }
                else if (received.Item1 == nameof(ExecutionStateUpdate))
                {
                    var state = JsonSerializer.Deserialize<ExecutionStateUpdate>(received.Item2);
                    var execution = this.TryGetExecution(state.RequestID);
                    if (execution != null)
                    {
                        execution.NotifyState(state.State);
                    }
                }
            }

            if (m_shouldAttach)
            {
                MoveWindows();
            }
        }


        private void HandleReceivedFileElements()
        {
            var commandObjectVariables = m_fileElements.Where(e => e is Variable && (e as Variable).Interfaces.HasFlag(VariableInterfaces.Command)).Select(e => (Variable)e).ToList();
            var namespaces = m_fileElements.Select(e => NamespaceFromFullName(e.FullName)).Distinct().ToList();
            var objectsForProcedures = m_fileElements.Where(e => e is Procedure && (e as Procedure).CompatibleObjectInstances != null).SelectMany(e => ((e as Procedure).CompatibleObjectInstances)).Distinct().ToList();
            var allProcedures = m_fileElements.Where(e => e is Procedure).Cast<Procedure>().ToList();

            for (int di = 0; di < toolStripSplitButtonRunScript.DropDownItems.Count;)
            {
                if (toolStripSplitButtonRunScript.DropDownItems[di].Tag == null)
                {
                    toolStripSplitButtonRunScript.DropDownItems.RemoveAt(di);
                }
                else di++;
            }

            // If short name alone is used, will there be name clashes? If so, use the full name.
            bool useFullNameInUseableObject = objectsForProcedures.Select(s => s.Split('.').Last()).Distinct().Count() != objectsForProcedures.Count;

            #region Tool variables used as instance references

            foreach (var useableObject in objectsForProcedures)
            {
                string shortName = useableObject.Split('.').Last();

                var objectMenu = new ToolStripMenuItem();
                objectMenu.Name = "toolStripMenuObject" + useableObject.Replace(".", "Dot");
                objectMenu.Size = new Size(182, 22);
                objectMenu.Text = useFullNameInUseableObject ? useableObject : shortName;
                objectMenu.ToolTipText = null;
                toolStripSplitButtonRunScript.DropDownItems.Add(objectMenu);

                var procedures = allProcedures.Where(p => p.CompatibleObjectInstances != null && p.CompatibleObjectInstances.Any(s => string.Equals(s, useableObject, StringComparison.InvariantCulture))).ToList();
                foreach (var procedure in procedures)
                {
                    string callText = (useFullNameInUseableObject ? useableObject : shortName) + "." + procedure.Name;
                    var procedureMenu = new ScriptExecutionToolStripMenuItem();
                    procedureMenu.Name = "toolStripMenuProcedure" + procedure.Name + "On" + useableObject.Replace(".", "Dot");
                    procedureMenu.Size = new Size(182, 22);
                    procedureMenu.Text = procedure.Name;
                    procedureMenu.ToolTipText = null;

                    procedureMenu.FileElement = procedure.FullName;
                    procedureMenu.InstanceObject = useableObject;
                    procedureMenu.Click += FileElementExecutionEntry_Click;

                    if (procedure.Parameters.Length > 1) procedureMenu.Enabled = false;     // TODO: Enable user to input the arguments.

                    objectMenu.DropDownItems.Add(procedureMenu);
                }
            }

            #endregion

            #region Namespace sections

            foreach (var ns in namespaces)
            {
                var namespaceMenu = new ToolStripMenuItem();

                var procedures = allProcedures.Where(p => NamespaceFromFullName(p.FullName) == ns).ToList();
                procedures.Sort(delegate (Procedure x, Procedure y)
                {
                    if (x.FullName == null && y.FullName == null) return 0;
                    else if (x.FullName == null) return -1;
                    else if (y.FullName == null) return 1;
                    else return x.FullName.CompareTo(y.FullName);
                });

                if (procedures.Count > 0)
                {
                    toolStripSplitButtonRunScript.Enabled = true;
                }
                foreach (var procedure in procedures)
                {
                    if ((procedure.Partners != null && procedure.Partners.Length > 0) ||
                        (procedure.CompatibleObjectInstances != null && procedure.CompatibleObjectInstances.Length > 0))
                    {
                        ToolStripMenuItem procedureMenu = new ToolStripMenuItem();
                        procedureMenu.Name = "toolStripMenuProcedure" + procedure.Name;
                        procedureMenu.Size = new Size(182, 22);
                        procedureMenu.Text = procedure.Name;
                        procedureMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}'";
                        namespaceMenu.DropDownItems.Add(procedureMenu);

                        if (procedure.Partners != null && procedure.Partners.Length > 0)
                        {
                            var options = new List<Partner>(procedure.Partners);
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
                                    var partnerProcedure = allProcedures.FirstOrDefault(p => p.FullName == partner.ProcedureReference);
                                    if (partnerProcedure == null ||
                                        (partnerProcedure.Parameters != null && partnerProcedure.Parameters.Length > ((partnerProcedure.FirstParameterIsInstanceReference) ? 1 : 0)))   // TODO: Check whether that first parameter is the parent procedure.
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
                                procedureMenu.DropDownItems.Add(procedureExecutionOptionMenu);
                            }
                        }
                        else if (procedure.CompatibleObjectInstances != null && procedure.CompatibleObjectInstances.Length > 0)
                        {
                            foreach (var variable in procedure.CompatibleObjectInstances)
                            {
                                string shortName = variable.Split('.').Last();

                                var procedureExecutionOptionMenu = new ScriptExecutionToolStripMenuItem(procedure.FullName, null, variable);
                                procedureExecutionOptionMenu.Size = new Size(182, 22);
                                procedureExecutionOptionMenu.Name = "toolStripMenuProcedure" + procedure.Name + "On" + variable.Replace(".", "Dot");
                                procedureExecutionOptionMenu.SetText();
                                procedureExecutionOptionMenu.ToolTipText = null; // $"Procedure '{procedure.FullName}' partner '{partner.Name}'";
                                procedureExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                                if (procedure.Parameters == null || procedure.Parameters.Length > 1)     // TODO: Enable user to input the arguments.
                                {
                                    procedureExecutionOptionMenu.Enabled = false;
                                }
                                procedureMenu.DropDownItems.Add(procedureExecutionOptionMenu);
                            }
                        }
                        else
                        {
                            var executionItem = procedureMenu as ScriptExecutionToolStripMenuItem;
                            executionItem.FileElement = procedure.FullName;
                            executionItem.SetText();
                            if (procedure.Parameters != null && procedure.Parameters.Length > 0)     // TODO: Enable user to input the arguments.
                            {
                                executionItem.Enabled = false;
                            }
                            procedureMenu.Click += FileElementExecutionEntry_Click;
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
                        namespaceMenu.DropDownItems.Add(procedureMenu);
                    }
                }

                #region TestLists

                var tests = m_fileElements.Where(e => NamespaceFromFullName(e.FullName) == ns && e is TestList).Cast<TestList>().ToList();
                tests.Sort(delegate (TestList x, TestList y)
                {
                    if (x.FullName == null && y.FullName == null) return 0;
                    else if (x.FullName == null) return -1;
                    else if (y.FullName == null) return 1;
                    else return x.FullName.CompareTo(y.FullName);
                });
                foreach (var testlist in tests)
                {
                    var testlistMenu = new ToolStripMenuItem();
                    testlistMenu.Name = "toolStripMenuTestList" + testlist.Name;
                    testlistMenu.Size = new Size(182, 22);
                    testlistMenu.Text = testlist.Name;
                    testlistMenu.ToolTipText = null; // $"The testlist '{testlist.FullName}'";
                    namespaceMenu.DropDownItems.Add(testlistMenu);

                    foreach (var partner in testlist.Partners)
                    {
                        var testlistExecutionOptionMenu = new ScriptExecutionToolStripMenuItem();
                        testlistExecutionOptionMenu.FileElement = testlist.FullName;
                        testlistExecutionOptionMenu.Partner = partner.Name;
                        testlistExecutionOptionMenu.Size = new Size(182, 22);
                        testlistExecutionOptionMenu.Name = "toolStripMenuTestlist" + testlist.Name + "Dot" + partner.Name;
                        testlistExecutionOptionMenu.SetText();
                        testlistExecutionOptionMenu.ToolTipText = null; // $"Test '{testlist.FullName}' model '{partner.Name}'";
                        testlistExecutionOptionMenu.BackColor = Color.Thistle;
                        testlistExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                        testlistMenu.DropDownItems.Add(testlistExecutionOptionMenu);
                    }
                }

                #endregion

                if (namespaceMenu.DropDownItems.Count > 0)
                {
                    namespaceMenu.Name = "toolStripMenuNamespace" + ns;
                    namespaceMenu.Size = new Size(182, 22);
                    namespaceMenu.Text = "Namespace " + ns;
                    namespaceMenu.ToolTipText = null; // $"All procedures and testlists in the namespace '{ns}'";
                    toolStripSplitButtonRunScript.DropDownItems.Add(namespaceMenu);
                }

            }

            #endregion

            #region Panel tool variables

            var panelVariables = m_fileElements.Where(e => e is PanelDefinitionVariable).Cast<PanelDefinitionVariable>().ToList();

            if (panelVariables.Count > 0)
            {
                if (m_panelsDialog == null)
                {
                    m_panelsDialog = new PanelsDialog((ICoreAccess)this);
                    m_panelsDialog.FormClosed += PanelsDialog_FormClosed;
                    m_panelsDialog.Show();
                }
                foreach (var panel in panelVariables)
                {
                    m_panelsDialog.AddCustomPanel(
                        panel.Title,
                        (PropertyBlock)panel.PanelDefinition.CloneAsPropertyBlockEntry());
                }
            }

            #endregion

            #region ToolBar tool variables

            var toolbarVariables = m_fileElements.Where(e => e is ToolBarDefinitionVariable).Cast<ToolBarDefinitionVariable>().ToList();

            var newToolBarList = new List<Tuple<string, StepBro.UI.WinForms.CustomToolBar.ToolBar>>();
            if (toolbarVariables.Count > 0)
            {
                foreach (var toolbarVar in toolbarVariables)
                {
                    StepBro.UI.WinForms.CustomToolBar.ToolBar toolBar = null;
                    var existing = m_customToolStrips.Where(t => t.Item1 == toolbarVar.FullName).FirstOrDefault();
                    if (existing != null)
                    {
                        toolBar = existing.Item2;
                    }
                    else
                    {
                        toolBar = new StepBro.UI.WinForms.CustomToolBar.ToolBar(this);
                    }
                    newToolBarList.Add(new Tuple<string, UI.WinForms.CustomToolBar.ToolBar>(toolbarVar.FullName, toolBar));

                    toolBar.Setup(this, toolbarVar.FullName, toolbarVar.ToolBarDefinition.CloneAsPropertyBlockEntry() as PropertyBlock);
                    toolBar.Height = toolStripMain.Height + 1;
                }

                //if (m_panelsDialog == null)
                //{
                //    m_panelsDialog = new PanelsDialog((ICoreAccess)this);
                //    m_panelsDialog.FormClosed += PanelsDialog_FormClosed;
                //    m_panelsDialog.Show();
                //}
                //foreach (var panel in panelVariables)
                //{
                //    m_panelsDialog.AddCustomPanel(
                //        panel.Title,
                //        (PropertyBlock)panel.PanelDefinition.CloneAsPropertyBlockEntry());
                //}
            }
            //var customToolBars = this.Controls.Cast<Control>().Where(c => c is ToolBar).ToList();
            //foreach (var tb in customToolBars)
            //{
            //    this.Controls.Remove(tb);
            //}

            this.Controls.Clear();
            toolStripMenuItemShownToolbars.DropDownItems.Clear();
            m_customToolStrips = newToolBarList.ToList();
            m_customToolStrips.Sort((l, r) => (1000000 - r.Item2.Index).CompareTo(1000000 - l.Item2.Index));    // Toolbars with same index in reading order from script, otherwise use after index order.
            m_customToolStrips.Reverse();
            int tabIndex = m_customToolStrips.Count;
            foreach (var tbData in m_customToolStrips)
            {
                var visibilityMenuItem = new ToolStripMenuItem();
                visibilityMenuItem.CheckOnClick = true;
                visibilityMenuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
                visibilityMenuItem.Size = new Size(180, 22);
                visibilityMenuItem.Text = tbData.Item1;
                visibilityMenuItem.Checked = !m_hiddenToolbars.Contains(tbData.Item1);
                visibilityMenuItem.CheckedChanged += (s, e) => { SetToolbarVisibility(((ToolStripMenuItem)s).Text, ((ToolStripMenuItem)s).Checked); };
                visibilityMenuItem.Tag = tbData.Item2;
                toolStripMenuItemShownToolbars.DropDownItems.Insert(0, visibilityMenuItem);

                if (tabIndex % 2 == 0)
                {
                    tbData.Item2.DefaultBackColor = Color.Beige;
                }
                tbData.Item2.TabIndex = tabIndex--;
                this.Controls.Add(tbData.Item2);
            }
            this.Controls.Add(toolStripMain);

            if (m_customToolStrips.Count > 0)
            {
                m_customToolStrips[0].Item2.AdjustSizesAndColumns();
            }
            else
            {
                this.Height = toolStripMain.Bounds.Bottom + 2;
            }

            #endregion

            #region Loading persisted shortcuts

            if (!m_userFileRead)
            {
                // Change the extension
                m_userFile = Path.Combine(Path.GetDirectoryName(m_topScriptFile), Path.GetFileNameWithoutExtension(m_topScriptFile)) + ".user.json";
                m_userFileRead = true;

                if (File.Exists(m_userFile))
                {
                    var data = JsonSerializer.Deserialize<UserDataCurrent>(File.ReadAllText(m_userFile));
                    if (data.Shortcuts != null)
                    {
                        foreach (var shortcut in data.Shortcuts)
                        {
                            if (shortcut is UserDataCurrent.ProcedureShortcut)
                            {
                                var typed = shortcut as UserDataCurrent.ProcedureShortcut;
                                this.AddProcedureShortcut(typed.Text, typed.Element, typed.Partner, typed.Instance);
                            }
                            else if (shortcut is UserDataCurrent.ObjectCommandShortcut)
                            {
                                var typed = shortcut as UserDataCurrent.ObjectCommandShortcut;
                                this.AddObjectCommandShortcut(typed.Text, typed.Instance, typed.Command);
                            }
                        }
                    }
                    if (data.HiddenToolbars != null && data.HiddenToolbars.Length > 0)
                    {
                        m_hiddenToolbars = data.HiddenToolbars.ToList();
                    }
                }
            }

            #endregion

            this.UpdateToolbarVisibility();
        }

        private void SetToolbarVisibility(string name, bool visible)
        {

            int i = -1;
            for (int j = 0; j < m_hiddenToolbars.Count; j++)
            {
                if (m_hiddenToolbars[j] == name)
                {
                    i = j;
                    break;
                }
            }
            if (visible)
            {
                if (i >= 0)
                {
                    m_hiddenToolbars.RemoveAt(i);
                }
            }
            else
            {
                if (i < 0)
                {
                    m_hiddenToolbars.Add(name);
                }
            }
            this.UpdateToolbarVisibility();
        }

        private void UpdateToolbarVisibility()
        {
            int missingHeight = toolStripMain.Bounds.Bottom - this.ClientRectangle.Height;

            // We can not use Controls[0] here, as that could give an invisible toolbar
            // so we search for the bottom-most visible toolbar
            StepBro.UI.WinForms.CustomToolBar.ToolBar bottomVisible = null; // Actually the first visible in the list, as toolbars are added in reverse order.
            if (m_customToolStrips.Count > 0)
            {
                foreach (var tb in m_customToolStrips)
                {
                    bool visible = !m_hiddenToolbars.Contains(tb.Item1);
                    tb.Item2.Visible = visible;
                    if (bottomVisible == null && visible)
                    {
                        bottomVisible = tb.Item2;
                    }
                }
                if (bottomVisible != null)
                {
                    missingHeight = bottomVisible.Bounds.Bottom - this.ClientRectangle.Height;
                }
            }

            this.Height += missingHeight;
        }

        public static string ScripExecutionButtonTitle(bool showFullName, string element, string partner, string objectVariable, object[] args)
        {
            var elementName = (showFullName && String.IsNullOrEmpty(objectVariable)) ? element : element.Split('.').Last();
            if (!String.IsNullOrEmpty(objectVariable))
            {
                if (showFullName)
                {
                    return objectVariable + "." + elementName;
                }
                else
                {
                    return objectVariable.Split('.').Last() + "." + elementName;
                }
            }
            else if (!String.IsNullOrEmpty(partner))
            {
                return elementName + "." + partner;
            }
            else
            {
                return elementName;
            }
        }

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

        private void PanelsDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_panelsDialog.FormClosed -= PanelsDialog_FormClosed;
            m_panelsDialog = null;
        }

        private ExecutionAccess TryGetExecution(ulong id)
        {
            var i = 0;
            while (m_activeExecutions.Count > i)
            {
                ExecutionAccess target;
                if (m_activeExecutions[i].TryGetTarget(out target))
                {
                    if (target.ID == id)
                    {
                        return target;
                    }
                    i++;
                }
                else
                {
                    m_activeExecutions.RemoveAt(i);     // Target disposed; remove from the list.
                }
            }
            return null;
        }

        #region ICoreAccess

        public bool ExecutionRunning => throw new NotImplementedException();

        IExecutionAccess ICoreAccess.StartExecution(string element, string model, string objectVariable, object[] args)
        {
            var execution = new ExecutionAccess(this, m_pipe);
            m_activeExecutions.Add(new WeakReference<ExecutionAccess>(execution));
            List<TypedValue> arguments = null;
            if (args != null)
            {
                arguments = args.Select(a => new TypedValue(a)).ToList();
            }
            var request = new RunScriptRequest(execution.ID, false, element, model, objectVariable, arguments);
            if (toolStripTextBoxExeNote.Visible)
            {
                request.ExecutionNote = toolStripTextBoxExeNote.Text;
            }
            m_pipe.Send(request);
            return execution;
        }

        void ICoreAccess.ExecuteObjectCommand(string objectVariable, string command)
        {
            ExecuteCommand(objectVariable, command);
        }

        #endregion

        private class ExecutionAccess : IExecutionAccess, IDisposing
        {
            private MainForm m_parent;
            private Pipe m_pipe;
            private TaskExecutionState m_state = TaskExecutionState.StartRequested;
            private bool m_active = true;

            public ExecutionAccess(MainForm parent, Pipe pipe)
            {
                m_parent = parent;
                m_pipe = pipe;
                this.ID = UniqueInteger.GetLongProtected();
            }

            ~ExecutionAccess()
            {
                this.Dispose();
            }

            public void Dispose()
            {
                if (m_active)
                {
                    m_active = false;
                    if (this.Disposing != null) this.Disposing(this, EventArgs.Empty);
                    m_pipe.Send(new ReleaseRequest(this.ID));
                }
            }

            public ulong ID { get; private set; }

            public event EventHandler Disposing;

            public void NotifyState(TaskExecutionState state)
            {
                if (state != this.State)
                {
                    this.State = state;
                    if (CurrentStateChanged != null) this.CurrentStateChanged(this, EventArgs.Empty);
                }
            }

            #region IExecutionAccess

            public TaskExecutionState State
            {
                get { return m_state; }
                private set
                {
                    m_state = value;
                }
            }

            public object ReturnValue { get; set; }

            public event EventHandler CurrentStateChanged;

            public void RequestStopExecution()
            {
                if (!this.State.HasEnded())
                {
                    m_pipe.Send(new StopExecutionRequest(this.ID));
                }
            }

            #endregion
        }

        private void toolStripMenuItemClearDisplay_Click(object sender, EventArgs e)
        {
            m_pipe.Send(ShortCommand.ClearDisplay);
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripMenuItemDeleteAllShortcuts_Click(object sender, EventArgs e)
        {
        }

        private void toolStripMenuItemExeNoteInput_CheckedChanged(object sender, EventArgs e)
        {
            toolStripTextBoxExeNote.Visible = toolStripMenuItemExeNoteInput.Checked;
            SetExtraFieldsSeparatorVisibility();
        }

        private void SetExtraFieldsSeparatorVisibility()
        {
            toolStripSeparatorExtraFields.Visible = toolStripTextBoxExeNote.Visible;
        }

        private void toolStripMenuItemShownToolbars_DropDownOpening(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in toolStripMenuItemShownToolbars.DropDownItems)
            {
                item.Checked = (item.Tag as StepBro.UI.WinForms.CustomToolBar.ToolBar).Visible;
            }
        }

        private void toolStripMenuItemView_DropDownOpening(object sender, EventArgs e)
        {
            toolStripMenuItemShownToolbars.Enabled = (m_customToolStrips.Count > 0);
        }

        #region ILogger

        bool ILogger.IsDebugging { get { return false; } }

        string ILogger.Location { get { return "Sidekick"; } }

        ILoggerScope ILogger.LogEntering(string location, string text)
        {
            throw new NotImplementedException();
        }

        ILoggerScope ILogger.CreateSubLocation(string name)
        {
            throw new NotImplementedException();
        }

        ITimestampedData ILogger.Log(string text)
        {
            var log = new Log() { LogType = Log.Type.Normal, Text = text };
            m_pipe.Send(log);
            System.Diagnostics.Debug.WriteLine("ILogger.LogError: " + text);
            return null;
        }

        void ILogger.LogDetail(string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogAsync(string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogError(string text)
        {
            var log = new Log() { LogType = Log.Type.Error, Text = text };
            m_pipe.Send(log);
            System.Diagnostics.Debug.WriteLine("ILogger.LogError: " + text);
        }

        void ILogger.LogUserAction(string text)
        {
            var log = new Log() { LogType = Log.Type.Normal, Text = text };
            m_pipe.Send(log);
            System.Diagnostics.Debug.WriteLine("ILogger.LogError: " + text);
        }

        void ILogger.LogCommSent(string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogCommReceived(string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogSystem(string text)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
