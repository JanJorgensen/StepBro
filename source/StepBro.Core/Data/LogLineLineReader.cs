using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class LogLineLineReader : ILineReader, IEnumerable<ILineReaderEntry>
    {
        private class Enumerator : IEnumerator<ILineReaderEntry>
        {
            private LogLineLineReader m_parent;
            private LogLineData m_first;
            private LogLineData m_current;
            private bool m_reset = true;

            public Enumerator(LogLineLineReader reader)
            {
                lock (reader.m_sync)
                {
                    m_parent = reader;
                    m_first = m_parent.m_entry;
                    m_current = null;
                }
            }

            public object Current
            {
                get { return m_current; }
            }

            ILineReaderEntry IEnumerator<ILineReaderEntry>.Current
            {
                get
                {
                    return m_current;
                }
            }

            public bool MoveNext()
            {
                if (m_current != null && m_current.Next != null)
                {
                    m_current = m_current.Next;
                    return true;
                }
                else
                {
                    if (m_reset && m_first != null)
                    {
                        m_current = m_first;
                        m_reset = false;
                        return true;
                    }
                    else
                    {
                        m_current = null;
                        return false;
                    }
                }
            }

            public void Reset()
            {
                m_current = null;
                m_reset = true;
            }

            public void Dispose()
            {
                m_parent = null;
                m_first = null;
                m_current = null;
            }
        }

        private object m_sync;
        private bool m_firstEntryIsNew = true;
        private LogLineData m_entry = null;
        private DateTime m_latestTimestamp = DateTime.MinValue;

        public event EventHandler LinesAdded;

        public LogLineLineReader(INameable source, LogLineData first, object sync)
        {
            this.Source = source;
            m_sync = sync;
            m_entry = first;
            m_firstEntryIsNew = true;
        }

        public void NotifyNew(LogLineData entry)
        {
            lock (m_sync)
            {
                if (m_entry == null)
                {
                    m_entry = entry;
                    m_firstEntryIsNew = true;
                    Monitor.Pulse(m_sync);  // In case someone is waiting.
                }
                this.LinesAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool LinesHaveTimestamp { get { return true; } }

        public bool HasMore
        {
            get
            {
                LogLineData entry = m_entry;
                return (entry != null) ? entry.Next != null : false;
            }
        }

        public object Sync { get { return m_sync; } }

        public DateTime LatestTimeStamp { get { return m_latestTimestamp; } }

        public ILineReaderEntry Current { get { return m_entry; } }

        public INameable Source { get; private set; }

        public bool Next()
        {
            lock (m_sync)
            {
                if (m_entry != null)
                {
                    m_latestTimestamp = m_entry.Timestamp;
                    m_entry = m_entry.Next;
                    return true;
                }
                else return false;
            }
        }

        public bool NextUnlessNewEntry()
        {
            lock (m_sync)
            {
                if (!m_firstEntryIsNew && m_entry != null)
                {
                    m_latestTimestamp = m_entry.Timestamp;
                    m_entry = m_entry.Next;
                    return true;
                }
                else
                {
                    m_firstEntryIsNew = false;
                }
                return false;
            }
        }

        public void Flush(ILineReaderEntry stopAt = null)
        {
            if (stopAt != null)
            {
                lock (m_sync)
                {
                    m_entry = (LogLineData)stopAt;
                    m_latestTimestamp = m_entry.Timestamp;
                }
            }
            else
            {
                lock (m_sync)
                {
                    // This timestamp can be significantly off with real-time
                    // but as we get rid of every other entry, there is no better
                    // choice for latest timestamp.
                    if (m_entry != null)
                    {
                        m_latestTimestamp = m_entry.Timestamp;
                    }
                    m_entry = null;
                }
            }
        }

        public IEnumerable<ILineReaderEntry> Peak()
        {
            return this;
        }

        public IEnumerator<ILineReaderEntry> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void DebugDump()
        {
            var entry = m_entry;
            if (entry == null)
            {
                System.Diagnostics.Debug.WriteLine("LogLine: <none>");
            }
            while (entry != null)
            {
                System.Diagnostics.Debug.WriteLine("LogLine: " + entry.Text);
                entry = entry.Next;
            }
        }
    }
}
