using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.Workbench.ToolViews
{
    internal class ObjectPanelToolViewModel : ToolItemViewModel
    {
        public ObjectPanelToolViewModel()
        {
            this.SerializationId = "ToolObjectPanel:Grigferlig";
            this.Title = "ObjectPanel";

            m_contentControl = CreateBindProblemIndicator();
        }

        private UserControl CreateBindProblemIndicator()
        {
            var ctrl = new UserControl();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1000, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1000, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1000, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1000, GridUnitType.Star) });
            ctrl.Content = grid;
            var header = new TextBlock()
            {
                Text = "Cannot show panel", 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var text = new TextBlock()
            {
                Text = "Waiting for it to be created",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            grid.Children.Add(header);
            Grid.SetColumn(header, 1);
            Grid.SetRow(header, 1);
            grid.Children.Add(text);
            Grid.SetColumn(text, 1);
            Grid.SetRow(text, 3);
            return ctrl;
        }

        void SetSteteText()
        {
            //switch (m_panel.State)
            //{
            //    case ObjectPanelBindingState.NotBindable:
            //    case ObjectPanelBindingState.Bound:
            //        m_panel.Visible = true;
            //        labelErrorMessage.Visible = false;
            //        break;

            //    case ObjectPanelBindingState.NotBound:
            //        m_panel.Visible = false;
            //        labelErrorMessage.Visible = true;
            //        labelErrorMessage.Text = "Awaiting object named \"" + m_objectName + "\" to created.";
            //        break;
            //    case ObjectPanelBindingState.BoundWithoutObject:
            //        m_panel.Visible = false;
            //        labelErrorMessage.Visible = true;
            //        labelErrorMessage.Text = "Awaiting object to be re-created.";
            //        break;
            //    case ObjectPanelBindingState.BindingFailed:
            //        m_panel.Visible = false;
            //        labelErrorMessage.Visible = true;
            //        labelErrorMessage.Text = "Binding to object named \"" + m_objectName + "\" failed.";
            //        break;
            //    default:
            //        break;
            //}

        }

        private UserControl m_contentControl;

        public UserControl ContentControl
        {
            get
            {
                this.Title = "ObjectPanel for someone";
                return m_contentControl;
            }
            private set
            {
                m_contentControl = value;
                this.NotifyPropertyChanged(nameof(ContentControl));
            }
        }


        //private Core.Controls.WinForms.ObjectPanel m_panel = null;
        //private string[] m_loadSpecification = null;
        //private string m_objectName = null;
        //private ObjectPanelInfo m_panelType = null;
        //private ServiceManager m_services = null;

        //public ObjectPanelDockWindow(ServiceManager services) : this()
        //{
        //    m_services = services;
        //}

        //protected override void OnDockStateChanged(EventArgs e)
        //{
        //    if (this.DockState == WeifenLuo.WinFormsUI.Docking.DockState.Unknown)
        //    {
        //        m_panel.Dispose();
        //    }
        //    base.OnDockStateChanged(e);
        //}

        //public static readonly string PersistTitle = nameof(ObjectPanelDockWindow);

        //protected override string GetPersistString()
        //{
        //    if (m_panel == null) return PersistTitle;
        //    else
        //    {
        //        if (m_panel.IsBindable)
        //        {
        //            return PersistTitle + "," + m_panel.GetType().ToString() + "," + m_panel.TargetObjectReference;
        //        }
        //        else
        //        {
        //            return PersistTitle + "," + m_panel.GetType().ToString();
        //        }
        //    }
        //}

        //public void SetPanel(ObjectPanelInfo type, Core.Controls.WinForms.ObjectPanel panel)
        //{
        //    System.Diagnostics.Debug.Assert(m_panel == null);
        //    System.Diagnostics.Debug.Assert(m_loadSpecification == null);

        //    m_panelType = type;
        //    m_panel = panel;
        //    m_objectName = (panel.BoundObject != null) ? panel.BoundObject.FullName : null;
        //    this.SetupPanel();
        //    this.UpdateWindowFromPanelState();
        //}

        //private void SetupPanel()
        //{
        //    m_panel.BindingStateChanged += this.Panel_BindingStateChanged;
        //    m_panel.Dock = System.Windows.Forms.DockStyle.Fill;
        //    m_panel.Location = new System.Drawing.Point(0, 0);
        //    m_panel.Name = "panel";
        //    m_panel.Size = new System.Drawing.Size(800, 450);
        //    m_panel.TabIndex = 0;
        //    m_panel.Visible = false;
        //    labelErrorMessage.Visible = false;
        //    if (!String.IsNullOrEmpty(m_objectName))
        //    {
        //        this.TabText = m_panelType.Name + " - " + m_objectName;
        //    }
        //    else
        //    {
        //        this.TabText = m_panelType.Name;
        //    }
        //    this.Controls.Add(m_panel);
        //}

        //private void Panel_BindingStateChanged(object sender, System.EventArgs e)
        //{
        //    this.UpdateWindowFromPanelState();
        //}

        //private void BoundObjectContainer_Disposing(object sender, System.EventArgs e)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public void SetupFromLoadSpecification(string[] specification)
        //{
        //    if (specification == null) throw new System.ArgumentNullException();
        //    System.Diagnostics.Debug.Assert(m_panel == null);
        //    System.Diagnostics.Debug.Assert(m_loadSpecification == null);
        //    System.Diagnostics.Debug.Assert(m_services != null);
        //    System.Diagnostics.Debug.Assert(specification.Length >= 1);

        //    m_loadSpecification = specification;
        //    m_objectName = (m_loadSpecification.Length > 1) ? m_loadSpecification[1] : null;

        //    var panelManager = m_services.Get<IObjectPanelManager>();
        //    m_panelType = panelManager.FindPanel(m_loadSpecification[0]);
        //    if (m_panelType != null)
        //    {
        //        if (m_loadSpecification.Length >= 2)
        //        {
        //            m_panel = panelManager.CreateObjectWinFormsPanel(m_panelType, m_loadSpecification[1]);
        //        }
        //        else
        //        {
        //            m_panel = panelManager.CreateStaticWinFormsPanel(m_panelType);
        //        }
        //        if (m_panel != null)
        //        {
        //            this.SetupPanel();
        //            //if (m_panel.IsBindable && m_loadSpecification.Length >= 2)
        //            //{
        //            //    m_panel.SetObjectReference();
        //            //}
        //            this.UpdateWindowFromPanelState();
        //        }
        //        else
        //        {
        //            labelErrorMessage.Text = "Failed to create panel named \"" + specification[0] + "\".";
        //        }
        //    }
        //    else
        //    {
        //        labelErrorMessage.Text = "Unable to find panel named \"" + specification[0] + "\".";
        //    }
        //}

        //private void UpdateWindowFromPanelState()
        //{
        //    switch (m_panel.State)
        //    {
        //        case ObjectPanelBindingState.NotBindable:
        //        case ObjectPanelBindingState.Bound:
        //            m_panel.Visible = true;
        //            labelErrorMessage.Visible = false;
        //            break;

        //        case ObjectPanelBindingState.NotBound:
        //            m_panel.Visible = false;
        //            labelErrorMessage.Visible = true;
        //            labelErrorMessage.Text = "Awaiting object named \"" + m_objectName + "\" to created.";
        //            break;
        //        case ObjectPanelBindingState.BoundWithoutObject:
        //            m_panel.Visible = false;
        //            labelErrorMessage.Visible = true;
        //            labelErrorMessage.Text = "Awaiting object to be re-created.";
        //            break;
        //        case ObjectPanelBindingState.BindingFailed:
        //            m_panel.Visible = false;
        //            labelErrorMessage.Visible = true;
        //            labelErrorMessage.Text = "Binding to object named \"" + m_objectName + "\" failed.";
        //            break;
        //        default:
        //            break;
        //    }
        //}










    }
}
