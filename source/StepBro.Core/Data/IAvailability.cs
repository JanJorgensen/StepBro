using System;

namespace StepBro.Core.Data
{
    public interface IAvailability : IDisposing
    {
        event EventHandler Disposed;
        bool IsStillValid { get; }
    }
}
