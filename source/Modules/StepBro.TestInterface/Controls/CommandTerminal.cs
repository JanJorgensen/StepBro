using StepBro.Core.Attributes;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace StepBro.TestInterface.Controls
{
    [ObjectPanelAttribute(allowMultipleInstances: false)]
    public partial class CommandTerminal : StepBro.Core.Controls.ObjectPanel
    {
        private SerialTestConnection m_connection = null;
        public CommandTerminal()
        {
            this.InitializeComponent();
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (textBoxCommand.Focused)
                {
                    if (!String.IsNullOrEmpty(textBoxCommand.Text))
                    {
                        m_connection.SendCommand(textBoxCommand.Text);
                        this.AddCommandToList(textBoxCommand.Text);
                        textBoxCommand.SelectAll();
                    }
                    return true;
                }
                else if (comboBoxPreviousCommands.Focused && !comboBoxPreviousCommands.DroppedDown)
                {
                    return true;
                }
                else if (textBoxCommandButtonText.Focused)
                {
                    AddCommandButton(textBoxCommandButtonText.Text, textBoxCommand.Text);
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
            return true;
        }

        protected override void DisconnectBinding()
        {
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
            button.Tag = textBoxCommand.Text;
            toolStripQuickCommands.Items.Insert(toolStripQuickCommands.Items.Count - 1, button);
        }

        private void toolStripButtonCommand_Click(object sender, EventArgs e)
        {
            var commandButton = sender as ToolStripButton;
            var command = commandButton.Tag as string;
            m_connection.SendCommand(command);
            textBoxCommand.Text = command;
            textBoxCommand.SelectAll();
            textBoxCommand.Focus();
            AddCommandToList(command);
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
    }
}
