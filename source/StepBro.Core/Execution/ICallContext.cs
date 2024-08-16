using System;
using System.Collections.Generic;

using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Host;
using StepBro.Core.Tasks;
using System.Threading;

namespace StepBro.Core.Execution
{
    /// <summary>
    /// Context interface to allow code module methods to access functionality from the script execution and the StepBro system.
    /// </summary>
    public interface ICallContext : IDisposable, IFolderShortcutsSource
    {
        /// <summary>
        /// Gets an indication whether the current method has been called earlier from the the same script line.
        /// </summary>
        CallEntry CurrentCallEntry { get; }

        bool ReportParsingError(ErrorID error = null, string description = "", Exception exception = null);


        void ReportFailure(string failureDescription, ErrorID id = null);
        void ReportError(string errorDescription, ErrorID id = null, Exception exception = null);
        void ReportExpectResult(string title, string expected, Verdict verdict);

        /// <summary>
        /// Creates a new named call context.
        /// </summary>
        /// <param name="location">A descriptive identification of the static or dynamic location of the entered context/scope.</param>
        /// <param name="separateStateLevel">Whether the new context should be displayed in a new progress/state level./</param>
        /// <remarks>The new call context must be disposed upon exit of the context scope.</remarks>
        /// <returns>Reference to a new call context object.</returns>
        ICallContext EnterNewContext(string location, bool separateStateLevel);

        /// <summary>
        /// Gets a reference to an object representing the host application.
        /// </summary>
        IHost HostApplication { get; }

        /// <summary>
        /// Interface to the logger to use in the current context.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Indicates whether normal logging is enabled in the current context.
        /// </summary>
        bool LoggingEnabled { get; }

        /// <summary>
        /// Interface to the status update for the current context.
        /// </summary>
        IExecutionScopeStatusUpdate StatusUpdater { get; }

        /// <summary>
        /// Indicates user wish to break debugging inside the called code module method.
        /// </summary>
        /// <remarks>To use this flag, the user must attach a dot net debugger to the StepBro host application.</remarks>
        bool DebugBreakIsSet { get; }

        TaskManager TaskManager { get; }

        /// <summary>
        /// Indicates whether the user or the host application has requested to stop the script execution.
        /// </summary>
        /// <returns>Whether to stop execution.</returns>
        bool StopRequested();
    }


    public static class ContextExtensions
    {
        /// <summary>
        /// Helper method to wait for at task to finish, while checking whether the task should be cancelled.
        /// </summary>
        /// <param name="context">The current call context.</param>
        /// <param name="task">The task to await completion for.</param>
        /// <param name="timeout">The maximum time to wait for completion, before cancelling the task.</param>
        /// <param name="cancellation">A cancellation token source to use for cancelling the task.</param>
        /// <returns><code>true</code> if the task did complete, and <code>false</code> if it was cancelled.</returns>
        static public bool Await(this ICallContext context, System.Threading.Tasks.Task task, TimeSpan timeout, CancellationTokenSource cancellation)
        {
            var logger = (context != null) ? context.Logger : StepBro.Core.Main.Logger.RootLogger;
            var start = DateTime.UtcNow;
            while (true)
            {
                if (task.IsCompleted) return true;
                var stopRequested = (context != null) ? context.StopRequested() : false;
                if (DateTime.UtcNow - start > timeout || stopRequested)
                {
                    if (stopRequested)
                    {
                        logger.LogError("Operation stopped by user.");
                    }
                    else
                    {
                        logger.LogError("Operation timeout.");
                    }
                    
                    try
                    {
                        cancellation.Cancel();
                        task.Wait(cancellation.Token);
                    }
                    finally { }
                    
                    return false;
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
            }
        }
    }
}
