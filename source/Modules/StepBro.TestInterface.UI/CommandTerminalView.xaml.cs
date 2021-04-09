using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using StepBro.Core.Tasks;
using StepBro.TestInterface.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace StepBro.TestInterface.Controls
{
    /// <summary>
    /// Interaction logic for CommandTerminal.xaml
    /// </summary>
    public partial class CommandTerminalView : UserControl, IEditorViewKeyInputEventSink
    {
        private IAsyncResult<object> m_activeCommand = null;
        DispatcherTimer m_commandTimer;

        public CommandTerminalView()
        {
            this.InitializeComponent();

            commandTextBox.Document.Language = SyntaxLanguage.PlainText;
            // Register a key input event sink to be able to act on Enter.
            commandTextBox.Document.Language.RegisterService<IEditorViewKeyInputEventSink>(this);

            m_commandTimer = new DispatcherTimer();
            m_commandTimer.Interval = TimeSpan.FromMilliseconds(100);
            m_commandTimer.Tick += CommandTimer_Tick;

            //if (this.DataContext != null)
            //{
            //    this.Connection.PropertyChanged += Connection_PropertyChanged;
            //    this.AddCommandsFromObject();
            //}
            this.UpdateUI();
        }

        private CommandTerminalViewModel ViewModel
        {
            get { return this.DataContext as CommandTerminalViewModel; }
        }
        private SerialTestConnection Connection
        {
            get { return (this.DataContext as CommandTerminalViewModel)?.Connection; }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(DataContext))
            {
                if (this.DataContext != null)
                {
                    this.ViewModel.PropertyChanged += VM_PropertyChanged;
                }
            }
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CommandTerminalViewModel.Connection))
            {
                if (this.ViewModel.Connection != null)
                {
                    this.Connection.PropertyChanged += Connection_PropertyChanged;
                    this.AddCommandsFromObject();
                    this.UpdateUI();
                }
                else
                {
                    this.Connection.PropertyChanged -= Connection_PropertyChanged;
                }
            }
        }

        private void UpdateUI()
        {
            bool allowSend = (m_activeCommand == null);
            sendButton.IsEnabled = (allowSend && !String.IsNullOrEmpty(commandTextBox.Text));
            commandHistory.IsEnabled = (commandHistory.Items.Count > 0);
            //buttonSendPrevious.Enabled = (allowSend && comboBoxPreviousCommands.Items.Count > 0 && comboBoxPreviousCommands.SelectedIndex >= 0);
            toolBar.IsEnabled = allowSend;
            //foreach (ToolStripItem tbb in toolStripQuickCommands.Items) tbb.Enabled = allowSend;
        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Enter)
        //    {
        //        if (textBoxCommand.Focused)
        //        {
        //            if (m_connection != null && m_activeCommand == null && !String.IsNullOrEmpty(textBoxCommand.Text))
        //            {
        //                this.SendCommand(textBoxCommand.Text);
        //            }
        //            return true;
        //        }
        //        else if (comboBoxPreviousCommands.Focused && !comboBoxPreviousCommands.DroppedDown)
        //        {
        //            return true;
        //        }
        //        else if (textBoxCommandButtonText.Focused)
        //        {
        //            this.AddCommandButton(textBoxCommandButtonText.Text, textBoxCommand.Text);
        //            panelCommandButtonInput.Visible = false;
        //            return true;
        //        }
        //    }
        //    else if (keyData == Keys.Up && textBoxCommand.Focused)
        //    {

        //    }
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private void Connection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SerialTestConnection.UICommands))
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    this.AddCommandsFromObject();
                });
            }
        }

        private void AddCommandsFromObject()
        {
            var connection = this.Connection;
            if (connection.UICommands != null && toolBar.Items.Count <= 1)   // Note: There's an "Add" button there.
            {
                foreach (var c in connection.UICommands)
                {
                    var baseMenu = toolBarMenu as ItemsControl;
                    var titleparts = c.Item1.Split('|');
                    for (int i = 0; i < (titleparts.Length - 1); i++)
                    {
                        MenuItem sub = null;
                        foreach (MenuItem mi in baseMenu.Items)
                        {
                            if (mi.Header is string && titleparts[i].Equals(mi.Header as string))
                            {
                                sub = mi;
                                break;
                            }
                        }
                        if (sub == null)
                        {
                            sub = new MenuItem();
                            sub.Header = titleparts[i];
                            baseMenu.Items.Add(sub);
                        }
                        baseMenu = sub;
                    }
                    var entry = new MenuItem();
                    entry.Header = titleparts[titleparts.Length - 1];
                    entry.Tag = c.Item2;
                    entry.Click += ConnectionCommandEntry_Click;
                    baseMenu.Items.Add(entry);
                }
            }
            this.UpdateUI();
        }

        private void ConnectionCommandEntry_Click(object sender, RoutedEventArgs e)
        {
            var menuEntry = e.Source as MenuItem;
            var command = menuEntry.Tag as string;
            this.SendCommand(command);
        }

        //protected override void DisconnectBinding()
        //{
        //    simpleLogViewFull.SetSource(null);
        //    m_connection = null;
        //}

        public void AddCommandButton(string name, string command)
        {
            //var button = new ToolStripButton();
            //button.Alignment = ToolStripItemAlignment.Left;
            //button.DisplayStyle = ToolStripItemDisplayStyle.Text;
            //button.Name = "toolStripButtonCommand_" + name.Replace(' ', '_');
            //button.Size = new Size(33, 22);
            //button.Text = name;
            //button.Click += new EventHandler(this.toolStripButtonCommand_Click);
            //button.Tag = command;
            //toolStripQuickCommands.Items.Insert(toolStripQuickCommands.Items.Count - 1, button);
            //checkBoxShowCommandButtons.Checked = true;  // Ensure visible
            this.UpdateUI();
        }

        //private void toolStripButtonCommand_Click(object sender, EventArgs e)
        //{
        //    var commandButton = sender as ToolStripButton;
        //    var command = commandButton.Tag as string;
        //    textBoxCommand.Text = command;
        //    textBoxCommand.SelectAll();
        //    textBoxCommand.Focus();
        //    this.SendCommand(command);
        //}

        private void SendPromptCommand()
        {
            var command = commandTextBox.Text.Trim();
            if (!String.IsNullOrEmpty(command))
            {
                this.SendCommand(command);
            }
        }

        private void sendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SendPromptCommand();
        }

        private void SendCommand(string command)
        {
            bool doCommand = true;
            var connection = this.Connection;
            if (connection == null) return;

            if (!connection.IsConnected())
            {
                if (connection.Stream != null)
                {
                    doCommand = connection.Connect(Core.Main.CreateUICallContext());

                    //if (MessageBox.Show(Application.Current.MainWindow,
                    //    "Do you wish to open the port/connaction?", 
                    //    "Serial Test Interface is not connacted", 
                    //    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    //{
                    //    doCommand = m_connection.Connect(Core.Main.CreateUICallContext());
                    //}
                    //else
                    //{
                    //    doCommand = false;
                    //}
                }
            }
            if (doCommand)
            {
                m_activeCommand = connection.SendCommand(null, command);
                //this.AddCommandToList(command);
                //commandTextBox.Document.SelectAll();
                commandTextBox.Focus();
                sendButton.IsEnabled = false;
                m_commandTimer.Start(); ;
            }
            //simpleLogViewFull.FollowEnd();
            this.UpdateUI();
        }

        private void CommandTimer_Tick(object sender, EventArgs e)
        {
            if (m_activeCommand != null && m_activeCommand.IsCompleted)
            {
                m_commandTimer.Stop();
                m_activeCommand = null;
                this.UpdateUI();
            }
        }

        public void NotifyKeyDown(IEditorView view, KeyEventArgs e)
        {
        }

        public void NotifyKeyUp(IEditorView view, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.SendPromptCommand();
                commandTextBox.Text = "";
            }
        }

        private void commandTextBox_DocumentTextChanged(object sender, EditorSnapshotChangedEventArgs e)
        {
            this.UpdateUI();
        }

        //private void AddCommandToList(string command)
        //{
        //    int i = comboBoxPreviousCommands.Items.IndexOf(command);
        //    if (i >= 0)
        //    {
        //        comboBoxPreviousCommands.Items.RemoveAt(i);
        //    }
        //    comboBoxPreviousCommands.Items.Insert(0, textBoxCommand.Text);
        //    comboBoxPreviousCommands.SelectedIndex = 0;
        //    this.UpdateUI();
        //}

        //private void checkBoxShowCommandButtons_CheckedChanged(object sender, EventArgs e)
        //{
        //    toolStripQuickCommands.Visible = checkBoxShowCommandButtons.Checked;
        //}

        //private void toolStripButtonAddCommand_Click(object sender, EventArgs e)
        //{
        //    panelCommandButtonInput.Left = (this.Width - panelCommandButtonInput.Width) / 2;
        //    panelCommandButtonInput.Visible = true;
        //    int i = textBoxCommand.Text.IndexOf(' ');
        //    if (i > 0) textBoxCommandButtonText.Text = textBoxCommand.Text.Substring(0, i);
        //    else textBoxCommandButtonText.Text = textBoxCommand.Text;
        //    textBoxCommandButtonText.SelectAll();
        //    textBoxCommandButtonText.Focus();
        //    //panelCommandButtonInput.Top = (this.Height - panelCommandButtonInput.Height) / 2;
        //}

        //private void comboBoxPreviousCommands_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (comboBoxPreviousCommands.Focused)
        //    {
        //        textBoxCommand.Text = comboBoxPreviousCommands.Items[comboBoxPreviousCommands.SelectedIndex] as string;
        //        textBoxCommand.SelectAll();
        //        textBoxCommand.Focus();
        //    }
        //}

        //private void textBoxCommandButtonText_Leave(object sender, EventArgs e)
        //{
        //    if (panelCommandButtonInput.Visible)
        //    {
        //        panelCommandButtonInput.Visible = false;
        //        textBoxCommand.Focus();
        //    }
        //}

        //private void buttonSendPrevious_Click(object sender, EventArgs e)
        //{
        //    var cmd = comboBoxPreviousCommands.Items[comboBoxPreviousCommands.SelectedIndex] as string;
        //    this.SendCommand(cmd);
        //}

        //private void buttonSendCommand_Click(object sender, EventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(textBoxCommand.Text))
        //    {
        //        this.SendCommand(textBoxCommand.Text);
        //    }
        //}

        //private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    simpleLogViewFull.ClearLog();
        //}
    }
}
