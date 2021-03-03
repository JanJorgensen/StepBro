using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public class LogEntry
    {
        public enum Type { Normal, Pre, Post, TaskEntry, Detail, Async, Error, UserAction, System }

        //private LogEntry m_previous;
        private LogEntry m_next = null;
        private LogEntry m_parent;
        private Type m_type;
        private uint m_id;
        private DateTime m_timestamp;
        private int m_threadId;
        private string m_location;
        private string m_text;

        internal LogEntry(uint id, DateTime timestamp, int thread, string location, string text)
        {
            //m_previous = null;
            m_parent = null;
            m_type = Type.Pre;
            m_id = id;
            m_timestamp = timestamp;
            m_threadId = thread;
            m_location = location;
            m_text = text;
        }

        internal LogEntry(LogEntry previous, LogEntry parent, Type type, uint id, DateTime timestamp, int thread, string location, string text)
        {
            //m_previous = previous;
            previous.m_next = this;
            m_parent = parent;
            m_type = type;
            m_id = id;
            m_timestamp = timestamp;
            m_threadId = thread;
            m_location = location;
            m_text = text;
        }

        public override string ToString()
        {
            return String.Format("Entry - '{0}' ({1}) {2} {3}", m_type, m_id, m_location, m_text);
        }

        public int IndentLevel { get { return (m_parent != null) ? m_parent.IndentLevel + 1 : 0; } }

        //public LogEntry Previous
        //{
        //    get
        //    {
        //        return m_previous;
        //    }
        //}

        public LogEntry Next
        {
            get
            {
                return m_next;
            }
        }

        public LogEntry Parent
        {
            get
            {
                return m_parent;
            }
        }

        public Type EntryType
        {
            get
            {
                return m_type;
            }
        }

        public uint Id
        {
            get
            {
                return m_id;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return m_timestamp;
            }
        }

        public int ThreadId
        {
            get
            {
                return m_threadId;
            }
        }

        public string Location
        {
            get
            {
                return m_location;
            }
        }

        public string Text
        {
            get
            {
                return m_text;
            }
        }
    }
}
