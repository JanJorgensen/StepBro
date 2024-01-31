using StepBro.Core.Api;

namespace StepBro.Core.Tasks
{
    /// <summary>
    /// Enumerates the different execution states of a task.
    /// </summary>
    [Public]
    public enum TaskExecutionState
    {
        /// <summary>
        /// The initial state where the process/task has been created but not started yet.
        /// </summary>
        Created,
        /// <summary>
        /// Process start requested, but start condition has not been checked.
        /// </summary>
        StartRequested,
        /// <summary>
        /// As the Started state, but a request to await the start conditions has been made.
        /// </summary>
        AwaitingStartCondition,
        ErrorStarting,
        /// <summary>
        /// Process  is confirmed to be running.
        /// </summary>
        Running,
        /// <summary>
        /// Process should be running, but is currently not responding.
        /// </summary>
        RunningNotResponding,
        PauseRequested,
        Paused,
        StopRequested,
        KillRequested,
        Terminating,
        Ended,
        EndedByException
    }

    public static class TaskExecutionStateHelpers
    {
        static public bool HasEnded(this TaskExecutionState state)
        {
            switch (state)
            {
                case Core.Tasks.TaskExecutionState.Created:
                case Core.Tasks.TaskExecutionState.StartRequested:
                case Core.Tasks.TaskExecutionState.AwaitingStartCondition:
                case Core.Tasks.TaskExecutionState.Running:
                case Core.Tasks.TaskExecutionState.RunningNotResponding:
                case Core.Tasks.TaskExecutionState.PauseRequested:
                case Core.Tasks.TaskExecutionState.Paused:
                case Core.Tasks.TaskExecutionState.StopRequested:
                case Core.Tasks.TaskExecutionState.KillRequested:
                case Core.Tasks.TaskExecutionState.Terminating:
                default:
                    return false;

                case Core.Tasks.TaskExecutionState.ErrorStarting:
                case Core.Tasks.TaskExecutionState.Ended:
                case Core.Tasks.TaskExecutionState.EndedByException:
                    return true;
            }
        }
    }
}
