using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public struct IndexerStateSnapshot
    {
        public IndexerStateSnapshot(long first, long last, long count)
        {
            this.FirstIndex = first;
            this.LastIndex = last;
            this.EffectiveCount = count;
        }
        public long FirstIndex { get; private set; }
        public long LastIndex { get; private set; }
        public long EffectiveCount { get; private set; }
    }

    public interface IElementIndexer
    {
        IndexerStateSnapshot GetState();
        object this[long index] { get; }
    }

    public interface IElementIndexer<T> : IElementIndexer
    {
        T Get(long index);
    }
}
