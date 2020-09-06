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

        public virtual bool IsBindable { get { return false; } }

        public object BoundObject { get { return m_objectContainer?.Object; } }

        protected virtual bool TryBind(object @object)
        {
            if (this.IsBindable) throw new NotImplementedException();
            return false;
        }

        protected virtual void DisconnectBinding()
        {
        }

        #region Internal functionality

        public ObjectPanel()
        {
            this.InitializeComponent();
            m_state = this.IsBindable ? BindingState.NotBound : BindingState.NotBindable;
        }

        public void SetObjectReference(string fullname)
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
            StepBro.Core.Main.ServiceManager.Get<IDynamicObjectManager>().ObjectListChanged += this.ObjectManager_ObjectListChanged;

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

        public BindingState State { get { return m_state; } }

        public event EventHandler BindingStateChanged;

        public IObjectContainer BoundObjectContainer { get { return m_objectContainer; } }

        public string TargetObjectReference { get { return m_targetObject; } }

        public void Bind(IObjectContainer container)
        {
            if (!this.IsBindable) throw new NotSupportedException();
            m_objectContainer = container;
            if (m_objectContainer.Object != null)
            {
                this.TryBind();
            }
            else
            {
                this.SetState(BindingState.BoundWithoutObject);
            }
            m_targetObject = m_objectContainer.FullName;
            m_objectContainer.Disposing += this.ObjectContainer_Disposing;
            m_objectContainer.ObjectReplaced += this.ObjectContainer_ObjectReplaced;
        }

        private void TryBind()
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
                throw new NotImplementedException();
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
                if (m_state == BindingState.BoundWithoutObject)
                {
                    this.TryBind();
                }
            }));
        }

        #endregion
    }
}
