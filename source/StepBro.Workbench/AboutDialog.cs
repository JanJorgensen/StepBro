using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace StepBro.Workbench
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            labelAppVersion.Text = typeof(Core.Main).Assembly.GetName().Version.ToString();
        }
    }
}