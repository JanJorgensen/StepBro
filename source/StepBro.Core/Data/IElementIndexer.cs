using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public struct IndexerStateSnapshot
    {
        public IndexerStateSnapshot(ulong first, ulong last, ulong count)
        {
            this.FirstIndex = first;
            this.LastIndex = last;
            this.EffectiveCount = count;
        }
        public ulong FirstIndex { get; private set; }
        public ulong LastIndex { get; private set; }
        public ulong EffectiveCount { get; private set; }
    }

    public interface IElementIndexer
    {
        IndexerStateSnapshot GetState();
        object this[long index] { get; }
    }

    public interface IElementIndexer<T> : IElementIndexer
    {
        T Get(ulong index);
    }
}
