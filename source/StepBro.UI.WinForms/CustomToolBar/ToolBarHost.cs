using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public partial class ToolBarHost : UserControl
    {
        private bool m_visible = true;
        private bool m_settingVisibility = false;

        public ToolBarHost()
        {
            InitializeComponent();
            m_visible = this.Visible;
            this.AdjustHeight();    // Initially hidden; no toolbars yet.
        }

        public void AdjustHeight()
        {
            if (this.Controls.Count > 0)
            {
                if (!this.Visible && m_visible)
                {
                    m_settingVisibility = true;
                    this.Visible = true;
                    m_settingVisibility = false;
                }
                this.Height = this.Controls[0].Bounds.Bottom + 3;
            }
            else
            {
                if (this.Visible)
                {
                    m_settingVisibility = true;
                    this.Visible = false;
                    m_settingVisibility = false;
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!m_settingVisibility)
            {
                m_visible = this.Visible;
            }

        }
    }
}
