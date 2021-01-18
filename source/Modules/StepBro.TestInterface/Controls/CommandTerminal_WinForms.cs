using StepBro.Core.Attributes;
using StepBro.Core.Tasks;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StepBro.TestInterface.Controls
{
    [ObjectPanel(allowMultipleInstances: false)]
    public partial class CommandTerminal_WinForms : StepBro.Core.Controls.WinForms.ObjectPanel
    {
        private SerialTestConnection m_connection = null;
        private IAsyncResult<object> m_activeCommand = null;

        public CommandTerminal_WinForms()
        {
            this.InitializeComponent();
            this.UpdateUI();
        }

        private void CommandTerminal_Load(object sender, EventArgs e)
        {
            //textBoxCommand.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //textBoxCommand.AutoCompleteSource = AutoCompleteSource.CustomSource;
            //AutoCompleteStringCollection commands = new AutoCompleteStringCollection();
            //commands.Add("anton");
            //commands.Add("bent");
            //commands.Add("chris");
            //commands.Add("dennis");
            //textBoxCommand.AutoCompleteCustomSource = commands;
        }

        private void UpdateUI()
        {
            bool allowSend = (m_activeCommand == null);
            buttonSendCommand.Enabled = (allowSend && !String.IsNullOrEmpty(textBoxCommand.Text));
            comboBoxPreviousCommands.Enabled = (comboBoxPreviousCommands.Items.Count > 0);
            buttonSendPrevious.Enabled = (allowSend && comboBoxPreviousCommands.Items.Count > 0 && comboBoxPreviousCommands.SelectedIndex >= 0);
            foreach (ToolStripItem tbb in toolStripQuickCommands.Items) tbb.Enabled = allowSend;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (textBoxCommand.Focused)
                {
                    if (m_connection != null && m_activeCommand == null && !String.IsNullOrEmpty(textBoxCommand.Text))
                    {
                        this.SendCommand(textBoxCommand.Text);
                    }
                    return true;
                }
                else if (comboBoxPreviousCommands.Focused && !comboBoxPreviousCommands.DroppedDown)
                {
                    return true;
                }
                else if (textBoxCommandButtonText.Focused)
                {
                    this.AddCommandButton(textBoxCommandButtonText.Text, textBoxCommand.Text);
                    panelCommandButtonInput.Visible = false;
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public override bool IsBindable { get { return true; } }

        protected override bool TryBind(object @object)
        {
            if (!(@object is SerialTestConnection)) return false;
            m_connection = @object as SerialTestConnection;
            simpleLogViewFull.SetSource(m_connection);
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(this.AddCommandsFromObject));
            }
            m_connection.PropertyChanged += this.ConnectionPropertyChanged;
            return true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (m_connection != null)
            {
                this.BeginInvoke(new Action(this.AddCommandsFromObject));
            }
        }

        private void ConnectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(m_connection.UICommands))
            {
                this.BeginInvoke(new Action(this.AddCommandsFromObject));
            }
        }

        private void AddCommandsFromObject()
        {
            if (m_connection.UICommands != null && toolStripQuickCommands.Items.Count <= 1)   // Note: There's an "Add" button there.
            {
                foreach (var c in m_connection.UICommands)
                {
                    this.AddCommandButton(c.Item1, c.Item2);
                }
            }
            this.UpdateUI();
        }

        protected override void DisconnectBinding()
        {
            simpleLogViewFull.SetSource(null);
            m_connection = null;
        }

        public void AddCommandButton(string name, string command)
        {
            var button = new ToolStripButton();
            button.Alignment = ToolStripItemAlignment.Left;
            button.DisplayStyle = ToolStripItemDisplayStyle.Text;
            button.Name = "toolStripButtonCommand_" + name.Replace(' ', '_');
            button.Size = new Size(33, 22);
            button.Text = name;
            button.Click += new EventHandler(this.toolStripButtonCommand_Click);
            button.Tag = command;
            toolStripQuickCommands.Items.Insert(toolStripQuickCommands.Items.Count - 1, button);
            checkBoxShowCommandButtons.Checked = true;  // Ensure visible
            this.UpdateUI();
        }

        private void toolStripButtonCommand_Click(object sender, EventArgs e)
        {
            var commandButton = sender as ToolStripButton;
            var command = commandButton.Tag as string;
            textBoxCommand.Text = command;
            textBoxCommand.SelectAll();
            textBoxCommand.Focus();
            this.SendCommand(command);
        }

        private void SendCommand(string command)
        {
            bool doCommand = true;
            if (!m_connection.IsConnected())
            {
                if (m_connection.Stream != null)
                {
                    if (MessageBox.Show(this, "Do you wish to open the port/connaction?", "Serial Test Interface is not connacted", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        doCommand = m_connection.Connect(Core.Main.CreateUICallContext());
                    }
                    else
                    {
                        doCommand = false;
                    }
                }
            }
            if (doCommand)
            {
                m_activeCommand = m_connection.SendCommand(null, command);
                this.AddCommandToList(command);
                textBoxCommand.SelectAll();
                textBoxCommand.Focus();
                buttonSendCommand.Enabled = false;
                timer.Enabled = true;
            }
            simpleLogViewFull.FollowEnd();
            this.UpdateUI();
        }

        private void AddCommandToList(string command)
        {
            int i = comboBoxPreviousCommands.Items.IndexOf(command);
            if (i >= 0)
            {
                comboBoxPreviousCommands.Items.RemoveAt(i);
            }
            comboBoxPreviousCommands.Items.Insert(0, textBoxCommand.Text);
            comboBoxPreviousCommands.SelectedIndex = 0;
            this.UpdateUI();
        }

        private void checkBoxShowCommandButtons_CheckedChanged(object sender, EventArgs e)
        {
            toolStripQuickCommands.Visible = checkBoxShowCommandButtons.Checked;
        }

        private void toolStripButtonAddCommand_Click(object sender, EventArgs e)
        {
            panelCommandButtonInput.Left = (this.Width - panelCommandButtonInput.Width) / 2;
            panelCommandButtonInput.Visible = true;
            int i = textBoxCommand.Text.IndexOf(' ');
            if (i > 0) textBoxCommandButtonText.Text = textBoxCommand.Text.Substring(0, i);
            else textBoxCommandButtonText.Text = textBoxCommand.Text;
            textBoxCommandButtonText.SelectAll();
            textBoxCommandButtonText.Focus();
            //panelCommandButtonInput.Top = (this.Height - panelCommandButtonInput.Height) / 2;
        }

        private void comboBoxPreviousCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxPreviousCommands.Focused)
            {
                textBoxCommand.Text = comboBoxPreviousCommands.Items[comboBoxPreviousCommands.SelectedIndex] as string;
                textBoxCommand.SelectAll();
                textBoxCommand.Focus();
            }
        }

        private void textBoxCommandButtonText_Leave(object sender, EventArgs e)
        {
            if (panelCommandButtonInput.Visible)
            {
                panelCommandButtonInput.Visible = false;
                textBoxCommand.Focus();
            }
        }

        private void buttonSendPrevious_Click(object sender, EventArgs e)
        {
            var cmd = comboBoxPreviousCommands.Items[comboBoxPreviousCommands.SelectedIndex] as string;
            this.SendCommand(cmd);
        }

        private void buttonSendCommand_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxCommand.Text))
            {
                this.SendCommand(textBoxCommand.Text);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (m_activeCommand != null && m_activeCommand.IsCompleted)
            {
                timer.Enabled = false;
                m_activeCommand = null;
                this.UpdateUI();
            }
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            simpleLogViewFull.ClearLog();
        }
    }
}
