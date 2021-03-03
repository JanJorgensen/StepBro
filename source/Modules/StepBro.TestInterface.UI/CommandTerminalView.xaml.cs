using StepBro.Core.Tasks;
using StepBro.TestInterface.UI;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.TestInterface.Controls
{
    /// <summary>
    /// Interaction logic for CommandTerminal.xaml
    /// </summary>
    public partial class CommandTerminalView : UserControl
    {
        private IAsyncResult<object> m_activeCommand = null;

        public CommandTerminalView()
        {
            this.InitializeComponent();
            this.UpdateUI();
        }

        private SerialTestConnection Connection
        {
            get
            {
                return (this.DataContext as CommandTerminalViewModel)?.Connection;
            }
        }
        private void sendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

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

        //protected override void OnHandleCreated(EventArgs e)
        //{
        //    base.OnHandleCreated(e);
        //    if (m_connection != null)
        //    {
        //        this.BeginInvoke(new Action(this.AddCommandsFromObject));
        //    }
        //}

        //private void ConnectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(m_connection.UICommands))
        //    {
        //        this.BeginInvoke(new Action(this.AddCommandsFromObject));
        //    }
        //}

        //private void AddCommandsFromObject()
        //{
        //    if (m_connection.UICommands != null && toolStripQuickCommands.Items.Count <= 1)   // Note: There's an "Add" button there.
        //    {
        //        foreach (var c in m_connection.UICommands)
        //        {
        //            this.AddCommandButton(c.Item1, c.Item2);
        //        }
        //    }
        //    this.UpdateUI();
        //}

        //protected override void DisconnectBinding()
        //{
        //    simpleLogViewFull.SetSource(null);
        //    m_connection = null;
        //}

        //public void AddCommandButton(string name, string command)
        //{
        //    var button = new ToolStripButton();
        //    button.Alignment = ToolStripItemAlignment.Left;
        //    button.DisplayStyle = ToolStripItemDisplayStyle.Text;
        //    button.Name = "toolStripButtonCommand_" + name.Replace(' ', '_');
        //    button.Size = new Size(33, 22);
        //    button.Text = name;
        //    button.Click += new EventHandler(this.toolStripButtonCommand_Click);
        //    button.Tag = command;
        //    toolStripQuickCommands.Items.Insert(toolStripQuickCommands.Items.Count - 1, button);
        //    checkBoxShowCommandButtons.Checked = true;  // Ensure visible
        //    this.UpdateUI();
        //}

        //private void toolStripButtonCommand_Click(object sender, EventArgs e)
        //{
        //    var commandButton = sender as ToolStripButton;
        //    var command = commandButton.Tag as string;
        //    textBoxCommand.Text = command;
        //    textBoxCommand.SelectAll();
        //    textBoxCommand.Focus();
        //    this.SendCommand(command);
        //}

        private void SendCommand(string command)
        {
            bool doCommand = true;
            var connection = this.Connection;
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
                //m_activeCommand = 
                    connection.SendCommand(null, command);
                //this.AddCommandToList(command);
                //commandTextBox.Document.SelectAll();
                commandTextBox.Focus();
                sendButton.IsEnabled = false;
                //timer.Enabled = true;
            }
            //simpleLogViewFull.FollowEnd();
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

        //private void timer_Tick(object sender, EventArgs e)
        //{
        //    if (m_activeCommand != null && m_activeCommand.IsCompleted)
        //    {
        //        timer.Enabled = false;
        //        m_activeCommand = null;
        //        this.UpdateUI();
        //    }
        //}

        //private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    simpleLogViewFull.ClearLog();
        //}
    }
}
