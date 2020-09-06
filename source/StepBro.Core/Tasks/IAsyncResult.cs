using System;

namespace StepBro.Core.Tasks
{
    public interface IAsyncResult<TResult> : IAsyncResult
    {
        TResult Result { get; }
        bool IsFaulted { get; }
    }
}
