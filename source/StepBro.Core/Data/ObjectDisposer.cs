using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ObjectDisposer<T> : IDisposable where T : class
    {
        private T m_target;
        Action m_disposer;

        public ObjectDisposer(T target, Action disposer)
        {
            m_target = target;
            m_disposer = disposer;
        }

        public T Target { get { return m_target; } }

        public void Dispose()
        {
            m_disposer();
            m_disposer = null;
            m_target = null;
        }
    }
}
