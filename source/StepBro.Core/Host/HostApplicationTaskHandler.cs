using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    public abstract class HostApplicationTaskHandler
    {
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

        private Queue<Tuple<TaskCaller, TaskStateProxy, string, string>> m_actions = new Queue<Tuple<TaskCaller, TaskStateProxy, string, string>>();
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

        public void AddTask<TState>(Task<TState> task, string workingText, string purposeText) where TState : struct, System.Enum
        {
            var caller = new TaskCaller<TState>(task);
            // TODO: Register task or make queue public somehow, to be able to show whats going on.

            var stateProxy = new TaskStateProxy(TaskExecutionState.AwaitingStartCondition, BreakOption.Stop);
            m_actions.Enqueue(new Tuple<TaskCaller, TaskStateProxy, string, string>(caller, stateProxy, workingText, purposeText));
            if (m_actions.Count == 1)
            {
                this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.StartingNew, workingText));
                RequestHostDomainHandling(this.HostDomainHandling);
            }
        }


        private void HostDomainHandling()
        {
            TaskHandling(false);
        }

        private void WorkerTaskHandling()
        {
            TaskHandling(true);
        }

        private void TaskHandling(bool isOnWorkerThread)
        {
            if (m_actions.Count > 0)
            {
                var task = m_actions.Peek();
                var caller = task.Item1;
                var stateReporter = task.Item2;

                var state = caller.CallTask(null);
                if (isOnWorkerThread)
                {
                    m_workerTask = null;
                }

                switch (state)
                {
                    case TaskAction.ContinueOnHostDomain:
                        RequestHostDomainHandling(this.HostDomainHandling);
                        break;
                    case TaskAction.ContinueOnWorkerThreadDomain:
                        m_workerTask = new System.Threading.Tasks.Task(this.WorkerTaskHandling);
                        m_workerTask.Start();
                        // Now get out of here without touching anything; the worker task will arrive in a moment!
                        break;
                    case TaskAction.Delay100ms:
                        break;
                    case TaskAction.Delay500ms:
                        break;
                    case TaskAction.Finish:
                    case TaskAction.Cancel:
                        m_actions.Dequeue();
                        if (m_actions.Count > 0)
                        {
                            this.StateChangeEvent?.Invoke(this, new StateChangedEventArgs(StateChange.StartingNew, m_actions.Peek().Item3));
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

        protected abstract void RequestHostDomainHandling(System.Action action);
    }
}
