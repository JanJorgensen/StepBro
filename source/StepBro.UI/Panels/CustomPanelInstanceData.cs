using StepBro.Core.Controls;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StepBro.UI.Panels
{
    /// <summary>
    /// Class that identifies the type and source of an intended custom panel instance. 
    /// </summary>
    public class CustomPanelInstanceData : IDisposable, INotifyPropertyChanged
    {
        private string m_typeName = null;
        private CustomPanelType m_type = null;
        private CustomPanelBindingState m_state = CustomPanelBindingState.NotBindable;
        private string m_targetObject = null;
        private IObjectContainer m_objectContainer = null;
        private IDynamicObjectManager m_objectManager = null;
        private UserControl m_panel = null;


        internal CustomPanelInstanceData(CustomPanelType type)
        {
            m_type = type;
            m_state = type.IsObjectPanel ? CustomPanelBindingState.NotBound : CustomPanelBindingState.NotBindable;
        }

        internal CustomPanelInstanceData(string type, string target)
        {
            m_typeName = type;
            m_targetObject = target;
            m_state = (String.IsNullOrEmpty(target)) ? CustomPanelBindingState.NotBindable : CustomPanelBindingState.NotBound;
        }

        internal CustomPanelInstanceData(CustomPanelType type, string target)
        {
            m_type = type;
            m_targetObject = target;
            m_state = CustomPanelBindingState.BoundWithoutObject;
        }

        internal CustomPanelInstanceData(CustomPanelType type, IObjectContainer container)
        {
            m_type = type;
            m_objectContainer = container;
            m_state = (container.Object != null) ? CustomPanelBindingState.Bound : CustomPanelBindingState.BoundWithoutObject;
        }

        public CustomPanelType PanelType { get { return m_type; } }

        public CustomPanelBindingState State { get { return m_state; } }

        public string TypeName { get { return m_typeName; } }

        public IObjectContainer BoundObjectContainer { get { return m_objectContainer; } }

        public string TargetObjectReference { get { return m_targetObject; } }

        public event EventHandler Disposed;

        public void Dispose()
        {
            DisconnectBinding();
        }

        public UserControl GetPanelControl()
        {
            if (m_panel == null)
            {
                m_panel = this.PanelType.CreatePanelView();
            }

            if (m_state == CustomPanelBindingState.Bound)
            {
                m_type.SetPanelObjectBinding(m_panel, m_objectContainer.Object);
            }

            return m_panel;
        }

        protected void DisconnectBinding()
        {
            this.DoDisconnectBinding();
        }

        public event EventHandler DisconnectBindingEvent;

        protected void DoDisconnectBinding()
        {
            m_type.SetPanelObjectBinding(m_panel, null);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void SetObjectReference(string fullname)
        {
            if (String.IsNullOrWhiteSpace(fullname)) throw new ArgumentNullException("Reference name is missing.");
            switch (m_state)
            {
                case CustomPanelBindingState.NotBindable:
                    throw new NotSupportedException("Panel does not support object binding.");
                case CustomPanelBindingState.Bound:
                case CustomPanelBindingState.BoundWithoutObject:
                    throw new NotSupportedException("Panel is already bound.");
                case CustomPanelBindingState.BindingFailed:
                    throw new NotSupportedException("Binding has already failed.");
                case CustomPanelBindingState.NotBound:
                    break;
            }
            this.SetState(CustomPanelBindingState.BoundWithoutObject);
            m_targetObject = fullname;
            if (m_objectManager == null)
            {
                m_objectManager = StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>();
            }
            m_objectManager.ObjectListChanged += this.ObjectManager_ObjectListChanged;
        }

        private void ObjectManager_ObjectListChanged(object sender, EventArgs e)
        {
            var container = StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>().TryFindObject(m_targetObject);

            if (m_objectContainer == null)
            {
                if (container != null)
                {
                    this.Bind(container);
                }
            }
            else
            {
                if (container == null || !Object.ReferenceEquals(container, m_objectContainer))
                {
                    this.Bind(null);
                }
            }
        }

        private void SetState(CustomPanelBindingState state)
        {
            if (state != m_state)
            {
                m_state = state;
                this.NotifyPropertyChanged(nameof(State));
            }
        }

        internal void Bind(IObjectContainer container)
        {
            if (!this.PanelType.IsObjectPanel) throw new NotSupportedException();
            if (m_objectContainer != null)
            {
                this.ReleaseObjectBindingDirect();
                m_objectContainer.Disposing -= this.ObjectContainer_Disposing;
                m_objectContainer.ObjectReplaced -= this.ObjectContainer_ObjectReplaced;
                m_objectContainer = null;
            }
            m_objectContainer = container;
            if (m_objectContainer != null)
            {
                if (m_objectContainer.Object != null)
                {
                    this.TryBindToObject();
                }
                else
                {
                    this.SetState(CustomPanelBindingState.BoundWithoutObject);
                }
                m_objectContainer.Disposing += this.ObjectContainer_Disposing;
                m_objectContainer.ObjectReplaced += this.ObjectContainer_ObjectReplaced;
            }
            else
            {
                this.SetState(CustomPanelBindingState.BoundWithoutObject);
            }
        }

        private void TryBindToObject()
        {
            if (m_objectContainer.Object != null)
            {
                if (this.PanelType.TryBind(m_objectContainer.Object))
                {
                    this.SetState(CustomPanelBindingState.Bound);
                }
                else
                {
                    this.SetState(CustomPanelBindingState.BindingFailed);
                }
                m_targetObject = m_objectContainer.FullName;
            }
            else
            {
                this.SetState(CustomPanelBindingState.BoundWithoutObject);
            }
        }

        internal void ReleaseObjectBinding()
        {
            this.ReleaseObjectBindingDirect();
            this.SetState(CustomPanelBindingState.BoundWithoutObject);
        }

        internal void ReleaseObjectBindingDirect()
        {
            if (m_objectContainer != null)
            {
                m_targetObject = null;
            }
        }

        private void ObjectContainer_Disposing(object sender, EventArgs e)
        {
            if (m_state == CustomPanelBindingState.Bound)
            {
                this.Bind(null);
                //m_objectContainer.Disposing -= this.ObjectContainer_Disposing;
                //m_objectContainer.ObjectReplaced -= this.ObjectContainer_ObjectReplaced;
                //this.DisconnectBinding();
                //this.SetState(CustomPanelBindingState.Disconnected);
                //m_objectContainer = null;
            }
        }

        private void ObjectContainer_ObjectReplaced(object sender, EventArgs e)
        {
            // Disconnect current object
            if (this.State == CustomPanelBindingState.Bound || this.State == CustomPanelBindingState.BoundWithoutObject)
            {

                this.TryBindToObject();
            }
        }
    }
}
