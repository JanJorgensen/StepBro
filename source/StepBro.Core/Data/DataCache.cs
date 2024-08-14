using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class DataCache<TEntry> : IElementIndexer<TEntry> where TEntry : class
    {
        private uint m_cacheSize;
        private uint m_minimumCacheFill;
        private IDataWalkerSource<TEntry> m_source;
        private long m_firstCachedIndex = -1L;
        private long m_lastCachedIndex = -1L;
        private long m_firstCachedLocation = -1L;
        private TEntry[] m_cache;
        private IDataWalker<TEntry> m_walker = null;

        public DataCache(IDataWalkerSource<TEntry> source, uint cacheSize = 200, uint minimumCacheFill = 20)
        {
            m_source = source;
            m_cache = new TEntry[cacheSize];
            m_cacheSize = cacheSize;
            m_minimumCacheFill = minimumCacheFill;
        }

        public IDataWalker<TEntry> CurrentWalker { get { return m_walker; } }

        internal Tuple<long, long> CachedRange()
        {
            return new Tuple<long, long>(m_firstCachedIndex, m_lastCachedIndex);
        }

        public object this[long index] => throw new NotImplementedException();

        public TEntry Get(long index)
        {
            bool forceUpdate = false;
            TEntry returnEntry = null;  // Not found yet.
            if (m_firstCachedIndex < 0L || index < m_firstCachedIndex)
            {
                bool dataAvailable = this.Reset();
                if (index == 0 && m_firstCachedIndex >= 0)
                {
                    returnEntry = m_cache[m_firstCachedIndex];
                }
                if (!dataAvailable) return null;
                forceUpdate = true;
            }

            if (index > m_lastCachedIndex || forceUpdate)
            {
                var lastLocation = ((m_lastCachedIndex - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize;
                var end = (index != Int64.MaxValue) ? (index + (m_minimumCacheFill - 1)) : Int64.MaxValue;

                while (m_lastCachedIndex < end && m_walker.HasMore())
                {
                    m_lastCachedIndex++;
                    lastLocation = (lastLocation + 1) % m_cacheSize;
                    var entry = m_walker.GetNext();
                    m_cache[lastLocation] = entry;
                    if (lastLocation == m_firstCachedLocation)
                    {
                        m_firstCachedLocation = (m_firstCachedLocation + 1) % m_cacheSize;
                        m_firstCachedIndex++;
                    }

                    if (m_lastCachedIndex == index) returnEntry = m_walker.CurrentEntry;
                }
                return returnEntry;
            }
            else
            {
                return m_cache[((index - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize];
            }
        }

        private bool Reset()
        {
            m_firstCachedIndex = -1L;
            m_lastCachedIndex = -1L;
            m_firstCachedLocation = -1L;

            m_walker = m_source.WalkFrom();
            if (m_walker != null && m_walker.CurrentEntry != null)
            {
                m_firstCachedIndex = 0L;
                m_firstCachedLocation = 0;
                m_cache[0] = m_walker.CurrentEntry;
                m_lastCachedIndex = 0L;
                return true;
            }
            else
            {
                return false;   // Empty source!
            }
        }

        public void Clear(long indexBeforeAdding = -1L)
        {
            m_firstCachedIndex = indexBeforeAdding;
            m_lastCachedIndex = indexBeforeAdding;
            m_firstCachedLocation = -1L;
        }

        public IndexerStateSnapshot GetState()
        {
            // Returning the state of the cache.
            // The caller should know that the cache might not contain all entries of the cache source.
            var count = (m_firstCachedIndex >= 0) ? (m_lastCachedIndex - m_firstCachedIndex + 1) : 0L;
            return new IndexerStateSnapshot(m_firstCachedIndex, m_lastCachedIndex, count);
        }
    }
}
