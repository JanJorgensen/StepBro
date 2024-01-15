using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.Sidekick;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using static StepBro.ConsoleSidekick.WinForms.MainForm;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class MainForm : Form, ICoreAccess
    {
        private Control m_topControl = null;
        private nint m_consoleWindow = 0;
        private bool m_isConsoleActive = false;
        private bool m_forceResize = false;
        private bool m_moveToTop = true;
        private bool m_closeRequestedByConsole = false;
        private SideKickPipe m_pipe = null;
        private Rect m_lastConsolePosition = new Rect();
        private IExecutionAccess m_executingScript = null;
        private PanelsDialog m_panelsDialog = null;
        private bool m_settingCommandCombo = false;
        List<WeakReference<ExecutionAccess>> m_activeExecutions = new List<WeakReference<ExecutionAccess>>();

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
                if (!String.IsNullOrEmpty(InstanceObject))
                {
                    if (this.ShowFullName)
                    {
                        this.Text = InstanceObject + "." + this.FileElement.Split('.').Last();
                    }
                    else
                    {
                        this.Text = InstanceObject.Split('.').Last() + "." + this.FileElement.Split('.').Last();
                    }
                }
                else if (!String.IsNullOrEmpty(Partner))
                {
                    if (this.ShowFullName)
                    {
                        this.Text = this.FileElement + "." + this.Partner;
                    }
                    else
                    {
                        this.Text = this.FileElement.Split('.').Last() + "." + this.Partner;
                    }
                }
                else
                {
                    if (this.ShowFullName)
                    {
                        this.Text = this.FileElement;
                    }
                    else
                    {
                        this.Text = this.FileElement.Split('.').Last();
                    }
                }
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

        public MainForm()
        {
            InitializeComponent();
            toolStripButtonRunCommand.Text = "\u23F5";
            toolStripButtonStopScriptExecution.Text = "\u23F9";
            toolStripButtonAddShortcut.Text = "\u2795";
            toolStripDropDownButtonMainMenu.Text = "\u2630";
        }

        // TODO: https://stackoverflow.com/questions/1732140/displaying-tooltip-over-a-disabled-control

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_topControl = this.TopLevelControl;

            System.Diagnostics.Trace.WriteLine("Sidekick STARTING!!");

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 2)
            {
                m_consoleWindow = nint.Parse(args[1], System.Globalization.NumberStyles.HexNumber);
                m_pipe = SideKickPipe.StartClient(args[1]);
            }
            else
            {
                return;
            }
            m_forceResize = true;
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
                        m_topControl.Height = toolStripMain.Height;
                    }
                }
            }
            m_forceResize = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (m_closeRequestedByConsole)
            {
                System.Diagnostics.Trace.WriteLine("Sidekick base.OnFormClosing() - as requested");
                //if (m_pipe != null)
                //{
                //    m_pipe.Send(ShortCommand.Close);
                //    m_pipe.Dispose();
                //    m_pipe = null;
                //}
                base.OnFormClosing(e);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Sidekick base.OnFormClosing() - requesting console app");
                m_pipe.Send(ShortCommand.RequestClose);
                e.Cancel = true;    // Don't close; wait for close request from console.
            }
            System.Diagnostics.Trace.WriteLine("Sidekick closing end");
        }

        #region USER INTERACTION - COMMANDS

        private void toolStripComboBoxTool_SelectedIndexChanged(object sender, EventArgs e)
        {
            toolStripComboBoxTool.ToolTipText =
                "Select tool/object to use for the command prompt. Selected object: '" +
                (toolStripComboBoxTool.SelectedItem as FileElements.Variable).FullName + "'";
        }

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
            var tool = (toolStripComboBoxTool.Items[toolStripComboBoxTool.SelectedIndex] as FileElements.Variable).FullName;
            m_pipe.Send(new ObjectCommand(tool, command));
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
            MenuFileElementExecutionStart(false, executionEntry.FileElement, executionEntry.Partner, executionEntry.InstanceObject, null);
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
                var title = ScripExecutionButtonTitle(element, model, objectVariable, args);

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
            if (toolStripSplitButtonRunScript.Tag != null)
            {
                var executionEntry = toolStripSplitButtonRunScript.DropDownItems[0] as ScriptExecutionToolStripMenuItem;
                var shortcut = new ScriptExecutionToolStripMenuItem();
                shortcut.FileElement = executionEntry.FileElement;
                shortcut.Partner = executionEntry.Partner;
                shortcut.InstanceObject = executionEntry.InstanceObject;
                shortcut.SetText();
                shortcut.Name = "toolStripMenuProcedure" + shortcut.Text.Replace(".", "Dot");
                shortcut.Size = new Size(182, 22);
                shortcut.ToolTipText = null; // $"Run " + target;
                shortcut.Click += FileElementExecutionEntry_ShortcutClick;

                var dialog = new DialogNameInput("Adding Shortcut", "Enter the name to show on the shortcut button.", shortcut.Text);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    toolStripMain.Items.Add(shortcut);
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

        private void timerMasterPull_Tick(object sender, EventArgs e)
        {
            Tuple<string, string> received;
            while (m_pipe != null && (received = m_pipe.TryGetReceived()) != null)
            {
                if (received.Item1 == nameof(ShortCommand))
                {
                    var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                    if (cmd == ShortCommand.Close)
                    {
                        m_closeRequestedByConsole = true;
                        m_pipe.Send(cmd);   // Send back, to make console continue the closing process.
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
                else if (received.Item1 == nameof(StepBro.Sidekick.FileElements))
                {
                    var elements = JsonSerializer.Deserialize<StepBro.Sidekick.FileElements>(received.Item2);
                    var commandObjectVariables = elements.Elements.Where(e => e is StepBro.Sidekick.FileElements.Variable && (e as StepBro.Sidekick.FileElements.Variable).Interfaces.HasFlag(FileElements.VariableInterfaces.Command)).Select(e => (StepBro.Sidekick.FileElements.Variable)e).ToList();
                    var namespaces = elements.Elements.Select(e => NamespaceFromFullName(e.FullName)).Distinct().ToList();
                    var objectsForProcedures = elements.Elements.Where(e => e is FileElements.Procedure && (e as FileElements.Procedure).CompatibleObjectInstances != null).SelectMany(e => ((e as FileElements.Procedure).CompatibleObjectInstances)).Distinct().ToList();

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

                    foreach (var useableObject in objectsForProcedures)
                    {
                        string shortName = useableObject.Split('.').Last();

                        var objectMenu = new ToolStripMenuItem();
                        objectMenu.Name = "toolStripMenuObject" + useableObject.Replace(".", "Dot");
                        objectMenu.Size = new Size(182, 22);
                        objectMenu.Text = useFullNameInUseableObject ? useableObject : shortName;
                        objectMenu.ToolTipText = null;
                        toolStripSplitButtonRunScript.DropDownItems.Add(objectMenu);

                        var procedures = elements.Elements.Where(e => e is FileElements.Procedure && (e as FileElements.Procedure).CompatibleObjectInstances != null && (e as FileElements.Procedure).CompatibleObjectInstances.Any(s => string.Equals(s, useableObject, StringComparison.InvariantCulture))).Cast<FileElements.Procedure>().ToList();
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

                    foreach (var ns in namespaces)
                    {
                        var namespaceMenu = new ToolStripMenuItem();

                        var procedures = elements.Elements.Where(e => NamespaceFromFullName(e.FullName) == ns && e is FileElements.Procedure).Cast<FileElements.Procedure>().ToList();
                        procedures.Sort(delegate (FileElements.Procedure x, FileElements.Procedure y)
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
                                    var options = new List<FileElements.Partner>(procedure.Partners);
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
                                            var partnerProcedure = procedures.FirstOrDefault(p => p.FullName == partner.ProcedureType);
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

                        var tests = elements.Elements.Where(e => NamespaceFromFullName(e.FullName) == ns && e is FileElements.TestList).Cast<FileElements.TestList>().ToList();
                        tests.Sort(delegate (FileElements.TestList x, FileElements.TestList y)
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
                                testlistExecutionOptionMenu.BackColor = Color.Purple;
                                testlistExecutionOptionMenu.Click += FileElementExecutionEntry_Click;
                                testlistMenu.DropDownItems.Add(testlistExecutionOptionMenu);
                            }
                        }

                        if (namespaceMenu.DropDownItems.Count > 0)
                        {
                            namespaceMenu.Name = "toolStripMenuNamespace" + ns;
                            namespaceMenu.Size = new Size(182, 22);
                            namespaceMenu.Text = "Namespace " + ns;
                            namespaceMenu.ToolTipText = null; // $"All procedures and testlists in the namespace '{ns}'";
                            toolStripSplitButtonRunScript.DropDownItems.Add(namespaceMenu);
                        }

                    }

                    var selectedTool = toolStripComboBoxTool.SelectedItem as FileElements.Variable;
                    toolStripComboBoxTool.Items.Clear();
                    int selection = 0;
                    int i = 0;
                    foreach (var toolVar in commandObjectVariables)
                    {
                        toolStripComboBoxTool.Items.Add(toolVar);
                        if (selectedTool != null && toolVar.FullName == selectedTool.FullName)
                        {
                            selection = i;
                        }
                        i++;
                    }
                    if (toolStripComboBoxTool.Items.Count > 0)
                    {
                        toolStripComboBoxTool.Enabled = true;
                        toolStripComboBoxToolCommand.Enabled = true;
                        toolStripComboBoxTool.SelectedIndex = selection;
                    }
                    else
                    {
                        toolStripComboBoxTool.Enabled = false;
                        toolStripComboBoxToolCommand.Enabled = false;
                    }
                    toolStripComboBoxTool.SelectionLength = 0;

                    var panelVariables = elements.Elements.Where(e => e is StepBro.Sidekick.FileElements.PanelDefinitionVariable).Select(e => (StepBro.Sidekick.FileElements.PanelDefinitionVariable)e).ToList();

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
                }
                else if (received.Item1 == nameof(StepBro.Sidekick.ExecutionStateUpdate))
                {
                    var state = JsonSerializer.Deserialize<StepBro.Sidekick.ExecutionStateUpdate>(received.Item2);
                    var execution = this.TryGetExecution(state.RequestID);
                    if (execution != null)
                    {
                        execution.NotifyState(state.State);
                    }
                }
            }

            MoveWindows();
        }

        string ScripExecutionButtonTitle(string element, string model, string objectVariable, object[] args)
        {
            if (String.IsNullOrEmpty(model))
            {
                return element;
            }
            else
            {
                return element + "." + model;
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

        public int ExecutionsRunning => throw new NotImplementedException();

        IExecutionAccess ICoreAccess.StartExecution(string element, string model, string objectVariable, object[] args)
        {
            var execution = new ExecutionAccess(this, m_pipe);
            m_activeExecutions.Add(new WeakReference<ExecutionAccess>(execution));
            m_pipe.Send(new RunScriptRequest(execution.ID, false, element, model, objectVariable));
            return execution;
        }

        #endregion

        private class ExecutionAccess : IExecutionAccess, IDisposing
        {
            private MainForm m_parent;
            private SideKickPipe m_pipe;
            private bool m_active = true;

            public ExecutionAccess(MainForm parent, SideKickPipe pipe)
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

            public TaskExecutionState State { get; set; } = TaskExecutionState.StartRequested;

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

        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
