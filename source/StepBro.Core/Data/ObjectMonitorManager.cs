using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ObjectMonitorManager
    {
        public static readonly ObjectMonitorManager Global;

        private ObjectMonitor m_first = null;
        private ObjectMonitor m_last = null;
        private object m_sync = new object();
        private int m_listLength = 0;
        private long m_listChangeIndex = 0L;

        static ObjectMonitorManager()
        {
            Global = new ObjectMonitorManager();
        }

        public ObjectMonitor CreateObjectMonitor(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            lock (m_sync)
            {
                if (m_first == null)
                {
                    m_first = new ObjectMonitor(null, target);
                    m_last = m_first;
                }
                else
                {
                    m_last = new ObjectMonitor(m_last, target);
                }
                m_last.PostCreate(target);
                m_listLength++;
                m_listChangeIndex++;
                return m_last;
            }
        }

        public static T Register<T>(T target) where T : class
        {
            Global.CreateObjectMonitor(target);
            return target;
        }

        public ObjectMonitor CreateObjectMonitor<TMon>(object target) where TMon : ObjectMonitor, new()
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            lock (m_sync)
            {
                m_last = new TMon();
                if (m_first == null)
                {
                    m_last.Setup(null, target);
                    m_first = m_last;
                }
                else
                {
                    m_last.Setup(m_last, target);
                }
                m_last.PostCreate(target);
                m_listLength++;
                m_listChangeIndex++;
                return m_last;
            }
        }

        public void Remove(ObjectMonitor monitor)
        {
            lock (m_sync)
            {
                if (Object.ReferenceEquals(m_first, monitor))
                {
                    if (Object.ReferenceEquals(m_last, monitor))
                    {
                        m_first = null;
                        m_last = null;
                    }
                    else
                    {
                        m_first = m_first.m_next;
                    }
                }
                else if (Object.ReferenceEquals(m_first, monitor))
                {
                    m_last = m_last.m_previous;
                }
                monitor.Dispose();
                m_listLength--;
                m_listChangeIndex++;
            }
        }

        public long ChangeIndex { get { lock (m_sync) { return m_listChangeIndex; } } }

        public ObjectMonitor[] CreateSnapshotListIfChanged(ref long lastKnownChangeIndex)
        {
            lock (m_sync)
            {
                if (m_listChangeIndex != lastKnownChangeIndex)
                {
                    lastKnownChangeIndex = m_listChangeIndex;
                    var array = new ObjectMonitor[m_listLength];
                    var next = m_first;
                    for (int i = 0; i < m_listLength; i++)
                    {
                        array[i] = next;
                        next = next.m_next;
                    }
                    return array;
                }
                else return null;
            }
        }
    }
}
