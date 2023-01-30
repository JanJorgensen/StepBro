using StepBro.Core.Data;
using System;

namespace StepBro.Core.Logging
{
    public class Logger : /*LogStorage<LogEntry>, */IDisposable
    {
        private readonly object m_sync = new object();
        private ulong m_firstIndex = 0;      // The history index of the first entry currently known. This will only change when entries are disposed.
        private ulong m_lastIndex = 0;
        private readonly string m_outputfile;
        private readonly bool m_directLogToFile;
        private LogEntry m_oldest = null;
        private LogEntry m_newest = null;
        public uint BLOCK_SIZE = 2000;
        public uint BLOCK_COUNT = 5000;
        public LogEntry[] m_blockStarts;        // The saved entry references for shorter search time.
        public uint m_blockIndex = 0;           // The block currently being used (added to).
        internal IProtectedLogger m_rootScope;
        private Predicate<LogEntry> m_breaker = null;
        private IService m_rootScopeService = null;

        public Logger(string outputFile, bool directLogToFile, string location, string starttext)
        {
            m_outputfile = outputFile;
            m_directLogToFile = directLogToFile && !String.IsNullOrEmpty(outputFile);

            m_blockStarts = new LogEntry[BLOCK_COUNT];
            for (int i = 0; i < BLOCK_COUNT; i++) m_blockStarts[i] = null;

            m_oldest = new LogEntry(
                UniqueInteger.GetLong(),
                DateTime.Now,
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                location,
                starttext);
            m_newest = m_oldest;
            m_blockStarts[0] = m_oldest;
            m_rootScope = new LoggerScope(this, m_newest, out m_rootScopeService);
        }

        public IService RootScopeService { get { return m_rootScopeService; } }

        internal LogEntry Log(LogEntry parent, LogEntry.Type type, DateTime timestamp, int thread, string location, string text)
        {
            lock (m_sync)
            {
                m_newest = new LogEntry(m_newest, parent, type, UniqueInteger.GetLong(), timestamp, 0, location, text);

                if ((++m_lastIndex % BLOCK_SIZE) == 0)
                {
                    m_blockIndex = (m_blockIndex + 1) % BLOCK_COUNT;
                    if (++m_blockIndex >= BLOCK_COUNT)
                    {
                        m_blockIndex = 0;
                    }
                    if (m_blockStarts[m_blockIndex] != null)
                    {
                        // Loose the connection the the oldest block to avoid using all memory.
                        m_oldest = m_blockStarts[m_blockIndex];
                        m_firstIndex += BLOCK_SIZE;
                    }
                    m_blockStarts[m_blockIndex] = m_newest;
                }

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

        public LogEntry Get(ulong index)
        {
            uint c = 0;
            LogEntry firstEntry = null;
            lock (m_sync)
            {
                if (index >= m_firstIndex && index <= m_lastIndex)
                {
                    var firstBlockIndex = (m_firstIndex == 0) ? 0 : ((m_blockIndex + (BLOCK_COUNT + 1)) % BLOCK_COUNT);
                    var blockIndex = (((index - m_firstIndex) / BLOCK_SIZE) + firstBlockIndex) % BLOCK_COUNT;
                    firstEntry = m_blockStarts[blockIndex];
                    c = (uint)index % BLOCK_SIZE;
                }
                else
                {
                    return null;
                }
            }
            // This moved out to avoid staying locked.
            var entry = firstEntry;
            for (int i = 0; i < c; i++)
            {
                entry = entry.Next;
            }
            return entry;

        }

        public ILogHistory<LogEntry> GetHistory()
        {
            return new HistoryAccess(this, 1000, 50);
        }

        private class HistoryAccess : ILogHistory<LogEntry>
        {
            private uint m_cacheSize;
            private uint m_minimumCacheFill = 5;
            private Logger m_parent;
            private ulong m_firstCachedIndex = UInt64.MaxValue;
            private ulong m_lastCachedIndex = UInt64.MaxValue;
            private ulong m_firstCachedLocation = 0UL;
            private LogEntry[] m_cache;

            public HistoryAccess(Logger parent, uint cacheSize = 200, uint minimumCacheFill = 20)
            {
                m_parent = parent;
                m_cache = new LogEntry[cacheSize];
                m_cacheSize = cacheSize;
                m_minimumCacheFill = minimumCacheFill;
            }

            public object this[long index] => throw new NotImplementedException();

            public LogEntry Get(ulong index)
            {
                if (m_firstCachedIndex == UInt64.MaxValue || index < m_firstCachedIndex)
                {
                    return Reset(index);
                }
                else if (index > m_lastCachedIndex)
                {
                    IndexerStateSnapshot indexer = this.GetState();
                    if (index > indexer.LastIndex) return null;
                    if (index < (ulong)(m_lastCachedIndex + m_cacheSize))
                    {
                        var lastLocation = ((m_lastCachedIndex - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize;
                        var entry = m_cache[lastLocation];
                        LogEntry returnEntry = null;
                        var end = index + (m_minimumCacheFill - 1);
                        entry = entry.Next;
                        while (m_lastCachedIndex < end && entry != null)
                        {
                            m_lastCachedIndex++;
                            lastLocation = (lastLocation + 1) % m_cacheSize;
                            m_cache[lastLocation] = entry;
                            if (lastLocation == m_firstCachedLocation)
                            {
                                m_firstCachedLocation = (m_firstCachedLocation + 1) % m_cacheSize;
                                m_firstCachedIndex++;
                            }

                            if (m_lastCachedIndex == index) returnEntry = entry;
                            entry = entry.Next;
                        }
                        return returnEntry;
                    }
                    else
                    {
                        return Reset(index);
                    }
                }
                return m_cache[((index - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize];
            }

            private LogEntry Reset(ulong index)
            {
                var entry = m_parent.Get(index);
                if (entry != null)
                {
                    m_firstCachedIndex = index;
                    m_firstCachedLocation = 0;
                    for (int i = 0; i < m_minimumCacheFill && entry != null; i++)
                    {
                        m_cache[i] = entry;
                        m_lastCachedIndex = index++;
                        entry = entry.Next;
                    }
                    entry = m_cache[0];
                }
                else
                {
                    m_firstCachedIndex = m_lastCachedIndex = UInt64.MaxValue;
                }
                return entry;
            }

            public IndexerStateSnapshot GetState()
            {
                lock (m_parent.m_sync)
                {
                    return new IndexerStateSnapshot(m_parent.m_firstIndex, m_parent.m_lastIndex, (m_parent.m_lastIndex - m_parent.m_firstIndex) + 1UL);
                }
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
