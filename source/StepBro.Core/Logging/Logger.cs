using StepBro.Core.Data;
using System;

namespace StepBro.Core.Logging
{
    public class Logger : IDisposable, IDataListSource<LogEntry>
    {
        private class LogWalker : IDataWalker<LogEntry>
        {
            private Logger m_parent;
            private LogEntry m_current;
            private long m_currentIndex;
            private string m_name;

            public LogWalker(Logger parent, LogEntry current, long currentIndex)
            {
                m_parent = parent;
                m_current = current;
                m_currentIndex = currentIndex;
                m_name = "LoggerWalker";
            }

            public LogEntry CurrentEntry { get { return m_current; } }

            public long CurrentIndex { get { return m_currentIndex; } }

            public LogEntry GetNext()
            {
                if (m_current.Next != null)
                {
                    m_current = m_current.Next;
                    m_currentIndex++;
                    return m_current;
                }
                else
                {
                    return null;
                }
            }

            public bool HasMore()
            {
                return m_current.Next != null;
            }

            public IDataWalker<LogEntry> Dublicate()
            {
                var walker = new LogWalker(m_parent, m_current, m_currentIndex);
                walker.m_name = m_name + "Derived";
                return walker;
            }

            public override string ToString()
            {
                return "Logger.LogWalker " + m_name + " @ " + m_currentIndex;
            }
        }

        private class HistoryController : LinkedListHistoryController<LogEntry>
        {
            public HistoryController(object sync, uint blockSize, uint blockCount) :
                base(sync, blockSize, blockCount)
            { }
            protected override LogEntry GetNext(LogEntry entry)
            {
                return entry?.Next;
            }
        }

        private readonly object m_sync;
        private LogEntry m_last = null;
        public uint BLOCK_SIZE = 2000;
        public uint BLOCK_COUNT = 5000;
        private HistoryController m_history;
        internal IProtectedLogger m_rootScope;
        private Predicate<LogEntry> m_breaker = null;
        private IService m_rootScopeService = null;
        private readonly string m_outputfile;
        private readonly bool m_directLogToFile;

        public Logger(string outputFile, bool directLogToFile, string location, string starttext)
        {
            m_sync = UniqueInteger.m_sync;
            m_outputfile = outputFile;
            m_directLogToFile = directLogToFile && !String.IsNullOrEmpty(outputFile);

            m_last = new LogEntry(
                UniqueInteger.GetLongDirectly(),
                DateTime.UtcNow,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                location,
                starttext);
            m_history = new HistoryController(m_sync, BLOCK_SIZE, BLOCK_COUNT);
            m_history.NotifyNew(m_last);
            m_rootScope = new LoggerScope(this, m_last, out m_rootScopeService);
        }

        public IService RootScopeService { get { return m_rootScopeService; } }

        internal LogEntry Log(LogEntry parent, LogEntry.Type type, DateTime timestamp, int thread, string location, string text)
        {
            lock (m_sync)
            {
                m_last = new LogEntry(m_last, parent, type, UniqueInteger.GetLongDirectly(), timestamp, thread, location, text);
                m_history.NotifyNew(m_last);
                if (m_breaker != null && m_breaker(m_last))
                {
                    this.BreakHere();
                }
                return m_last;
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

        public Tuple<long, LogEntry> GetFirst()
        {
            lock (m_sync)
            {
                return new Tuple<long, LogEntry>(m_history.FirstIndex, m_history.FirstEntry);
            }
        }

        public LogEntry GetLast()
        {
            lock (m_sync)
            {
                return m_last;
            }
        }

        public void DebugDump()
        {
            var entry = m_history.FirstEntry;
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

        public LogEntry Get(long index)
        {
            return m_history.Get(index);
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

        public IDataWalker<LogEntry> WalkFrom(long first)
        {
            if (first < 0) first = m_history.FirstIndex;
            var entry = m_history.Get(first);
            if (entry != null)
            {
                return new LogWalker(this, entry, first);
            }
            else
            {
                return null;
            }
        }

        public IndexerStateSnapshot GetState()
        {
            lock (m_sync)
            {
                var first = m_history.FirstIndex;
                var last = m_history.LastIndex;
                if (first < 0L) return new IndexerStateSnapshot(-1L, -1L, 0L);
                else return new IndexerStateSnapshot(first, last, last - first + 1);
            }
        }
    }
}
