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
}
