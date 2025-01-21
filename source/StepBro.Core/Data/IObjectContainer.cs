using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Interface to a named container for a dynamic object.
    /// </summary>
    public interface IObjectContainer : IAvailability
    {
        /// <summary>
        /// The fully qualified name of the container/object.
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// Notifies when the container has been assigned another object.
        /// </summary>
        event EventHandler ObjectReplaced;
        /// <summary>
        /// Gets the container object reference.
        /// </summary>
        object Object { get; }
    }

    public class SimpleObjectContainer : IObjectContainer
    {
        private string m_name;
        private object m_object;
        private bool m_disposed = false;

        public SimpleObjectContainer(string name, object o)
        {
            m_name = name;
            m_object = o;
        }

        public string FullName { get { return m_name; } }

        public object Object { get { return m_object; } }

        public bool IsStillValid { get { return m_disposed == false; } }

        public event EventHandler ObjectReplaced;
        public event EventHandler Disposed;
        public event EventHandler Disposing;

        public void Dispose()
        {
            if (!m_disposed)
            {
                this.Disposing?.Invoke(this, EventArgs.Empty);
                if (m_object is IDisposable)
                {
                    ((IDisposable)m_object).Dispose();
                }
                m_disposed = true;
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
