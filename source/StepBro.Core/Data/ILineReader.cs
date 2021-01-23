using System;

namespace StepBro.Core.Data
{
    public interface ILineReader
    {
        bool LinesHaveTimestamp { get; }
        string Text { get; }
        DateTime Timestamp { get; }
        bool HasMore { get; }
        bool Find(string text, bool flushIfNotFound = false);
        bool Await(string text, TimeSpan timeout);
        bool Next();
        void Flush();
    }


    public class LogLineDataReader : ILineReader
    {
        private object m_sync;
        private LogLineData m_entry = null;

        public LogLineDataReader(LogLineData first, object sync)
        {
            m_sync = sync;
            m_entry = first;
        }

        public void NotifyNew(LogLineData entry)
        {
            lock (m_sync)
            {
                if (m_entry == null)
                {
                    m_entry = entry;
                }
            }
        }

        public bool LinesHaveTimestamp { get { return true; } }

        public string Text
        {
            get
            {
                LogLineData entry = m_entry;
                return entry?.LineText;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                LogLineData entry = m_entry;
                return (entry != null) ? entry.Timestamp : DateTime.MinValue;
            }
        }

        public bool HasMore
        {
            get
            {
                LogLineData entry = m_entry;
                return (entry != null) ? entry.Next != null : false;
            }
        }

        public bool Find(string text, bool flushIfNotFound = false)
        {
            var t = text.ToLower();
            var entry = m_entry;
            var last = entry;
            while (entry != null)
            {
                last = entry;
                if (entry.LineText.ToLower().Contains(t))
                {
                    m_entry = entry;
                    return true;
                }
                lock (m_sync)
                {
                    entry = entry.Next;
                }
            }
            if (flushIfNotFound && last != null)
            {
                lock (m_sync)
                {
                    m_entry = last.Next;    // The one after the last we inspected (might be none/null).
                }
            }
            return false;
        }

        public bool Await(string text, TimeSpan timeout)
        {
            DateTime entry = DateTime.Now;
            DateTime to = (timeout == TimeSpan.MaxValue) ? DateTime.MaxValue : entry + timeout;
            bool sleep = false;
            do
            {
                if (sleep) System.Threading.Thread.Sleep(5);
                if (Find(text, true))
                {
                    return true;
                }
                sleep = true;
            } while (DateTime.Now.TimeTill(to) > TimeSpan.Zero);
            return false;
        }

        public bool Next()
        {
            lock (m_sync)
            {
                if (m_entry != null)
                {
                    m_entry = m_entry.Next;
                    return true;
                }
                else return false;
            }
        }

        public void Flush()
        {
            lock (m_sync)
            {
                m_entry = null;
            }
        }
    }
}
