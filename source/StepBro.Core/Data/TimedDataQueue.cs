using System;
using System.Threading;

namespace StepBro.Core.Data
{
    public class TimedDataQueue<T> where T : ITimedAction
    {
        private class Entry
        {
            public T Data = default(T);
            public Entry Next = null;
        }

        private Entry m_first = null;
        private readonly object m_sync = new object();
        private readonly bool m_isWaiting = false;

        public TimedDataQueue()
        {

        }

        public bool IsEmpty { get { lock (m_sync) { return m_first == null; } } }

        public void Add(T action)
        {
            this.InsertEntryInQueue(new Entry() { Data = action });
        }

        private void InsertEntryInQueue(Entry entry)
        {
            lock (m_sync)
            {
                var t = entry.Data.NextActionTime;
                if (m_first != null)
                {
                    if (t < m_first.Data.NextActionTime)
                    {
                        entry.Next = m_first;
                        m_first = entry;
                        if (m_isWaiting)
                        {
                            Monitor.Pulse(m_sync);
                        }
                        return;
                    }
                    var e = m_first;
                    while (e.Next != null && e.Next.Data.NextActionTime <= t) e = e.Next;
                    entry.Next = e.Next;
                    e.Next = entry;
                }
                else
                {
                    m_first = entry;
                    entry.Next = null;
                    if (m_isWaiting)
                    {
                        Monitor.Pulse(m_sync);
                    }
                }
            }
        }

        public DateTime NextActionTime()
        {
            if (m_first != null)
            {
                return m_first.Data.NextActionTime;
            }
            else return DateTime.MaxValue;
        }

        public T AwaitNext(TimeSpan maxTime)
        {
            lock (m_sync)
            {
                if (m_first != null)
                {
                    if (m_first.Data.NextActionTime <= DateTime.Now)
                    {
                        return this.HandleDueEntry();
                    }
                    TimeSpan timeout = m_first.Data.NextActionTime - DateTime.Now;
                    if (maxTime < timeout) timeout = maxTime;
                    if (Monitor.Wait(m_sync, timeout))
                    {
                        return this.HandleDueEntry();
                    }
                }
                else
                {
                    if (Monitor.Wait(m_sync, maxTime))
                    {
                        return this.HandleDueEntry();
                    }
                }
            }
            return default(T);
        }

        private T HandleDueEntry()
        {
            if (m_first.Data.NextActionTime <= DateTime.Now)
            {
                var e = m_first;
                m_first = m_first.Next;
                if (e.Data.IsActive)
                {
                    if (e.Data.UpdateWhenHandling(DateTime.Now))
                    {
                        this.InsertEntryInQueue(e);
                    }
                    return e.Data;
                }
            }
            return default(T);
        }

        public void DeactivateAll()
        {
            lock(m_sync)
            {
                var e = m_first;
                while (e != null)
                {
                    e.Data.Deactivate();
                    e = e.Next;
                }
            }
        }
    }
}
