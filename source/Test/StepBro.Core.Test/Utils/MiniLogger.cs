using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBroCoreTest.Utils
{
    public class MiniLogger
    {
        public class Entry
        {
            private Entry m_next = null;
            private DateTime m_timestamp;
            private string m_text;

            public Entry(string text, Entry previous)
            {
                m_timestamp = DateTime.UtcNow;
                m_text = text;
                if (previous != null)
                {
                    previous.m_next = this;
                }
            }

            public DateTime Timestamp { get { return m_timestamp; } }
            public string Text { get { return m_text; } }
            public Entry Next { get { return m_next; } }
        }

        public static readonly MiniLogger Instance;
        static MiniLogger()
        {
            Instance = new MiniLogger();
        }

        private object m_sync = new object();
        private Entry m_first = null;
        private Entry m_last = null;

        public Entry First { get { return m_first; } }

        public void Add(string text)
        {
            lock (m_sync)
            {
                var entry = new Entry(text, m_last);
                if (m_first == null)
                {
                    m_first = entry;
                }
                m_last = entry;
            }
        }

        public void Clear()
        {
            lock (m_sync)
            {
                m_first = null;
                m_last = null;
            }
        }

        public void DumpToDebugTrace()
        {
            System.Diagnostics.Debug.WriteLine("MiniLogger.DumpToDebugTrace");
            if (m_first != null)
            {
                DateTime start = m_first.Timestamp;
                var entry = m_first;
                while (entry != null)
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("[{0}] {1}", entry.Timestamp, entry.Text));
                    entry = entry.Next;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("   - EMPTY");
            }
        }
    }
}
