using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    public class HostApplicationTaskHandler
    {

        public enum Priority
        {
            Low,
            Normal,
            High
        }

        public enum TaskAction
        {
            Continue,
            ContinueOnHostDomain = Continue,
            ContinueOnWorkerThreadDomain,
            Delay100ms,
            Delay500ms,
            Finish,
            Cancel,
        }

        private struct TaskData
        {
            public TaskData(TaskCaller caller, TaskStateProxy state, Priority priority, string workingText, string purposeText)
            {
                this.caller = caller;
                this.state = state;
                this.priority = priority;
                this.workingText = workingText;
                this.purposeText = purposeText;
            }
            public TaskCaller caller;
            public TaskStateProxy state;
            public Priority priority;
            public string workingText;
            public string purposeText;
        }

        public delegate TaskAction Task<TState>(ref TState state, ref int index, ITaskStateReporting reporting);

        private abstract class TaskCaller
        {
            public abstract TaskAction CallTask(ITaskStateReporting reporting);
        }

        private class TaskCaller<TState> : TaskCaller where TState : struct, System.Enum
        {
            private Task<TState> m_task;
            private TState m_state;
            private int m_index;

            public TaskCaller(Task<TState> task)
            {
                m_task = task;
                m_state = default(TState);
            }

            public override TaskAction CallTask(ITaskStateReporting reporting)
            {
                return m_task(ref m_state, ref m_index, reporting);
            }
        }

        private SynchronizationContext m_synchronizationContext = null;
        private Queue<TaskData> m_actions = new Queue<TaskData>();
        private TaskAction m_currentAction = TaskAction.Continue;
        private DateTime m_currentActionTimerExpiryTime = DateTime.MinValue;
        private System.Threading.Tasks.Task m_workerTask = null;

        public enum StateChange { Idle, StartingNew, StillWorking }

        public class StateChangedEventArgs : EventArgs
        {
            private StateChange m_change;
            private string m_workingText;
            public StateChangedEventArgs(StateChange change, string workingText)
            {
                m_change = change;
                m_workingText = workingText;
            }
            public StateChange State { get { return m_change; } }
            public string WorkingText { get { return m_workingText; } }
        }

        public event EventHandler<StateChangedEventArgs> StateChangeEvent;

        public void AddTask<TState>(Task<TState> task, Priority priority, string workingText, string purposeText) where TState : struct, System.Enum
        {
            var caller = new TaskCaller<TState>(task);
            // TODO: Register task or make queue public somehow, to be able to show whats going on.

            var stateProxy = new TaskStateProxy(TaskExecutionState.AwaitingStartCondition, BreakOption.Stop);
            m_actions.Enqueue(new TaskData(caller, stateProxy, priority, workingText, purposeText));
            if (m_actions.Count == 1)
            {
                this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.StartingNew, workingText));
                RequestHostDomainHandling(this.HostDomainHandling);
            }
        }

        public bool AnyTasks()
        {
            return (m_actions.Count != 0);
        }

        private void HostDomainHandling(object state)
        {
            TaskHandling(false);
        }

        private void WorkerTaskHandling(object state)
        {
            TaskHandling(true);
        }

        private void TaskHandling(bool isOnWorkerThread)
        {
            if (m_actions.Count > 0)
            {
                if (m_currentAction == TaskAction.Delay100ms || m_currentAction == TaskAction.Delay500ms)
                {
                    if (DateTime.UtcNow < m_currentActionTimerExpiryTime)
                    {
                        m_currentAction = TaskAction.ContinueOnHostDomain;
                        RequestHostDomainHandling(this.HostDomainHandling);
                        Thread.Sleep(10);   // TODO: Create an OS timer to do this instead.
                        return;
                    }
                }
                var task = m_actions.Peek();
                var caller = task.caller;
                var stateReporter = task.state;

                m_currentAction = caller.CallTask(null);
                if (isOnWorkerThread)
                {
                    m_workerTask = null;
                }

                switch (m_currentAction)
                {
                    case TaskAction.ContinueOnHostDomain:
                        RequestHostDomainHandling(this.HostDomainHandling);
                        break;
                    case TaskAction.ContinueOnWorkerThreadDomain:
                        m_workerTask = new System.Threading.Tasks.Task(this.WorkerTaskHandling, null);
                        m_workerTask.Start();
                        // Now get out of here without touching anything; the worker task will arrive in a moment!
                        break;
                    case TaskAction.Delay100ms:
                    case TaskAction.Delay500ms:
                        m_currentActionTimerExpiryTime = DateTime.UtcNow + ((m_currentAction == TaskAction.Delay100ms) ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMilliseconds(500));
                        RequestHostDomainHandling(this.HostDomainHandling);
                        break;
                    case TaskAction.Finish:
                    case TaskAction.Cancel:
                        m_actions.Dequeue();
                        if (m_actions.Count > 0)
                        {
                            this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.StartingNew, m_actions.Peek().workingText));
                            RequestHostDomainHandling(this.HostDomainHandling);
                        }
                        else
                        {
                            if (isOnWorkerThread)
                            {
                                // Jump to host domain, to finish (send state update event).
                                RequestHostDomainHandling(this.HostDomainHandling);
                            }
                            else
                            {
                                this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.Idle, "Idle"));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.Idle, "Idle"));
            }
        }

        protected void RequestHostDomainHandling(SendOrPostCallback action, object state = null)
        {
            if (m_synchronizationContext == null)
            {
                m_synchronizationContext = SynchronizationContext.Current;
            }
            m_synchronizationContext.Post(action, state);
        }
    }
}
