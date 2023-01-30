using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public class LogStorage<T> : ILogHistory<T> where T : ILogHistoryEntry
    {
        private object m_addSync = new object();
        private object m_getSync = new object();
        private int m_blockSizeBits;
        private int m_blockSize;
        private int m_blockSizeMask;
        private int m_blockArraySize;
        private int m_blocksToSave;
        private T[][] m_blocks;
        private long m_firstIndex = 0L;     // Changed first time when the first block is overwritten.
        private long m_lastIndex = -1L;

        public LogStorage(int blockSizeBits = 14, int initialBlockArraySize = 100, int blocksToSave = 20)
        {
            m_blockSizeBits = blockSizeBits;
            m_blockSize = 1 << blockSizeBits;
            m_blockSizeMask = m_blockSize - 1;
            m_blockArraySize = initialBlockArraySize;
            m_blocks = new T[m_blockArraySize][];
            m_blocksToSave = blocksToSave;
        }

        public ILogHistoryEntry this[long index]
        {
            get
            {
                lock (m_getSync)
                {
                    if (index > m_lastIndex) throw new ArgumentOutOfRangeException();
                    if (index < m_firstIndex) throw new ArgumentOutOfRangeException();
                    return m_blocks[index >> m_blockSizeBits][index % m_blockSize];
                }
            }
        }

        //public FastListView.IEntry this[long index]
        //{
        //    get
        //    {
        //        lock (m_sync)
        //        {
        //            if (index > m_lastIndex) throw new ArgumentOutOfRangeException();
        //            if (index < m_firstIndex) throw new ArgumentOutOfRangeException();
        //            int i = (int)(index - m_firstIndex);
        //            int block = i / ENTRIES_IN_BLOCK;
        //            int blockIndex = (block + m_tailBlock) % BLOCKS_IN_MEMORY;
        //            //int blockIndex = block + m_tailBlock;
        //            //if (blockIndex >= BLOCKS_IN_MEMORY) blockIndex -= BLOCKS_IN_MEMORY;
        //            return m_blocks[blockIndex][i - (block * ENTRIES_IN_BLOCK)];
        //        }
        //    }
        //}

        public long FirstIndex { get { return m_firstIndex; } }
        public long LastIndex { get { return m_lastIndex; } }

        public int EffectiveCount { get { return (int)(m_lastIndex - m_firstIndex) + 1; } }

        //public event EventHandler Updated;

        public long Add(T entry)
        {
            lock (m_addSync)
            {
                m_lastIndex++;
                int block = (int)(m_lastIndex >> m_blockSizeBits);
                int indexInBlock = (int)(m_lastIndex & m_blockSizeMask);
                if (indexInBlock == 0)
                {
                    if (block >= m_blockArraySize)
                    {
                        m_blockArraySize *= 2;  // Double the array size.
                        Array.Resize(ref m_blocks, m_blockArraySize);
                    }
                    m_blocks[block] = new T[m_blockSize];
                    var oldestBlock = block - m_blocksToSave;
                    if (oldestBlock >= 0)
                    {
                        lock (m_getSync)
                        {
                            m_firstIndex += m_blockSize;
                            m_blocks[oldestBlock] = null;     // Release oldest block of entries.
                        }
                    }
                }
                m_blocks[block][indexInBlock] = entry;
            }
            //this.Updated?.Invoke(this, EventArgs.Empty);
            return m_lastIndex;
        }

        //public long Add(string category, string text)
        //{
        //    lock (m_sync)
        //    {
        //        m_lastIndexInBlock++;
        //        m_count++;
        //        m_lastIndex++;
        //        if (m_lastIndexInBlock >= ENTRIES_IN_BLOCK)
        //        {
        //            // New Block now
        //            m_headBlock = (++m_headBlock) % BLOCKS_IN_MEMORY;
        //            if (m_headBlock == m_tailBlock)     // Discard/override the oldest block?
        //            {
        //                m_tailBlock = (++m_tailBlock) % BLOCKS_IN_MEMORY;
        //                m_count -= ENTRIES_IN_BLOCK;        // All entries in the block are discarded.
        //                m_firstIndex += ENTRIES_IN_BLOCK;   // The known entries in the log now starts ENTRIES_IN_BLOCK later.
        //            }
        //            m_lastIndexInBlock = 0;
        //        }
        //        m_blocks[m_headBlock][m_lastIndexInBlock] = new FastListView.DefaultLogEntry(category, text);
        //    }
        //    //this.Updated?.Invoke(this, EventArgs.Empty);
        //    return m_lastIndex;
        //}
    }
}
