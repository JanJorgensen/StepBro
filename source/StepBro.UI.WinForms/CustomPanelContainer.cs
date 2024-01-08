using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.PanelCreator;
using StepBro.UI.WinForms.PanelElements;

namespace StepBro.UI.WinForms
{
    public partial class CustomPanelContainer : UserControl
    {
        private ICoreAccess m_coreAccess = null;

        public CustomPanelContainer()
        {
            InitializeComponent();
        }

        public CustomPanelContainer(ICoreAccess coreAccess) : this()
        {
            m_coreAccess = coreAccess;
        }

        public void SetCoreAccess(ICoreAccess coreAccess)
        {
            m_coreAccess = coreAccess;
        }

        public IPanelElement SetCustomPanelDefinition(string name, PropertyBlock definition)
        {
            SuspendLayout();
            panelParent.Controls.Clear();
            var rootPanel = PanelElementBase.Create(null, definition, m_coreAccess);
            if (rootPanel != null)
            {
                panelParent.Controls.Add(rootPanel);
                rootPanel.Location = new Point(0, 0);
                rootPanel.Size = this.Size;
                rootPanel.Dock = DockStyle.Fill;
            }
            ResumeLayout(false);
            return rootPanel as IPanelElement;
        }
    }
}