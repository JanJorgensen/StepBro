using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    //public interface IElementIndexer
    //{
    //    long FirstIndex { get; }
    //    long LastIndex { get; }
    //    int EffectiveCount { get; }
    //    object this[long index] { get; }
    //}

    public interface IElementIndexer<T>
    {
        long FirstIndex { get; }
        long LastIndex { get; }
        int EffectiveCount { get; }
        T this[long index] { get; }
    }
}
