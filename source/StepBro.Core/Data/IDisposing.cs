using System;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Interface for objects which can notify when it is disposing.
    /// </summary>
    public interface IDisposing : IDisposable
    {
        event EventHandler Disposing;
    }
}
