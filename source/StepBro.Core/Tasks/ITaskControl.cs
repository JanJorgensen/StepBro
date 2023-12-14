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
