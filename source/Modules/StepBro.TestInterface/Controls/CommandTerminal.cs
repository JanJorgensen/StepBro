using FastColoredTextBoxNS;
using StepBro.Core.Attributes;
using StepBro.Core.Data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StepBro.TestInterface.Controls
{
    [ObjectPanelAttribute(allowMultipleInstances: false)]
    public partial class CommandTerminal : StepBro.Core.Controls.ObjectPanel
    {
        private TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        private TextStyle warningStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        private TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        private LogLineData m_firstSeen = null;
        private LogLineData m_lastHandled = null;
        private LogLineData m_oldestEntry = null;

        private SerialTestConnection m_connection = null;
        public CommandTerminal()
        {
            this.InitializeComponent();
        }

        private void CommandTerminal_Load(object sender, EventArgs e)
        {
            textBoxCommand.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBoxCommand.AutoCompleteSource = AutoCompleteSource.CustomSource;
            AutoCompleteStringCollection commands = new AutoCompleteStringCollection();
            commands.Add("anton");
            commands.Add("bent");
            commands.Add("chris");
            commands.Add("dennis");
            textBoxCommand.AutoCompleteCustomSource = commands;
        }

        public override bool IsBindable { get { return true; } }

        protected override bool TryBind(object @object)
        {
            if (!(@object is SerialTestConnection)) return false;
            m_connection = @object as SerialTestConnection;
            m_connection.LogLineAdded += this.Connection_LogLineAdded;
            return true;
        }

        private void Connection_LogLineAdded(object sender, LogLineEventArgs args)
        {
            throw new NotImplementedException();
        }

        protected override void DisconnectBinding()
        {
            m_connection = null;
        }

        public void AddCommandButton(string command)
        {
            var button = new System.Windows.Forms.ToolStripButton();
            button.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            button.Name = "toolStripButtonCommand_" + command;
            button.Size = new System.Drawing.Size(33, 22);
            button.Text = "command";
            button.Click += new System.EventHandler(this.toolStripButtonCommand_Click);
            toolStripQuickCommands.Items.Insert(toolStripQuickCommands.Items.Count - 1, button);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButtonCommand_Click(object sender, EventArgs e)
        {
            var commandButton = sender as ToolStripButton;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {

        }
    }
}
