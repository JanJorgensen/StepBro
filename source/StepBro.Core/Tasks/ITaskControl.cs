using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.ObjectMonitor;

namespace StepBro.Core.Tasks
{
    public interface ITaskControl
    {
        TaskExecutionState CurrentState { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        event EventHandler CurrentStateChanged;
        BreakOption BreakOptions { get; }

        bool RequestPause();
        bool RequestContinue();
        bool RequestStop();
        bool Kill();
    }

    public class TaskStateProxy : ITaskControl
    {
        private TaskExecutionState m_state;
        BreakOption m_breakOptions;
        BreakOption m_breakRequested = BreakOption.None;

        public TaskStateProxy(TaskExecutionState initialState, BreakOption breakOptions)
        {
            m_state = initialState;
            m_breakOptions = breakOptions;
        }

        public void NotifyStarted()
        {

        }

        public void NotifyFinished()
        {

        }

        #region ITaskControl interface

        TaskExecutionState ITaskControl.CurrentState { get { return m_state; } }

        DateTime ITaskControl.StartTime => throw new NotImplementedException();

        DateTime ITaskControl.EndTime => throw new NotImplementedException();

        BreakOption ITaskControl.BreakOptions { get { return m_breakOptions; } }

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

        bool ITaskControl.Kill()
        {
            m_breakRequested |= BreakOption.Kill;
            return true;
        }

        bool ITaskControl.RequestContinue()
        {
            m_breakRequested |= BreakOption.None;
            return true;
        }

        bool ITaskControl.RequestPause()
        {
            m_breakRequested |= BreakOption.Pause;
            return true;
        }

        bool ITaskControl.RequestStop()
        {
            m_breakRequested |= BreakOption.Stop;
            return true;
        }

        #endregion
    }

    public static class TaskControlSupport
    {
        public static bool Ended(this ITaskControl task)
        {
            switch (task.CurrentState)
            {
                case TaskExecutionState.Created:
                case TaskExecutionState.StartRequested:
                case TaskExecutionState.AwaitingStartCondition:
                case TaskExecutionState.Running:
                case TaskExecutionState.RunningNotResponding:
                case TaskExecutionState.PauseRequested:
                case TaskExecutionState.Paused:
                case TaskExecutionState.StopRequested:
                case TaskExecutionState.KillRequested:
                case TaskExecutionState.Terminating:
                    return false;
                case TaskExecutionState.ErrorStarting:
                case TaskExecutionState.Ended:
                case TaskExecutionState.EndedByException:
                default:
                    return true;
            }
        }
    }
}
