using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms
{
    public partial class DialogAddShortcut : Form
    {
        private string m_procedureButtonText = "";
        private string m_commandButtonText = "";

        public DialogAddShortcut()
        {
            InitializeComponent();
        }

        public DialogAddShortcut(
            String caption,
            string procedureDescription,
            string commandDescription,
            string defaultNameProcedureExecution,
            string defaultNameCommandExecution)
        {
            InitializeComponent();
            this.Text = caption;
            labelProcedureDetails.Text = procedureDescription;
            labelCommandDetails.Text = commandDescription;
            m_procedureButtonText = defaultNameProcedureExecution;

            if (!String.IsNullOrEmpty(procedureDescription))
            {
                radioButtonProcedureExecution.Checked = true;
                textBoxString.Text = defaultNameProcedureExecution;
            }
            else
            {
                radioButtonProcedureExecution.Enabled = false;
                labelProcedureDetails.Text = "";
            }
            if (!String.IsNullOrEmpty(commandDescription))
            {
                if (radioButtonProcedureExecution.Enabled)
                {
                    radioButtonObjectCommand.Checked = false;
                }
                else
                {
                    radioButtonObjectCommand.Checked = true;
                    textBoxString.Text = defaultNameCommandExecution;
                }
            }
            else
            {
                radioButtonObjectCommand.Enabled = false;
                labelCommandDetails.Text = "";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            textBoxString.Focus();
            textBoxString.SelectAll();
        }

        private void radioButtonProcedureExecution_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonProcedureExecution.Checked)
            {
                m_commandButtonText = textBoxString.Text;
                textBoxString.Text = m_procedureButtonText;
            }
        }

        private void radioButtonObjectCommand_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonObjectCommand.Checked)
            {
                m_procedureButtonText = textBoxString.Text;
                textBoxString.Text = m_commandButtonText;
            }
        }

        public string ButtonText { get { return textBoxString.Text; } }

        public bool ProcedureExecutionSelected { get {  return radioButtonProcedureExecution.Checked; } }

        public bool CommandExecutionSelected { get {  return radioButtonObjectCommand.Checked; } }
    }
}
