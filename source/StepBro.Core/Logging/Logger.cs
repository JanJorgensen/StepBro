using System;

namespace StepBro.Core.Logging
{
    public class Logger : /*LogStorage<LogEntry>, */IDisposable
    {
        private readonly object m_sync = new object();
        private long m_firstIndex = 0;      // Will only change when entries are disposed.
        private long m_totalCount = 0;
        private readonly string m_outputfile;
        private readonly bool m_directLogToFile;
        private readonly LogEntry m_oldest = null;
        private LogEntry m_newest = null;
        internal IProtectedLogger m_rootScope;
        private Predicate<LogEntry> m_breaker = null;
        private IService m_rootScopeService = null;

        public Logger(string outputFile, bool directLogToFile, string location, string starttext)
        {
            m_outputfile = outputFile;
            m_directLogToFile = directLogToFile && !String.IsNullOrEmpty(outputFile);

            m_oldest = new LogEntry(
                UniqueInteger.Get(),
                DateTime.Now,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                location,
                starttext);
            m_totalCount++;
            m_newest = m_oldest;
            m_rootScope = new LoggerScope(this, m_newest, out m_rootScopeService);
        }

        public IService RootScopeService { get { return m_rootScopeService; } }

        internal LogEntry Log(LogEntry parent, LogEntry.Type type, DateTime timestamp, int thread, string location, string text)
        {
            lock (m_sync)
            {
                m_newest = new LogEntry(m_newest, parent, type, UniqueInteger.Get(), timestamp, 0, location, text);
                m_totalCount++;
                if (m_breaker != null && m_breaker(m_newest))
                {
                    this.BreakHere();
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

        public ILogger RootLogger { get { return m_rootScope; } }

        public bool IsDebugging { get; set; } = false;

        public LogEntry GetOldestEntry()
        {
            return m_oldest;
        }

        public LogEntry GetNewestEntry()
        {
            return m_newest;
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

        internal static Logger Root(ILogger logger)
        {
            if (logger is LoggerScope)
            {
                return ((LoggerScope)logger).Logger;
            }
            else
            {
                throw new ArgumentException();
            }
        }
    }
}
