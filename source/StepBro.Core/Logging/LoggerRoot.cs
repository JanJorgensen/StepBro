using System;

namespace StepBro.Core.Logging
{
    public class LoggerRoot : IDisposable
    {
        private readonly object m_sync = new object();
        private readonly string m_outputfile;
        private readonly bool m_directLogToFile;
        private readonly LogEntry m_oldest = null;
        private LogEntry m_newest = null;
        internal IProtectedLogger m_rootLogger;
        private Predicate<LogEntry> m_breaker = null;
        private ILogSink m_sink = null;

        public LoggerRoot(string outputFile, bool directLogToFile, string location, string starttext)
        {
            m_outputfile = outputFile;
            m_directLogToFile = directLogToFile && !String.IsNullOrEmpty(outputFile);

            m_oldest = new LogEntry(
                UniqueInteger.Get(),
                DateTime.UtcNow,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                location,
                starttext);
            m_newest = m_oldest;
            m_rootLogger = new Logger(this, m_newest).GetProtectedLogger();     // Cannot be disposed by accident, then.

            if (m_sink != null)
            {
                m_sink.Add(m_newest);
            }
        }

        internal LogEntry Log(LogEntry parent, LogEntry.Type type, uint id, DateTime timestamp, int thread, string location, string text)
        {
            lock (m_sync)
            {
                m_newest = new LogEntry(m_newest, parent, type, id, timestamp, 0, location, text);
                if (m_breaker != null && m_breaker(m_newest))
                {
                    this.BreakHere();
                }
                if (m_sink != null)
                {
                    m_sink.Add(m_newest);
                }
                return m_newest;
            }
        }

        public void SetBreaker(Predicate<LogEntry> breaker)
        {
            m_breaker = breaker;
        }

        private void BreakHere()
        {
            System.Diagnostics.Debug.Assert(false, "Break here!");
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILogger RootLogger { get { return m_rootLogger; } }

        public bool IsDebugging { get; set; } = false;

        public LogEntry GetOldestEntry()
        {
            return m_oldest;
        }

        public LogEntry GetNewestEntry()
        {
            return m_newest;
        }

        public void SetSink(ILogSink sink)
        {
            if (sink == null) throw new ArgumentNullException("sink");
            lock (m_sync)
            {
                if (m_sink != null) throw new NotSupportedException("Only one sink can be registered.");
                m_sink = sink;
            }
        }

        public void RemoveSink(ILogSink sink)
        {
            lock (m_sync)
            {
                if (sink != null && !Object.ReferenceEquals(sink, m_sink)) throw new ArgumentException("Specified sink is not registered.");
                if (m_sink != null)
                {
                    m_sink.Stop();
                    m_sink = null;
                }
            }
        }

        public void DebugDump()
        {
            var entry = m_oldest;
            while (entry != null)
            {
                var s = String.Format("{0} - {1} - {2} - {3}",
                    entry.IndentLevel,
                    entry.EntryType,
                    String.IsNullOrEmpty(entry.Location) ? "<empty>" : entry.Location,
                    String.IsNullOrEmpty(entry.Text) ? "<empty>" : entry.Text);
                System.Diagnostics.Debug.WriteLine(s);
                entry = entry.Next;
            }
        }

        internal static LoggerRoot Root(ILogger logger)
        {
            if (logger is Logger)
            {
                return ((Logger)logger).Root;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
