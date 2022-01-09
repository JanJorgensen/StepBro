using StepBro.Core.Execution;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public sealed class TaskManager : ServiceBase<TaskManager, TaskManager>
    {
        private readonly List<TaskContext> m_tasks = new List<TaskContext>();

        private class TaskContext : ITaskControl, ITaskContext
        {
            protected readonly ICallContext m_callContext;
            protected Task m_task = null;
            protected ILoggerScope m_logger = null;

            public TaskContext(ICallContext context, Task task = null)
            {
                m_callContext = context;
            }

            public void SetLogger(ILoggerScope logger) { m_logger = logger; }


            #region Interfaces

            TaskExecutionState ITaskControl.CurrentState => throw new NotImplementedException();

            DateTime ITaskControl.StartTime => throw new NotImplementedException();

            DateTime ITaskControl.EndTime => throw new NotImplementedException();

            BreakOption ITaskControl.BreakOptions => throw new NotImplementedException();

            bool ITaskContext.PauseRequested => throw new NotImplementedException();

            public ILoggerScope Logger
            {
                get { return m_logger; }
            }

            event EventHandler ITaskControl.CurrentStateChanged
            {
                add
                {
                    throw new NotImplementedException();
                }

                remove
                {
                    throw new NotImplementedException();
                }
            }

            bool ITaskContext.EnterPauseIfRequested(string state)
            {
                throw new NotImplementedException();
            }

            bool ITaskControl.Kill()
            {
                throw new NotImplementedException();
            }

            void ITaskStateReporting.ProgressAliveSignal()
            {
                throw new NotImplementedException();
            }

            void ITaskStateReporting.ProgressSetup(long start, long length, Func<long, string> toText)
            {
                throw new NotImplementedException();
            }

            bool ITaskControl.RequestContinue()
            {
                throw new NotImplementedException();
            }

            bool ITaskControl.RequestPause()
            {
                throw new NotImplementedException();
            }

            bool ITaskControl.RequestStop()
            {
                throw new NotImplementedException();
            }

            void ITaskStateReporting.UpdateStatus(string text, long progress)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        private class TaskContext<TResult> : TaskContext
        {
            public TaskContext(ICallContext context) : base(context)
            {
            }

            //public Task<TResult> Call(TaskDelegateWithControl<TResult> function)
            //{
            //    var task = Task.Run(() =>
            //    {
            //        return function((ITaskControl)this, (ITaskContext)this);
            //    });
            //    m_task = task;
            //    return task;
            //}
        }

        public TaskManager(out IService serviceAccess) : base("TaskManager", out serviceAccess, typeof(ILogger))
        {

        }

        public void RegisterTask(Task task)
        {
            var taskContext = new TaskContext(null, task);
            m_tasks.Add(taskContext);
        }

        //public Task<TResult> StartTask<TResult>(ICallContext context, TaskDelegateWithControl<TResult> function)
        //{
        //    var taskContext = new TaskContext<TResult>(context);
        //    m_tasks.Add(taskContext);
        //    return taskContext.Call(function);
        //}
    }
}
