using System;
using System.Threading;

namespace StepBro.Core.Tasks;

public interface IAsyncResult<TResult> : IAsyncResult
{
    TResult Result { get; }
    bool IsFaulted { get; }
}

public class AsyncResultCompletedDummy : IAsyncResult<object>
{
    object IAsyncResult<object>.Result => null;

    bool IAsyncResult<object>.IsFaulted => false;

    object IAsyncResult.AsyncState => null;

    WaitHandle IAsyncResult.AsyncWaitHandle => null;

    bool IAsyncResult.CompletedSynchronously => true;

    bool IAsyncResult.IsCompleted => true;
}
