﻿using System;
using System.Collections.Generic;

using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Host;
using StepBro.Core.Tasks;

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
    }
}
