using StepBro.Core.Api;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Data
{
    public interface ILineReaderEntry
    {
        string Text { get; }
        DateTime Timestamp { get; }
    }

    [Public]
    public interface ILineReader
    {
        INameable Source { get; }
        object Sync { get; }
        ILineReaderEntry Current { get; }
        ILineReaderEntry Previous { get; }
        bool LinesHaveTimestamp { get; }
        bool HasMore { get; }
        bool Next();
        void Flush(ILineReaderEntry stopAt = null);
        IEnumerable<ILineReaderEntry> Peak();
        event EventHandler LinesAdded;
        void DebugDump();
    }
}
