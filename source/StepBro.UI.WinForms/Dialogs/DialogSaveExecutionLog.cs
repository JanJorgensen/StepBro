using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Dialogs
{
    public partial class DialogSaveExecutionLog : Form
    {
        public enum SaveOption
        {
            SaveCurrentOrLastExecution,
            SaveEntireLog,
            SaveAllSinceClear,
            SaveAllInLogView,
            SaveSelectedInLogView
        }

        private string m_targetFolder = null;

        public DialogSaveExecutionLog()
        {
            InitializeComponent();
        }

        public DialogSaveExecutionLog(string targetFolder) : this()
        {
            m_targetFolder = targetFolder;
            linkLabelTargetFolder.Text = targetFolder;
        }

        public SaveOption SelectedOption { get; private set; }

        public string TargetFolder
        {
            get { return m_targetFolder; }
            set
            {
                m_targetFolder = value;
                linkLabelTargetFolder.Text = value;
            }
        }

        private void radioButtonOption_Click(object sender, EventArgs e)
        {
            if (radioButtonSaveLastExecution.Checked) { this.SelectedOption = SaveOption.SaveCurrentOrLastExecution; }
            else if (radioButtonEntireLog.Checked) { this.SelectedOption = SaveOption.SaveEntireLog; }
            else if (radioButtonAfterClear.Checked) { this.SelectedOption = SaveOption.SaveAllSinceClear; }
        }

        private void linkLabelTargetFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!String.IsNullOrEmpty(m_targetFolder) && Directory.Exists(m_targetFolder))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = m_targetFolder,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show(string.Format("{0} Directory does not exist!", m_targetFolder));
            }
        }
    }
}
