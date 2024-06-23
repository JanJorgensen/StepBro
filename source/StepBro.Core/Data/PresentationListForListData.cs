using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.Core.Data
{
    public interface IPresentationList<TPresentationEntry> :
        IElementIndexer<TPresentationEntry>
        where TPresentationEntry : class
    {
        /// <summary>
        /// Searches for the specified entry, or the entry that mathes as closely as possible;
        /// </summary>
        /// <param name="entry">The entry/data to search for.</param>
        /// <returns>The index of the found entry, or the index of the entry </returns>
        long SearchForEntry(TPresentationEntry entry);
    }


    public abstract class PresentationListForListData<TSourceEntry, TPresentationEntry> :
        IPresentationList<TPresentationEntry>
        where TSourceEntry : class
        where TPresentationEntry : class
    {
        private class WalkerSource : IDataWalkerSource<TPresentationEntry>
        {
            private PresentationListForListData<TSourceEntry, TPresentationEntry> m_parent;

            public WalkerSource(PresentationListForListData<TSourceEntry, TPresentationEntry> parent)
            {
                m_parent = parent;
            }

            public IndexerStateSnapshot GetState()
            {
                return m_parent.GetState();
            }

            public DataWalker<TPresentationEntry> WalkFrom(long start)
            {
                if (start < 0L) start = 0;
                System.Diagnostics.Debug.Assert(start == 0);    // Other values not supported yet.
                var walker = new Walker(m_parent);
                walker.SearchFirst();
                return walker;
            }
        }

        private class Walker : DataWalker<TPresentationEntry>
        {
            private PresentationListForListData<TSourceEntry, TPresentationEntry> m_parent;
            private DataWalker<TSourceEntry> m_sourceWalker;
            private System.Predicate<TSourceEntry> m_filter;
            private long m_currentIndex = -1L;
            private Queue<TPresentationEntry> m_inHand = new Queue<TPresentationEntry>();

            public Walker(PresentationListForListData<TSourceEntry, TPresentationEntry> parent)
            {
                m_parent = parent;
                m_sourceWalker = m_parent.m_source.WalkFrom();
                m_filter = m_parent.m_filter;
            }

            private Walker(Walker other)
            {
                m_parent = other.m_parent;
                m_sourceWalker = (DataWalker<TSourceEntry>)other.Dublicate();
                m_filter = other.m_filter;
                m_currentIndex = other.m_currentIndex;
                foreach (var item in other.m_inHand)
                {
                    m_inHand.Enqueue(item);
                }
            }

            public TPresentationEntry CurrentEntry
            {
                get
                {
                    TPresentationEntry entry;
                    if (m_inHand.TryPeek(out entry))
                    {
                        return entry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public long CurrentIndex { get { return m_currentIndex; } }

            //public long LastKnownIndex { get { return m_parent.GetState().LastIndex; } }

            public bool SearchNext()
            {
                // Note: If none found yet, we only search for the first.
                if (m_inHand.Count > 1) return true;
                while (m_sourceWalker.HasMore())
                {
                    var next = m_sourceWalker.GetNext();
                    if (Check(next, m_sourceWalker.CurrentIndex)) return true;
                }
                return false;
            }

            public bool SearchFirst()
            {
                if (this.CheckFirst())
                {
                    m_currentIndex = 0;
                    return true;
                }
                else
                {
                    if (this.SearchNext())   // Check if there are any entries right now.
                    {
                        m_currentIndex = 0;
                        return true;
                    }
                }
                return false;
            }

            public bool Check(TSourceEntry entry, long index)
            {
                var foundAtEntry = m_inHand.Count;
                if (m_filter(entry))
                {
                    m_parent.CreatePresentationEntry(entry, index, this.AddEntry);
                    if (m_inHand.Count > foundAtEntry) return true;
                }
                return false;
            }

            public bool CheckFirst()
            {
                return this.Check(m_sourceWalker.CurrentEntry, m_sourceWalker.CurrentIndex);
            }

            private void AddEntry(TPresentationEntry entry)
            {
                m_inHand.Enqueue(entry);
            }

            public TPresentationEntry GetNext()
            {
                var inHandCount = m_inHand.Count;
                if (inHandCount > 1 || SearchNext())
                {
                    if (inHandCount > 0)
                    {
                        m_inHand.Dequeue();    // If there were a current entry, remove that.
                        //System.Diagnostics.Debug.WriteLine("Walker.GetNext now on: " + m_inHand.Peek().ToString());
                    }
                    //else 
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Walker.GetNext found: " + m_inHand.Peek().ToString());
                    //}
                    m_currentIndex++;
                    return m_inHand.Peek();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Walker.GetNext negative");
                    return null;
                }
            }

            public bool HasMore()
            {
                return (m_inHand.Count > 1 || SearchNext());
            }

            public DataWalker<TPresentationEntry> Dublicate()
            {
                return new Walker(this);
            }
        }

        private IDataListSource<TSourceEntry> m_source;
        private WalkerSource m_walkerSource;
        private System.Predicate<TSourceEntry> m_filter;
        protected DataCache<TPresentationEntry> m_cache;
        private DataWalker<TSourceEntry> m_sourceTipWalker = null;
        private DataWalker<TSourceEntry> m_sourceCacheWalker = null;
        private bool m_tipMode = true;
        //private long m_tipIndex = -1;

        public PresentationListForListData(
            IDataListSource<TSourceEntry> source,
            uint cacheSize, uint minimumCacheFill)
        {
            m_source = source;
            m_walkerSource = new WalkerSource(this);
            m_cache = new DataCache<TPresentationEntry>(m_walkerSource, cacheSize, minimumCacheFill);
        }

        public void SetTipMode(bool inTipMode)
        {
            if (m_tipMode != inTipMode)
            {
                m_tipMode = inTipMode;
                if (m_tipMode)
                {
                }
            }
        }

        public abstract void CreatePresentationEntry(TSourceEntry entry, long sourceIndex, System.Action<TPresentationEntry> adder);

        /// <summary>
        /// Function used to get the source entry for a specified presentation entry.
        /// </summary>
        /// <param name="entry">A presentation entry.</param>
        /// <returns>The source entry</returns>
        public abstract TSourceEntry PresentationToSource(TPresentationEntry entry);

        /// <summary>
        /// Clear the history and cache, and rescan the source using the specified filter.
        /// </summary>
        /// <param name="filter">The filter to use for selecting entries for presentation.</param>
        /// <param name="focusSourceEntry">The index of a source entry that should be included in the cached entries.</param>
        public void Reset(System.Predicate<TSourceEntry> filter, long focusSourceEntry)
        {
            m_filter = filter;
            m_cache.Clear();

            var first = m_source.GetFirst();
            m_sourceCacheWalker = m_source.WalkFrom(first.Item1);
            if (!m_tipMode)
            {
                m_sourceTipWalker = m_sourceCacheWalker.Dublicate();
            }
            if (m_tipMode)
            {
                this.Get(Int64.MaxValue);
            }
            else
            {
                // TODO: Search for the location of the entry with the specified source index.
            }
            //var adder = (TPresentationEntry entry) =>
            //{
            //    //this.NotifyNew(entry);
            //    m_cache.Add(entry);
            //};

            //while (m_sourceTipWalker.CurrentEntry != null)
            //{
            //    if (m_filter(m_sourceTipWalker.CurrentEntry))
            //    {
            //        this.CreatePresentationEntry(m_sourceTipWalker.CurrentEntry, m_sourceTipWalker.CurrentIndex, adder);
            //    }
            //    if (!m_sourceTipWalker.HasMore()) break;
            //    m_sourceTipWalker.GetNext();
            //}
        }

        public object this[long index]
        {
            get { return m_cache[index]; }
        }

        public TPresentationEntry Get(long index)
        {
            // Preconditions: Tip has been checked/updated (and maybe cache updated).
            return m_cache.Get(index);
        }

        public IndexerStateSnapshot GetState()
        {
            //if (m_tipMode)
            {
                var cacheState = m_cache.CachedRange();
                long count = (cacheState.Item1 >= 0) ? ((cacheState.Item2 - cacheState.Item1) + 1) : 0;
                return new IndexerStateSnapshot(
                    (count > 0) ? 0L : -1L,
                    cacheState.Item2,
                    (count > 0) ? (cacheState.Item2 + 1L) : 0L);
            }
            //else
            //{
            //    throw new NotImplementedException();
            //}
        }

        /// <summary>
        /// Make the list check for new data in the source, run new data through the filter and update index of last known.
        /// </summary>
        public void UpdateTip()
        {
            if (m_tipMode)
            {
                m_cache.Get(Int64.MaxValue);
            }
        }

        public long SearchForEntry(TPresentationEntry entry)
        {
            return -1L;
        }


        //private void AddToCacheAndTip(TPresentationEntry entry)
        //{
        //    this.AddToTip(entry);
        //    this.AddToCache(entry);
        //}

        //private void AddToCache(TPresentationEntry entry)
        //{
        //    m_cache.Add(entry);
        //}

        //private void AddToTip(TPresentationEntry entry)
        //{
        //    m_tipIndex++;
        //}
    }
}
