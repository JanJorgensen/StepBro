using StepBro.Core;
using StepBro.Core.Data;
using StepBro.UI.Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StepBro.Workbench.ToolViews
{
    internal class CustomPanelToolViewModel : ToolItemViewModel
    {
        private CustomPanelInstanceData m_panelInstanceData;
        private UserControl m_contentControl;
        private UserControl m_indcatorControl;
        private TextBlock m_header = null;
        private TextBlock m_message = null;
        private ServiceManager m_services;
        private bool m_updatingSerializationId = false;

        public CustomPanelToolViewModel(ServiceManager services)
        {
            m_services = services;
            this.DestructWhenClosed = true;
            m_updatingSerializationId = true;
            this.SerializationId = "ToolCustomPanel";
            m_updatingSerializationId = false;
            this.Title = this.CreateTitle();

            CreateBindProblemIndicator();
        }

        public CustomPanelToolViewModel(ServiceManager services, CustomPanelInstanceData panelData) : this(services)
        {
            m_panelInstanceData = panelData;
            m_updatingSerializationId = true;
            this.SerializationId = "ToolCustomPanel|" + CreateSpecificSerializationId();
            m_updatingSerializationId = false;
            this.Title = this.CreateTitle();
            panelData.PropertyChanged += PanelData_PropertyChanged;
            this.UpdateFromBindingState();
        }

        private void PanelData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CustomPanelInstanceData.State))
            {
                this.UpdateFromBindingState();
            }
        }

        private void UpdateFromBindingState()
        {
            this.Title = this.CreateTitle();
            switch (m_panelInstanceData.State)
            {
                case Core.Controls.CustomPanelBindingState.Bound:
                    MainViewModel.Invoke((s) =>
                    {
                        this.ContentControl = m_panelInstanceData.GetPanelControl();
                    });
                    break;
                default:
                    MainViewModel.Invoke((s) =>
                    {
                        this.ContentControl = null;     // Show binding indicator
                    });
                    break;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SerializationId) && !m_updatingSerializationId)
            {
                if (m_panelInstanceData == null)
                {
                    CreateInstanceDataFromSerializationId(this.SerializationId);
                }
            }
            else if (e.PropertyName == nameof(IsOpen))
            {
                if (!this.IsOpen)
                {
                    if (m_panelInstanceData != null)
                    {
                        m_panelInstanceData.Dispose();
                    }
                }
            }
            base.OnPropertyChanged(e);
        }

        private UserControl CreateBindProblemIndicator()
        {
            m_indcatorControl = new UserControl();
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1000, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1000, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1000, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1000, GridUnitType.Star) });
            m_indcatorControl.Content = grid;
            m_header = new TextBlock()
            {
                Text = "Cannot show panel",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            m_message = new TextBlock()
            {
                Text = "Waiting for it to be created",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            grid.Children.Add(m_header);
            Grid.SetColumn(m_header, 1);
            Grid.SetRow(m_header, 1);
            grid.Children.Add(m_message);
            Grid.SetColumn(m_message, 1);
            Grid.SetRow(m_message, 3);
            return m_indcatorControl;
        }

        private string CreateSpecificSerializationId()
        {
            if (m_panelInstanceData.PanelType != null)
            {
                if (m_panelInstanceData.BoundObjectContainer != null)
                {
                    return m_panelInstanceData.PanelType.TypeIdentification + "|" + m_panelInstanceData.BoundObjectContainer.FullName;
                }
                else
                {
                    if (String.IsNullOrEmpty(m_panelInstanceData.TargetObjectReference))
                    {
                        return m_panelInstanceData.PanelType.TypeIdentification;
                    }
                    else
                    {
                        return m_panelInstanceData.PanelType.TypeIdentification + "|" + m_panelInstanceData.TargetObjectReference;
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(m_panelInstanceData.TargetObjectReference))
                {
                    return m_panelInstanceData.TypeName;
                }
                else
                {
                    return m_panelInstanceData.TypeName + "|" + m_panelInstanceData.TargetObjectReference;
                }
            }
        }

        internal void CreateInstanceDataFromSerializationId(string id)
        {
            var parts = id.Split('|');
            System.Diagnostics.Debug.Assert(parts.Length >= 2 && parts.Length <= 3);

            if (parts.Length > 2)
            {
                m_panelInstanceData = m_services.Get<ICustomPanelManager>().CreateObjectPanel(parts[1], parts[2]);
            }
            else
            {
                m_panelInstanceData = m_services.Get<ICustomPanelManager>().CreateStaticPanel(parts[1]);
            }
            m_panelInstanceData.PropertyChanged += PanelData_PropertyChanged;
            this.UpdateFromBindingState();
        }

        private string CreateTitle()
        {
            if (m_panelInstanceData != null)
            {
                if (m_panelInstanceData.PanelType != null)
                {
                    switch (m_panelInstanceData.State)
                    {
                        case Core.Controls.CustomPanelBindingState.NotBindable:
                            return m_panelInstanceData.PanelType.Name;
                        case Core.Controls.CustomPanelBindingState.NotBound:
                            return m_panelInstanceData.TypeName;
                        case Core.Controls.CustomPanelBindingState.BoundWithoutObject:
                        case Core.Controls.CustomPanelBindingState.BindingFailed:
                            return m_panelInstanceData.TargetObjectReference + " - " + m_panelInstanceData.PanelType.Name;
                        case Core.Controls.CustomPanelBindingState.Bound:
                            return (m_panelInstanceData.BoundObjectContainer as IValueContainer).Name + " - " + m_panelInstanceData.PanelType.Name;
                        case Core.Controls.CustomPanelBindingState.Disconnected:
                            return "<disconnected>";
                        default:
                            throw new NotImplementedException();
                    }

                    //if (m_panelInstanceData.BoundObjectContainer != null)
                    //{
                    //    return m_panelInstanceData.PanelType.Name + " - " + m_panelInstanceData.BoundObjectContainer.FullName;
                    //}
                    //else
                    //{
                    //    if (String.IsNullOrEmpty(m_panelInstanceData.TargetObjectReference))
                    //    {
                    //        return m_panelInstanceData.PanelType.TypeIdentification;
                    //    }
                    //    else
                    //    {
                    //        return m_panelInstanceData.PanelType.TypeIdentification + " - " + m_panelInstanceData.TargetObjectReference;
                    //    }
                    //}
                }
                else
                {
                    if (String.IsNullOrEmpty(m_panelInstanceData.TargetObjectReference))
                    {
                        return "Custom Panel " + m_panelInstanceData.TypeName;
                    }
                    else
                    {
                        return "Custom Panel " + m_panelInstanceData.TypeName + " - " + m_panelInstanceData.TargetObjectReference;
                    }
                }
            }
            else
            {
                return "Custom Panel - ???";
            }
        }

        void SetStateText()
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

        public UserControl ContentControl
        {
            get
            {
                return (m_contentControl != null) ? m_contentControl : m_indcatorControl;
            }
            private set
            {
                m_contentControl = value;
                this.NotifyPropertyChanged(nameof(ContentControl));
            }
        }

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
        //    System.Diagnostics.Debug.Assert(m_contentControl == null);
        //    System.Diagnostics.Debug.Assert(m_loadSpecification == null);
        //    System.Diagnostics.Debug.Assert(m_services != null);
        //    System.Diagnostics.Debug.Assert(specification.Length >= 1);

        //    m_loadSpecification = specification;
        //    m_objectName = (m_loadSpecification.Length > 1) ? m_loadSpecification[1] : null;

        //    var panelManager = m_services.Get<ICustomPanelManager>();
        //    m_panelType = panelManager.FindPanelType(m_loadSpecification[0]);
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
