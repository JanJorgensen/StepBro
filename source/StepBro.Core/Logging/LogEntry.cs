using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public class LogEntry : ILogEntry
    {
        public enum Type : UInt32
        {
            /// <summary>
            /// Normal information about a unning task in the current scope.
            /// </summary>
            Normal = 0x00,
            /// <summary>
            /// Entering a new scope.
            /// </summary>
            Pre,
            /// <summary>
            /// Entering a new high level scope.
            /// </summary>
            PreHighLevel,
            /// <summary>
            /// Exit of the current scope.
            /// </summary>
            Post,
            /// <summary>
            /// Details from the parallell task just entered.
            /// </summary>
            TaskEntry,
            /// <summary>
            /// Information of a more detailed nature, that typically are not necessary for documenting a test execution.
            /// </summary>
            Detail,
            /// <summary>
            /// Information logged on a separate thread and not synchronized with the general execution thred.
            /// </summary>
            Async,
            /// <summary>
            /// Communication out of (sent from) the automation system.
            /// </summary>
            CommunicationOut,
            /// <summary>
            /// Communication in to (received by) the automation system.
            /// </summary>
            CommunicationIn, 
            Error,
            Failure,
            UserAction,
            System,
            /// <summary>
            /// A special kind of information, where only the source knows how to show/decode/understand the data.
            /// </summary>
            Special = 0x100
        }

        private LogEntry m_next = null;
        private LogEntry m_parent;
        private Type m_type;
        private ulong m_id;
        private DateTime m_timestamp;
        private int m_threadId;
        private string m_location;
        private string m_text;

        internal LogEntry(ulong id, DateTime timestamp, int thread, string location, string text)
        {
            m_parent = null;
            m_type = Type.Pre;
            m_id = id;
            m_timestamp = timestamp;
            m_threadId = thread;
            m_location = location;
            m_text = text;
        }

        internal LogEntry(LogEntry previous, LogEntry parent, Type type, ulong id, DateTime timestamp, int thread, string location, string text)
        {
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

        public LogEntry Next { get { return m_next; } }

        public LogEntry Parent { get { return m_parent; } }

        public Type EntryType { get { return m_type; } }

        public bool HasId { get { return true; } }

        public ulong Id { get { return m_id; } }

        public DateTime Timestamp { get { return m_timestamp; } }

        public int ThreadId { get { return m_threadId; } }

        public string Location { get { return m_location; } }

        public string Text { get { return m_text; } }

        #region Persisting 

        public string ToPersistanceString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_id.ToString());
            sb.Append('\t');
            sb.Append((m_parent != null) ? m_parent.m_id.ToString() : "-");
            sb.Append('\t');
            sb.Append(m_threadId.ToString());
            sb.Append('\t');
            sb.Append(m_timestamp.Ticks.ToString());
            sb.Append('\t');
            sb.Append(m_type.ToString());
            if (!String.IsNullOrEmpty(m_location))
            {
                sb.Append("\t\"");
                sb.Append(m_location.EscapeString());
                sb.Append('\"');
            }
            else
            {
                sb.Append("\t-");
            }
            if (!String.IsNullOrEmpty(m_text))
            {
                sb.Append("\t\"");
                sb.Append(m_text.EscapeString());
                sb.Append("\"");
            }
            else
            {
                sb.Append("\t-");
            }

            return sb.ToString();
        }

        #endregion
    }
}
