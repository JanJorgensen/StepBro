using StepBro.Core.Tasks;
using System;

namespace StepBro.Core.General
{
    public interface IHostApplicationActionQueue
    {
        ITask AddTask(string title, bool blockUI, System.Func<bool> precondition, TaskDelegate task);
        ITask CurrentTask { get; }
        event EventHandler TaskEnded;
        event EventHandler TaskStarted;
        uint FinishedTasks { get; }
    }
}
