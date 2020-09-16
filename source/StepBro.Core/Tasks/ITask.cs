using System;

namespace StepBro.Core.Tasks
{
    public interface ITask
    {
        string Title { get; }
        ITaskControl Control { get; }
        bool Wait(TimeSpan time);
    }
}
