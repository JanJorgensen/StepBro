using StepBro.Core.Data;
using System;
using System.Windows.Forms;

namespace StepBro.Core.Controls
{
    public partial class ObjectPanel : UserControl
    {
        public enum BindingState { NotBindable, NotBound, BoundWithoutObject, BindingFailed, Bound, Disconnected }
        private IObjectContainer m_objectContainer = null;
        private BindingState m_state = BindingState.NotBindable;
        private string m_targetObject = null;
        private IDynamicObjectManager m_objectManager = null;

        public ObjectPanel()
        {
            this.InitializeComponent();
            m_state = this.IsBindable ? BindingState.NotBound : BindingState.NotBindable;
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

        public BindingState State { get { return m_state; } }

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
                case BindingState.NotBindable:
                    throw new NotSupportedException("Panel does not support object binding.");
                case BindingState.Bound:
                case BindingState.BoundWithoutObject:
                    throw new NotSupportedException("Panel is already bound.");
                case BindingState.BindingFailed:
                    throw new NotSupportedException("Binding has already failed.");
                case BindingState.NotBound:
                    break;
            }
            this.SetState(BindingState.BoundWithoutObject);
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

        private void SetState(BindingState state)
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
                this.SetState(BindingState.BoundWithoutObject);
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
                    this.SetState(BindingState.Bound);
                }
                else
                {
                    this.SetState(BindingState.BindingFailed);
                }
            }
            else
            {
                this.SetState(BindingState.BoundWithoutObject);
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
                this.SetState(BindingState.Disconnected);
            }
        }

        private void ObjectContainer_Disposing(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new System.Action(() =>
                {
                    if (m_state == BindingState.Bound)
                    {
                        m_objectContainer.Disposing -= this.ObjectContainer_Disposing;
                        m_objectContainer.ObjectReplaced -= this.ObjectContainer_ObjectReplaced;
                        this.DisconnectBinding();
                        this.SetState(BindingState.Disconnected);
                        m_objectContainer = null;
                    }
                }));
        }
        private void ObjectContainer_ObjectReplaced(object sender, EventArgs e)
        {
            this.BeginInvoke(new System.Action(() =>
            {
                // Disconnect current object
                this.DisconnectBinding();
                this.SetState(BindingState.BoundWithoutObject);

                this.TryBindToObject();
            }));
        }

        #endregion
    }
}
