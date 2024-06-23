using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface ILinkedListIndexer
    {
        object Get(ulong index);
    }

    public interface ILinkedListIndexer<TData>
    {
        TData Get(ulong index);
    }
}
