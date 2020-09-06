using StepBro.Core;
using StepBro.Core.Controls;
using StepBro.Core.Data;
using System.Linq;

namespace StepBro.Workbench
{
    public partial class ObjectPanelDockWindow : ToolWindow
    {
        private ObjectPanel m_panel = null;
        private string[] m_loadSpecification = null;
        private string m_objectName = null;
        private ObjectPanelInfo m_panelType = null;
        private ServiceManager m_services = null;

        public ObjectPanelDockWindow()
        {
            this.InitializeComponent();
        }

        public ObjectPanelDockWindow(ServiceManager services) : this()
        {
            m_services = services;
        }

        public static readonly string PersistTitle = nameof(ObjectPanelDockWindow);

        protected override string GetPersistString()
        {
            if (m_panel == null) return PersistTitle;
            else
            {
                if (m_panel.IsBindable)
                {
                    return PersistTitle + "," + m_panel.GetType().ToString() + "," + m_panel.TargetObjectReference;
                }
                else
                {
                    return PersistTitle + "," + m_panel.GetType().ToString();
                }
            }
        }

        public void SetPanel(ObjectPanel panel)
        {
            System.Diagnostics.Debug.Assert(m_panel == null);
            System.Diagnostics.Debug.Assert(m_loadSpecification == null);

            m_panel = panel;
            this.SetupPanel();
            this.UpdateWindowFromPanelState();
        }

        private void SetupPanel()
        {
            m_panel.BindingStateChanged += this.Panel_BindingStateChanged;
            m_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            m_panel.Location = new System.Drawing.Point(0, 0);
            m_panel.Name = "panel";
            m_panel.Size = new System.Drawing.Size(800, 450);
            m_panel.TabIndex = 0;
            m_panel.Visible = false;
            labelErrorMessage.Visible = false;
            this.Text = m_panel.Name;
            this.Controls.Add(m_panel);
        }

        private void Panel_BindingStateChanged(object sender, System.EventArgs e)
        {
            this.UpdateWindowFromPanelState();
        }

        private void BoundObjectContainer_Disposing(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        public void SetupFromLoadSpecification(string[] specification)
        {
            if (specification == null) throw new System.ArgumentNullException();
            System.Diagnostics.Debug.Assert(m_panel == null);
            System.Diagnostics.Debug.Assert(m_loadSpecification == null);
            System.Diagnostics.Debug.Assert(m_services != null);
            System.Diagnostics.Debug.Assert(specification.Length >= 1);

            m_loadSpecification = specification;
            m_objectName = (m_loadSpecification.Length > 1) ? m_loadSpecification[1] : null;

            var panelManager = m_services.Get<IObjectPanelManager>();
            var panelInfo = panelManager.FindPanel(m_loadSpecification[0]);
            if (panelInfo != null)
            {
                m_panel = panelManager.CreatePanel(panelInfo, null);
                if (m_panel != null)
                {
                    this.SetupPanel();
                    if (m_panel.IsBindable && m_loadSpecification.Length >= 2)
                    {
                        m_panel.SetObjectReference(m_loadSpecification[1]);
                    }
                    this.UpdateWindowFromPanelState();
                }
                else
                {
                    labelErrorMessage.Text = "Failed to create panel named \"" + specification[0] + "\".";
                }
            }
            else
            {
                labelErrorMessage.Text = "Unable to find panel named \"" + specification[0] + "\".";
            }
        }

        private void UpdateWindowFromPanelState()
        {
            switch (m_panel.State)
            {
                case ObjectPanel.BindingState.NotBindable:
                case ObjectPanel.BindingState.Bound:
                    m_panel.Visible = true;
                    labelErrorMessage.Visible = false;
                    break;

                case ObjectPanel.BindingState.NotBound:
                    m_panel.Visible = false;
                    labelErrorMessage.Visible = true;
                    labelErrorMessage.Text = "Awaiting object named \"" + m_objectName + "\" to created.";
                    break;
                case ObjectPanel.BindingState.BoundWithoutObject:
                    m_panel.Visible = false;
                    labelErrorMessage.Visible = true;
                    labelErrorMessage.Text = "Awaiting object to be re-created.";
                    break;
                case ObjectPanel.BindingState.BindingFailed:
                    m_panel.Visible = false;
                    labelErrorMessage.Visible = true;
                    labelErrorMessage.Text = "Binding to object named \"" + m_objectName + "\" failed.";
                    break;
                default:
                    break;
            }
        }

        //private void ObjectPanelDockWindow_ObjectListChanged(object sender, System.EventArgs e)
        //{
        //    if (m_panel == null)
        //    {
        //        this.BeginInvoke(new System.Action(() =>
        //        {
        //            var container = this.TryFindSpecifiedObject();
        //            if (container != null)
        //            {
        //                var panelManager = m_services.Get<IObjectPanelManager>();
        //                m_panel = panelManager.CreatePanel(m_panelType, container);
        //                if (m_panel != null)
        //                {
        //                    this.SetupPanel();
        //                }
        //            }
        //        }));
        //    }
        //}
    }
}
