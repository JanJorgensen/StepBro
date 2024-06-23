using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface DataWalker<TEntry>
    {
        TEntry GetNext();
        bool HasMore();
        TEntry CurrentEntry { get; }
        long CurrentIndex { get; }

        /// <summary>
        /// Create a DataWalker at the same current location.
        /// </summary>
        /// <returns>New DataWalker object.</returns>
        DataWalker<TEntry> Dublicate();
    }

    public interface IDataWalkerSource<TEntry>
    {
        IndexerStateSnapshot GetState();

        /// <summary>
        /// Create an <see cref="DataWalker"/> object that starts at the specified index.
        /// </summary>
        /// <param name="start">The start index for the walker object.</param>
        /// <returns>A walker object.</returns>
        DataWalker<TEntry> WalkFrom(long start = -1L);
    }
}
