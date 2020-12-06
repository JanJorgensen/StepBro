using System;

namespace StepBro.Core.Execution
{
    /// <summary>
    /// Feedback interface for methods or tasks that awaits some specific event.
    /// </summary>
    public interface IAwaitAction
    {
        /// <summary>
        /// Used by the event trigger to report the awaited event.
        /// </summary>
        /// <returns>Whether the event was still awaited (if false, it was canceled).</returns>
        bool ReportEvent();
        /// <summary>
        /// Event that signals the event trigger to be cancelled.
        /// </summary>
        event EventHandler CancelEvent;
        /// <summary>
        /// Indicates whether the event trigger has been cancelled.
        /// </summary>
        bool Cancelled { get; }
    }
}
