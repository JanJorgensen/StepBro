using CommandLine;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.PanelCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class PanelsDialog : Form
    {
        private ICoreAccess m_coreAccess = null;

        public PanelsDialog()
        {
            InitializeComponent();
        }

        public PanelsDialog(ICoreAccess coreAccess) : this()
        {
            m_coreAccess = coreAccess;
        }


        public IPanelElement AddCustomPanel(string name, PropertyBlock definition)
        {
            //TabPage existing = null;
            //try
            //{
            //    existing = tabControlPanels.TabPages[name];
            //}
            //finally { }
            //foreach (object page in tabControlPanels.TabPages)
            //{
            //    System.Diagnostics.Debug.WriteLine("Page: " + page.GetType().Name);
            //    if (page.Name == name)
            //    {
            //        existing = page;
            //        break;
            //    }
            //}
            var existing = tabControlPanels.TabPages.Cast<TabPage>().FirstOrDefault(p => p.Name == name);
            if (existing != null)
            {
                var panelContainer = existing.Controls[0] as StepBro.UI.WinForms.CustomPanelContainer;
                return panelContainer.SetCustomPanelDefinition(name, definition);
            }
            else
            {
                var panelContainer = new StepBro.UI.WinForms.CustomPanelContainer(m_coreAccess);
                var page = new TabPage(name);
                page.Name = name;
                tabControlPanels.TabPages.Add(page);
                page.Controls.Add(panelContainer);
                panelContainer.Dock = DockStyle.Fill;
                panelContainer.Location = new Point(0, 0);
                panelContainer.Size = tabControlPanels.TabPages[tabControlPanels.TabPages.Count - 1].ClientSize;
                return panelContainer.SetCustomPanelDefinition(name, definition);
            }
        }
    }
}
