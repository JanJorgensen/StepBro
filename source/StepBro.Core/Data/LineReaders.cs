using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class StringListLineReader : ILineReader
    {
        private class EntryWrapper : ILineReaderEntry
        {
            public EntryWrapper(int index, string text) { this.Index = index; this.Text = text; }
            public int Index { get; private set; }
            public string Text { get; private set; }
            public DateTime Timestamp => throw new NotImplementedException();
        }

        private readonly object m_sync = new object();
        private readonly List<string> m_list;
        private int m_index;
        private EntryWrapper m_current;
        private bool m_first = true;

        public StringListLineReader(List<string> list, INameable source = null)
        {
            m_list = list;
            m_index = (m_list.Count > 0) ? 0 : -1;
            m_first = true;
            this.Source = source;
        }


        public object Sync { get { return m_sync; } }

        public ILineReaderEntry Current
        {
            get
            {
                if (m_current == null)
                {
                    if (m_index < m_list.Count && m_list.Count > 0) m_current = new EntryWrapper(m_index, m_list[m_index]); ;
                }
                return m_current;
            }
        }
        public bool LinesHaveTimestamp { get { return false; } }

        public bool HasMore { get { return (m_index < (m_list.Count - 1)); } }

        public INameable Source { get; private set; }

        public event EventHandler LinesAdded;

        public void Flush(ILineReaderEntry stopAt = null)
        {
            if (stopAt != null)
            {
                m_index = (stopAt as EntryWrapper).Index;
            }
            else
            {
                m_index = m_list.Count;
            }
            m_current = null;
        }

        public bool Next()
        {
            m_current = null;
            if (m_index < (m_list.Count - 1))
            {
                m_index++;
                return true;
            }
            else if (m_index < (m_list.Count))
            {
                m_index++;  // Skip past the last entry
            }
            return false;
        }

        public bool NextUnlessNewEntry()
        {
            if (!m_first)
                return Next(); // Next works the same as NextUnlessNewEntry for this line reader - Except for the very first
            else
                m_first = false;
            return false;
        }

        public IEnumerable<ILineReaderEntry> Peak()
        {
            if (m_index < m_list.Count)
            {
                for (int i = m_index; i < m_list.Count; i++)
                {
                    yield return new EntryWrapper(i, m_list[i]);
                }
            }
            else yield break;
        }

        public void DebugDump()
        {
            if (m_index < m_list.Count)
            {
                for (int i = m_index; i < m_list.Count; i++)
                {
                    System.Diagnostics.Debug.WriteLine("LogLine: " + m_list[i]);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("LogLine: <none>");
            }
        }
    }
}
