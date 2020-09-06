using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ObjectMonitor
    {
        public enum State { TargetActive, TargetDisposing, TargetDisposed, TargetVoid, MonitorDisposed }

        internal ObjectMonitor m_previous;
        internal ObjectMonitor m_next = null;
        private WeakReference m_reference;
        private State m_state = State.TargetActive;
        private string m_targetType = "";
        private string m_lastTargetState = "";

        internal ObjectMonitor(ObjectMonitor previous, object target)
        {
            this.Setup(previous, target);
        }

        protected ObjectMonitor()
        {
            m_previous = null;
            m_reference = null;
        }

        internal void Setup(ObjectMonitor previous, object target)
        {
            if (previous != null) previous.m_next = this;
            m_previous = previous;
            m_reference = new WeakReference(target);
            if (target is IAvailability)
            {
                (target as IAvailability).Disposing += ObjectMonitor_Disposing;
                (target as IAvailability).Disposed += ObjectMonitor_Disposed;
            }
        }

        public event EventHandler StateChanged;
        public event EventHandler LastTargetStateChanged;

        private void ObjectMonitor_Disposing(object sender, EventArgs e)
        {
            m_lastTargetState = "<disposing>";
            m_state = State.TargetDisposing;
            this.StateChanged?.Invoke(this, EventArgs.Empty);
            this.LastTargetStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ObjectMonitor_Disposed(object sender, EventArgs e)
        {
            m_lastTargetState = "<disposed>";
            m_state = State.TargetDisposed;
            this.StateChanged?.Invoke(this, EventArgs.Empty);
            this.LastTargetStateChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void PostCreate(object target)
        {
            m_targetType = this.GetTypeString(target);
            m_lastTargetState = this.GetStateString(target);
        }

        internal void Dispose()
        {
            this.OnMonitorDispose();
            m_state = State.MonitorDisposed;
            m_reference = null;
            m_next.m_previous = m_previous;
            m_previous.m_next = m_next;
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMonitorDispose()
        {
        }

        public void UpdateState()
        {
            if (m_state != State.TargetVoid && m_state != State.MonitorDisposed)
            {
                var o = (m_reference.IsAlive) ? m_reference.Target : null;
                if (o == null)
                {
                    m_lastTargetState = "<void>";
                    m_state = State.TargetVoid;
                    this.StateChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (m_state == State.TargetActive)
                {
                    var state = this.GetStateString(o);
                    if (!Equals(state, m_lastTargetState))
                    {
                        m_lastTargetState = state;
                        this.LastTargetStateChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public State CurrentState { get { return m_state; } }

        public T TryGetTargetObject<T>() where T : class
        {
            if (m_reference.IsAlive)
            {
                object target = m_reference.Target;
                if (target != null)
                {
                    return target as T;
                }
            }
            return null;
        }

        public string TypeString
        {
            get
            {
                return m_targetType;
            }
        }

        protected virtual string GetTypeString(object target)
        {
            return target.GetType().Name;
        }

        public string LastTargetState
        {
            get
            {
                return m_lastTargetState;
            }
        }

        protected virtual string GetStateString(object target)
        {
            return target.ToString();
        }
    }
}
