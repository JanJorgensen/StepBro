using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogHistoryEntry
    {
        DateTime Timestamp { get; }
    }

    public interface ILogHistory : IElementIndexer<ILogHistoryEntry>
    {
    }
}
