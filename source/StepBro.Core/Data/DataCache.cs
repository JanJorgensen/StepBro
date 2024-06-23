using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private DataWalker<TEntry> m_walker = null;

        public DataCache(IDataWalkerSource<TEntry> source, uint cacheSize = 200, uint minimumCacheFill = 20)
        {
            m_source = source;
            m_cache = new TEntry[cacheSize];
            m_cacheSize = cacheSize;
            m_minimumCacheFill = minimumCacheFill;
        }

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
                    //System.Diagnostics.Debug.WriteLine("Add to cache: " + entry.ToString());
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

        //public TEntry Get(long index)
        //{
        //    if (m_firstCachedIndex < 0L || index < m_firstCachedIndex)
        //    {
        //        return Reset(index);
        //    }
        //    else if (index > m_lastCachedIndex)
        //    {
        //        if (index > m_walker.LastKnownIndex && !m_walker.HasMore()) return null;
        //        if (index < (m_lastCachedIndex + m_cacheSize))  // Short jump from the current cached entries?
        //        {
        //            var lastLocation = ((m_lastCachedIndex - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize;
        //            //var entry = m_cache[lastLocation];
        //            TEntry returnEntry = null;  // Not found yet.
        //            var end = index + (m_minimumCacheFill - 1);
        //            //entry = entry.Next;
        //            while (m_lastCachedIndex < end && m_walker.HasMore())
        //            {
        //                m_lastCachedIndex++;
        //                lastLocation = (lastLocation + 1) % m_cacheSize;
        //                m_cache[lastLocation] = m_walker.GetNext();
        //                if (lastLocation == m_firstCachedLocation)
        //                {
        //                    m_firstCachedLocation = (m_firstCachedLocation + 1) % m_cacheSize;
        //                    m_firstCachedIndex++;
        //                }

        //                if (m_lastCachedIndex == index) returnEntry = m_walker.CurrentEntry;
        //            }
        //            return returnEntry;
        //        }
        //        else
        //        {
        //            // Long jump; discard current and re-read.
        //            return Reset(index);
        //        }
        //    }
        //    return m_cache[((index - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize];
        //}

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

        //private TEntry Reset(long index)
        //{
        //    m_walker = m_source.WalkFrom(Math.Max(0L, (index - m_cacheSize / 4)));  // Start from 1/4 of the sice of the cache before the specified entry.   
        //    if (m_walker != null)
        //    {
        //        m_firstCachedIndex = index;
        //        m_firstCachedLocation = 0;
        //        for (int i = 0; i < m_minimumCacheFill; i++)
        //        {
        //            m_cache[i] = m_walker.CurrentEntry;
        //            m_lastCachedIndex = index++;
        //            if (m_walker.HasMore()) m_walker.GetNext();
        //            else break;
        //        }
        //        return m_cache[0];
        //    }
        //    else
        //    {
        //        m_firstCachedIndex = m_lastCachedIndex = -1L;
        //    }
        //    return null;
        //}

        public void Clear(long indexBeforeAdding = -1L)
        {
            m_firstCachedIndex = indexBeforeAdding;
            m_lastCachedIndex = indexBeforeAdding;
            m_firstCachedLocation = -1L;
        }

        //public void Add(TEntry entry)
        //{
        //    if (m_firstCachedIndex < 0L || m_firstCachedLocation < 0L)
        //    {
        //        m_firstCachedIndex++;
        //        m_lastCachedIndex = m_firstCachedIndex;
        //        m_firstCachedLocation = 0L;
        //        m_cache[m_firstCachedLocation] = entry;
        //    }
        //    else
        //    {
        //        var lastLocation = ((m_lastCachedIndex - m_firstCachedIndex) + m_firstCachedLocation) % m_cacheSize;
        //        lastLocation = (lastLocation + 1) % m_cacheSize;
        //        m_lastCachedIndex++;
        //        m_cache[lastLocation] = entry;
        //        if (lastLocation == m_firstCachedLocation)
        //        {
        //            m_firstCachedLocation = (m_firstCachedLocation + 1) % m_cacheSize;
        //            m_firstCachedIndex++;
        //        }
        //    }
        //}

        public IndexerStateSnapshot GetState()
        {
            return m_source.GetState();
        }
    }
}
