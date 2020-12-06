using StepBro.Core.Data;
using System;
using System.Windows.Forms;

namespace StepBro.Core.Controls.WinForms
{
    public partial class ObjectPanel : UserControl
    {
        private IObjectContainer m_objectContainer = null;
        private ObjectPanelBindingState m_state = ObjectPanelBindingState.NotBindable;
        private string m_targetObject = null;
        private IDynamicObjectManager m_objectManager = null;

        public ObjectPanel()
        {
            this.InitializeComponent();
            m_state = this.IsBindable ? ObjectPanelBindingState.NotBound : ObjectPanelBindingState.NotBindable;
        }

        public virtual bool IsBindable { get { return false; } }

        public IObjectContainer BoundObject { get { return m_objectContainer; } }

        protected virtual bool TryBind(object @object)
        {
            if (this.IsBindable) throw new NotImplementedException($"Panel should override this method ({nameof(this.TryBind)}).");
            else throw new NotSupportedException("This panel is not bindable (it is a static panel).");
        }

        /// <summary>
        /// This function is called when the current bound object is not used anymore. Inheritors should override this method to disconnect the current object.
        /// </summary>
        /// <remarks>The panel can still be bound to the same variable/container, but not the current object.</remarks>
        protected virtual void DisconnectBinding()
        {
        }

        public ObjectPanelBindingState State { get { return m_state; } }

        public event EventHandler BindingStateChanged;

        public IObjectContainer BoundObjectContainer { get { return m_objectContainer; } }

        public string TargetObjectReference { get { return m_targetObject; } }

        #region Internal functionality

        internal ObjectPanelInfo PanelType { get; set; } = null;

        internal void SetObjectReference(string fullname)
        {
            if (String.IsNullOrWhiteSpace(fullname)) throw new ArgumentNullException("Reference name is missing.");
            switch (m_state)
            {
                case ObjectPanelBindingState.NotBindable:
                    throw new NotSupportedException("Panel does not support object binding.");
                case ObjectPanelBindingState.Bound:
                case ObjectPanelBindingState.BoundWithoutObject:
                    throw new NotSupportedException("Panel is already bound.");
                case ObjectPanelBindingState.BindingFailed:
                    throw new NotSupportedException("Binding has already failed.");
                case ObjectPanelBindingState.NotBound:
                    break;
            }
            this.SetState(ObjectPanelBindingState.BoundWithoutObject);
            m_targetObject = fullname;
            if (m_objectManager == null)
            {
                m_objectManager = StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>();
            }
            m_objectManager.ObjectListChanged += this.ObjectManager_ObjectListChanged;
        }

        private void ObjectManager_ObjectListChanged(object sender, EventArgs e)
        {
            if (m_objectContainer == null)
            {
                this.BeginInvoke(new System.Action(() =>
                {
                    var container = StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>().TryFindObject(m_targetObject);

                    if (container != null)
                    {
                        StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>().ObjectListChanged -= this.ObjectManager_ObjectListChanged;
                        this.Bind(container);
                    }
                }));
            }
        }

        private void SetState(ObjectPanelBindingState state)
        {
            if (state != m_state)
            {
                m_state = state;
                this.BindingStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        internal void Bind(IObjectContainer container)
        {
            if (!this.IsBindable) throw new NotSupportedException();
            m_objectContainer = container;
            if (m_objectContainer.Object != null)
            {
                this.TryBindToObject();
            }
            else
            {
                this.SetState(ObjectPanelBindingState.BoundWithoutObject);
            }
            m_targetObject = m_objectContainer.FullName;
            m_objectContainer.Disposing += this.ObjectContainer_Disposing;
            m_objectContainer.ObjectReplaced += this.ObjectContainer_ObjectReplaced;
        }

        private void TryBindToObject()
        {
            if (m_objectContainer.Object != null)
            {
                if (this.TryBind(m_objectContainer.Object))
                {
                    this.SetState(ObjectPanelBindingState.Bound);
                }
                else
                {
                    this.SetState(ObjectPanelBindingState.BindingFailed);
                }
            }
            else
            {
                this.SetState(ObjectPanelBindingState.BoundWithoutObject);
            }
        }

        internal void ReleaseBinding()
        {
            if (m_objectContainer != null)
            {
                this.DisconnectBinding();
                m_objectContainer.Disposing -= this.ObjectContainer_Disposing;
                m_objectContainer.ObjectReplaced -= this.ObjectContainer_ObjectReplaced;
                m_targetObject = null;
                m_objectContainer = null;
                this.SetState(ObjectPanelBindingState.Disconnected);
            }
        }

        private void ObjectContainer_Disposing(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new System.Action(() =>
                {
                    if (m_state == ObjectPanelBindingState.Bound)
                    {
                        m_objectContainer.Disposing -= this.ObjectContainer_Disposing;
                        m_objectContainer.ObjectReplaced -= this.ObjectContainer_ObjectReplaced;
                        this.DisconnectBinding();
                        this.SetState(ObjectPanelBindingState.Disconnected);
                        m_objectContainer = null;
                    }
                }));
        }
        private void ObjectContainer_ObjectReplaced(object sender, EventArgs e)
        {
            this.BeginInvoke(new System.Action(() =>
            {
                // Disconnect current object
                if (this.State == ObjectPanelBindingState.Bound || this.State == ObjectPanelBindingState.BoundWithoutObject)
                {
                    this.DisconnectBinding();
                }
                this.SetState(ObjectPanelBindingState.BoundWithoutObject);

                this.TryBindToObject();
            }));
        }

        #endregion
    }
}
