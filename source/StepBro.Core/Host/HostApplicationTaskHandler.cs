using StepBro.Core.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    public class HostApplicationTaskHandler : IHostTaskHandler
    {

        private struct TaskData
        {
            public TaskData(IHostTaskHandler.Task task, IHostTaskHandler.Priority priority, string workingText, string purposeText)
            {
                this.task = task;
                this.priority = priority;
                this.workingText = workingText;
                this.purposeText = purposeText;
            }
            public IHostTaskHandler.Task task;
            public IHostTaskHandler.Priority priority;
            public string workingText;
            public string purposeText;
        }

        private class SingleThreadContext() : SynchronizationContext
        {
            public override void Send(SendOrPostCallback task, object state)
            {
                task(state);
            }
            public override void Post(SendOrPostCallback task, object state)
            {
                task(state);
            }
        }

        private SynchronizationContext m_synchronizationContext = null;
        private Queue<TaskData> m_actions = new Queue<TaskData>();
        private IHostTaskHandler.TaskState m_taskState = IHostTaskHandler.TaskState.Init;
        private TaskStateProxy m_taskStateAccess = null;
        private int m_taskIndexValue = 0;
        private IHostTaskHandler.TaskHandlingAction m_currentHandlingAction = IHostTaskHandler.TaskHandlingAction.Continue;
        private DateTime m_currentActionTimerExpiryTime = DateTime.MinValue;
        private System.Threading.Tasks.Task m_workerTask = null;
        private object m_sync = new object();

        public HostApplicationTaskHandler(SynchronizationContext mainContext)
        {
            m_synchronizationContext = mainContext;
        }

        public event EventHandler<IHostTaskHandler.StateChangedEventArgs> StateChangeEvent;

        public void AddTask(IHostTaskHandler.Task task, IHostTaskHandler.Priority priority, string workingText, string purposeText)
        {
            // TODO: Register task or make queue public somehow, to be able to show whats going on.

            bool runAction = false;
            lock (m_sync)
            {
                runAction = (m_actions.Count == 0);
                m_actions.Enqueue(new TaskData(task, priority, workingText, purposeText));
            }
            if (runAction)
            {
                m_taskState = IHostTaskHandler.TaskState.Init;
                this.StateChangeEvent?.Invoke(this, new IHostTaskHandler.StateChangedEventArgs(IHostTaskHandler.StateChange.StartingNew, workingText));
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
                if (m_currentHandlingAction == IHostTaskHandler.TaskHandlingAction.Delay100ms || m_currentHandlingAction == IHostTaskHandler.TaskHandlingAction.Delay500ms)
                {
                    if (DateTime.UtcNow < m_currentActionTimerExpiryTime)
                    {
                        m_currentHandlingAction = IHostTaskHandler.TaskHandlingAction.ContinueOnHostDomain;
                        RequestHostDomainHandling(this.HostDomainHandling);
                        Thread.Sleep(10);   // TODO: Create an OS timer to do this instead.
                        return;
                    }
                }

                m_currentHandlingAction = m_actions.Peek().task(ref m_taskState, ref m_taskIndexValue, null);
                if (isOnWorkerThread)
                {
                    m_workerTask = null;
                }

                switch (m_currentHandlingAction)
                {
                    case IHostTaskHandler.TaskHandlingAction.ContinueOnHostDomain:
                        RequestHostDomainHandling(this.HostDomainHandling);
                        break;
                    case IHostTaskHandler.TaskHandlingAction.ContinueOnWorkerThreadDomain:
                        m_workerTask = new System.Threading.Tasks.Task(this.WorkerTaskHandling, null);
                        m_workerTask.Start();
                        // Now get out of here without touching anything; the worker task will arrive in a moment!
                        break;
                    case IHostTaskHandler.TaskHandlingAction.Delay100ms:
                    case IHostTaskHandler.TaskHandlingAction.Delay500ms:
                        m_currentActionTimerExpiryTime = DateTime.UtcNow + ((m_currentHandlingAction == IHostTaskHandler.TaskHandlingAction.Delay100ms) ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMilliseconds(500));
                        RequestHostDomainHandling(this.HostDomainHandling);
                        break;
                    case IHostTaskHandler.TaskHandlingAction.Finish:
                    case IHostTaskHandler.TaskHandlingAction.Cancel:
                        m_actions.Dequeue();
                        if (m_actions.Count > 0)
                        {
                            m_taskState = IHostTaskHandler.TaskState.Init;
                            this.StateChangeEvent?.Invoke(this, new IHostTaskHandler.StateChangedEventArgs(IHostTaskHandler.StateChange.StartingNew, m_actions.Peek().workingText));
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
                                this.StateChangeEvent?.Invoke(this, new IHostTaskHandler.StateChangedEventArgs(IHostTaskHandler.StateChange.Idle, "Idle"));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                this.StateChangeEvent?.Invoke(this, new IHostTaskHandler.StateChangedEventArgs(IHostTaskHandler.StateChange.Idle, "Idle"));
            }
        }

        protected void RequestHostDomainHandling(SendOrPostCallback action, object state = null)
        {
            m_synchronizationContext.Post(action, state);
        }
    }
}
