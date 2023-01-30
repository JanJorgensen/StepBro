using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogEntry
    {
        ulong Id { get; }
        DateTime Timestamp { get; }
    }

    public interface ILogHistory<T> : IElementIndexer<T> where T : ILogEntry
    {
    }
}
