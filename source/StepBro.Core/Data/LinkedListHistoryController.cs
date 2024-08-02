using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public abstract class LinkedListHistoryController<T> where T : class
    {
        protected object m_sync;
        private long m_firstIndex = 0L;     // The history index of the first entry currently known. This will only change when entries are disposed.
        private long m_lastIndex = -1L;
        private T m_first = null;
        private T m_last = null;
        private uint BLOCK_SIZE = 2000;
        private uint BLOCK_COUNT = 5000;
        private T[] m_blockStarts;          // The saved entry references for shorter search time.
        private uint m_blockIndex = 0;     // The block currently being used (added to).

        public LinkedListHistoryController(object sync, uint blockSize, uint blockCount)
        {
            m_sync = sync;
            BLOCK_SIZE = blockSize;
            BLOCK_COUNT = blockCount;
            m_blockStarts = new T[BLOCK_COUNT];
            this.Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < BLOCK_COUNT; i++) m_blockStarts[i] = null;
            m_blockIndex = BLOCK_COUNT - 1;
            m_firstIndex = 0L;
            m_lastIndex = -1L;
            m_first = null;
            m_last = null;
        }

        public long FirstIndex { get { return m_firstIndex; } }
        public long LastIndex { get { return m_lastIndex; } }
        public T FirstEntry { get { return m_first; } }
        public T LastEntry { get { return m_last; } }

        public void NotifyNew(T element)
        {
            m_last = element;
            if ((++m_lastIndex % BLOCK_SIZE) == 0)
            {
                if (m_first == null) m_first = element;
                //m_blockIndex = (m_blockIndex + 1) % BLOCK_COUNT;
                if (++m_blockIndex >= BLOCK_COUNT)
                {
                    m_blockIndex = 0;
                }
                if (m_blockStarts[m_blockIndex] != null)
                {
                    // Release the connection to the oldest block to avoid using all system memory.
                    m_first = m_blockStarts[m_blockIndex];
                    m_firstIndex += BLOCK_SIZE;
                }
                m_blockStarts[m_blockIndex] = m_last;
            }
        }

        public T Get(long index)
        {
            uint c = 0;
            T firstEntry = null;
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
                entry = this.GetNext(entry);
            }
            return entry;
        }

        protected abstract T GetNext(T entry);
    }
}
