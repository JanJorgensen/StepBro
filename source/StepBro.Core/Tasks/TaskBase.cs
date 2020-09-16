using System;
using System.Threading;

namespace StepBro.Core.Tasks
{
    public abstract class TaskBase : ITask
    {
        protected TaskBase(string title)
        {
            this.Title = title;
        }

        public string Title { get; private set; }

        public abstract ITaskControl Control { get; }

        public bool Wait(TimeSpan time)
        {
            return this.DoWait(time);
        }

        protected abstract bool DoWait(TimeSpan time);

        protected void Start(ITaskContext context)
        {
            try
            {
                this.ExecuteTaskFunction(context);
            }
            catch (Exception)
            {
            }
        }

        protected abstract void ExecuteTaskFunction(ITaskContext context);
    }
}
