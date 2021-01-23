using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.General
{
    internal class HostApplicationActionQueue : ServiceBase<IHostApplicationActionQueue, HostApplicationActionQueue>, IHostApplicationActionQueue
    {
        private class TaskData : TaskBase, ITaskControl, IDisposable
        {
            private class TaskContext : ITaskContext
            {
                private readonly TaskData m_parent;

                public TaskContext(TaskData parent)
                {
                    m_parent = parent;
                }

                public bool PauseRequested { get { return m_parent.PauseRequested; } }

                public ILoggerScope Logger { get { return m_parent.Logger; } }

                public bool EnterPauseIfRequested(string state)
                {
                    return m_parent.EnterPauseIfRequested(state);
                }

                public void ProgressAliveSignal()
                {
                    m_parent.ProgressAliveSignal();
                }

                public void ProgressSetup(long start, long length, Func<long, string> toText)
                {
                    m_parent.ProgressSetup(start, length, toText);
                }

                public void UpdateStatus(string text, long progress)
                {
                    m_parent.UpdateStatus(text, progress);
                }
            }

            private ILoggerScope m_logger = null;
            System.Func<bool> m_precondition;
            private TaskDelegate m_taskFunction;
            private readonly object m_sync = new object();
            private readonly bool m_blockUI;
            private readonly TaskContext m_context = null;
            private TaskExecutionState m_state = TaskExecutionState.Created;
            private Task m_task = null;

            public TaskData(string title, bool blockUI, System.Func<bool> precondition, TaskDelegate taskFunction) : base(title)
            {
                m_blockUI = blockUI;
                m_precondition = precondition;
                m_taskFunction = taskFunction;
                m_context = new TaskContext(this);
            }

            public void Dispose()
            {
                m_taskFunction = null;
            }

            public override ITaskControl Control
            {
                get { return this; }
            }

            public ILoggerScope Logger { get { return m_logger; } }

            public void SetTaskReference(Task task)
            {
                m_task = task;
            }

            public void SetTaskLogger(ILoggerScope logger)
            {
                m_logger = logger;
            }

            #region ITaskControl Interface

            public TaskExecutionState CurrentState { get { return m_state; } }

            public DateTime StartTime => throw new NotImplementedException();

            public DateTime EndTime => throw new NotImplementedException();

            public BreakOption BreakOptions => throw new NotImplementedException();

            public event EventHandler CurrentStateChanged;

            public bool Kill()
            {
                throw new NotImplementedException();
            }

            public bool RequestContinue()
            {
                throw new NotImplementedException();
            }

            public bool RequestPause()
            {
                throw new NotImplementedException();
            }

            public bool RequestStop()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Context access

            public bool PauseRequested { get { return false; } }

            public bool EnterPauseIfRequested(string state)
            {
                return false;
            }

            public void ProgressAliveSignal()
            {
            }

            public void ProgressSetup(long start, long length, Func<long, string> toText)
            {
            }

            public void UpdateStatus(string text = null, long progress = -1)
            {
                this.Logger.Log(this.Title, text);
            }

            #endregion

            protected void ChangeState(TaskExecutionState state)
            {
                bool changed = false;
                lock (m_sync)
                {
                    if (m_state != state)
                    {
                        m_state = state;
                        changed = true;
                    }
                }
                if (changed) this.CurrentStateChanged?.Invoke(this, EventArgs.Empty);
            }

            public void RunTask()
            {
                try
                {
                    m_logger.EnteredParallelTask(this.Title, null);
                    this.ExecuteTaskFunction(m_context);
                    m_logger.LogExit("Ended successfully", "");
                }
                catch (Exception ex)
                {
                    m_logger.LogExit("Ended with fail", ex.ToString());
                }
            }

            protected override void ExecuteTaskFunction(ITaskContext context)
            {
                if (m_state == TaskExecutionState.Created)
                {
                    this.ChangeState(TaskExecutionState.Running);
                    try
                    {
                        m_taskFunction(m_context);
                        this.ChangeState(TaskExecutionState.Ended);
                    }
                    catch (Exception)
                    {
                        this.ChangeState(TaskExecutionState.Ended);
                    }
                }
            }

            protected override bool DoWait(TimeSpan timeout)
            {
                if (m_task == null)
                {
                    var entry = DateTime.Now;
                    if (timeout == TimeSpan.MinValue)
                    {
                        while (m_task == null)
                        {
                            Thread.Sleep(10);
                            //if (DateTime.Now > (entry + timeout)) throw new TimeoutException();
                        }
                        return m_task.Wait(timeout);
                    }
                    else if (timeout < TimeSpan.Zero)
                    {
                        if (m_task == null) return false;
                        else if (!m_task.IsCompleted) return false;
                        return true;
                    }
                    else
                    {
                        var now = DateTime.Now;
                        while (m_task == null)
                        {
                            if (DateTime.Now > (entry + timeout)) return false;
                            Thread.Sleep(10);
                        }
                        return m_task.Wait(DateTime.Now.TimeTill( entry + timeout) );
                    }
                }
                else
                {
                    return m_task.Wait(timeout);
                }
            }
        }

        private IMainLogger m_logger = null;
        private Queue<TaskData> m_tasks = new Queue<TaskData>();
        private TaskData m_currentTaskData = null;
        private Task m_currentTask = null;
        private readonly object m_sync = new object();
        private uint m_finishedTasks = 0;

        public event EventHandler TaskEnded;
        public event EventHandler TaskStarted;

        public ITask CurrentTask { get { return m_currentTaskData; } }

        public uint FinishedTasks { get { return m_finishedTasks; } }

        public HostApplicationActionQueue(out IService serviceAccess) :
            base("HostApplicationActionQueue", out serviceAccess, typeof(IMainLogger))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_logger = manager.Get<IMainLogger>();
        }

        public ITask AddTask(string title, bool blockUI, System.Func<bool> precondition, TaskDelegate taskFunction)
        {
            lock (m_sync)
            {
                var data = new TaskData(title, blockUI, precondition, taskFunction);
                m_tasks.Enqueue(data);
                if (m_tasks.Count == 1)
                {
                    this.StartNext();
                }
                return data;
            }
        }

        private void StartNext()
        {
            lock (m_sync)
            {
                if (m_tasks.Count > 0 && m_currentTask == null)
                {
                    m_currentTaskData = m_tasks.Dequeue();
                    m_currentTask = new Task(new Action<object>(this.TaskCaller), m_currentTaskData);
                    m_currentTaskData.SetTaskReference(m_currentTask);
                    m_currentTaskData.SetTaskLogger(m_logger.Logger.RootLogger.LogEntering("Application Action", m_currentTaskData.Title));
                    m_currentTask.Start();
                }
            }
        }

        private void TaskCaller(object data)
        {
            try
            {
                this.TaskStarted?.Invoke(this, EventArgs.Empty);
                var taskData = data as TaskData;
                taskData.SetTaskLogger(m_logger.Logger.RootLogger.LogEntering("Starting Application Task", taskData.Title));
                taskData.RunTask();
            }
            finally
            {
                lock (m_sync)
                {
                    this.TaskEnded?.Invoke(this, EventArgs.Empty);
                    m_currentTask = null;
                    m_currentTaskData = null;
                    m_finishedTasks++;
                    this.StartNext();
                }
            }
        }
    }
}
