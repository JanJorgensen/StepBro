using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface IDataListSource<TEntry> : IDataWalkerSource<TEntry>
    {
        Tuple<long, TEntry> GetFirst();
        TEntry GetLast();
        TEntry Get(long index);

        ///// <summary>
        ///// Create an <see cref="DataWalker"/> object that starts at the specified data entry.
        ///// </summary>
        ///// <param name="first">The entry to start from.</param>
        ///// <returns>A walker object.</returns>
        //DataWalker<TEntry> WalkFrom(TEntry first);
    }
}
