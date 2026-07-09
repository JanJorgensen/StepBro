using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace StepBro.Core.Host
{
    public interface IHostTaskHandler
    {
        enum Priority
        {
            Low,
            Normal,
            High
        }

        enum TaskHandlingAction
        {
            Continue,
            ContinueOnHostDomain = Continue,
            ContinueOnWorkerThreadDomain,
            Delay100ms,
            Delay500ms,
            Finish,
            Cancel,
        }

        enum TaskState
        {
            Init = 0,
            Work,
            Error,      // Unexpected.
            Failure,    // Faults in the task result.
            Finish
        }

        delegate TaskHandlingAction Task(ref TaskState state, ref int index, ITaskStateReporting reporting);

        enum StateChange { Idle, StartingNew, StillWorking }

        class StateChangedEventArgs : EventArgs
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

        event EventHandler<StateChangedEventArgs> StateChangeEvent;

        void AddTask(IHostTaskHandler.Task task, IHostTaskHandler.Priority priority, string workingText, string purposeText);
    }
}
