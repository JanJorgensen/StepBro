using StepBro.Core.Api;
using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Controls
{
    public partial class ToolInteractionPopup : Form
    {
        ICoreAccess m_coreAccess = null;

        public ToolInteractionPopup()
        {
            InitializeComponent();
        }

        public ToolInteractionPopup(ICoreAccess coreAccess) : this()
        {
            m_coreAccess = coreAccess;
            toolInteractionView.Bind(coreAccess);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            toolInteractionView.DataContext = this.DataContext;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            this.Close();
        }

        private void toolInteractionView_TextCommandToolSelected(object sender, EventArgs e)
        {
            //this.Close();     // Enable again when finding a solution to wait for mouse button release (avoid release on whatever behind dialog).
        }
    }
}
