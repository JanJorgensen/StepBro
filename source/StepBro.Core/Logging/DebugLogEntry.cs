using StepBro.Core.Data;
using System;

namespace StepBro.Core.Logging
{
    public abstract class DebugLogEntry
    {
        private const long MAX_COUNT = 1000000;
        private static DebugLogEntry m_first;
        private static DebugLogEntry m_last;
        private static readonly object m_sync = new object();
        private static long m_count = 0;

        static DebugLogEntry()
        {
            m_first = m_last = new DebugLogEntryString("DebugLog Created");
        }
        public static void Register(DebugLogEntry entry)
        {
            lock (m_sync)
            {
                m_count++;
                m_last.m_next = entry;
                m_last = entry;
                if (m_count > MAX_COUNT) m_first = m_first.m_next;
            }
        }

        public static DebugLogEntry First { get { return m_first; } }

        private DebugLogEntry m_next;
        protected DateTime m_timestamp;
        public DebugLogEntry()
        {
            m_timestamp = DateTime.UtcNow;
        }

        public DateTime Timestamp { get { return m_timestamp; } }
        public DebugLogEntry Next { get { return m_next; } }
        public abstract override string ToString();
    }

    public class DebugLogEntry<T> : DebugLogEntry
    {
        private T m_data;
        public DebugLogEntry(T data) : base() { m_data = data; }
        public override string ToString()
        {
            return m_data.ToString();
        }
    }

    public class DebugLogEntryString : DebugLogEntry
    {
        private readonly string m_message;
        public DebugLogEntryString(string message) : base() { m_message = message; }
        public override string ToString()
        {
            return m_message;
        }
    }

    public static class DebugLogUtils
    {
        private static object sync = new object();

        public static string DumpFilePath
        {
            get
            {
                return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\StepBro DebugLog.txt";
            }
        }

        public static void DumpToFile(DebugLogEntry start = null)
        {
            var first = (start == null) ? DebugLogEntry.First : start;
            var entry = first;
            lock(sync)
            {
                using (System.IO.FileStream dump = new System.IO.FileStream(
                    DumpFilePath,
                    System.IO.FileMode.Create, System.IO.FileAccess.Write,
                    System.IO.FileShare.None))
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(dump))
                    {
                        while (entry != null)
                        {
                            writer.WriteLine(entry.Timestamp.ToMinutesTimestamp(first.Timestamp) + "  " + entry.ToString());
                            entry = entry.Next;
                        }
                    }
                }
            }
        }
    }
}
