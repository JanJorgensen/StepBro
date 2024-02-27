using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogger
    {
        bool IsDebugging { get; }
        /// <summary>
        /// Get the current location identification for the logging.
        /// </summary>
        string Location { get; }
        /// <summary>
        /// Enters a new logging scope that is a sub scope of this scope.
        /// </summary>
        /// <param name="location">The name of the location inside the new scope.</param>
        /// <param name="text">Additional log text for the scope entering.</param>
        /// <returns>A logger to use for the scope.</returns>
        ILoggerScope LogEntering(string location, string text);
        /// <summary>
        /// Create a new scope for a location, but without adding the entering to the log.
        /// </summary>
        /// <param name="name">The name of the new scope.</param>
        /// <returns>A logger to use for the scope.</returns>
        ILoggerScope CreateSubLocation(string name);
        ILogEntry Log(string text);
        void LogDetail(string text);
        void LogAsync(string text);
        void LogError(string text);
        void LogUserAction(string text);
        void LogSystem(string text);
    }

    public delegate string LoggerDynamicLocationSource();

    public interface ILoggerScope : ILogger, IDisposable
    {
        /// <summary>
        /// Enters a new logging scope that is a sub scope of this scope and where the location string of each log entry can be different.
        /// </summary>
        /// <param name="isHighLevel">Whether the entered scope is to be considered a high level scope in the execution.</param>
        /// <param name="location">The name of the location inside the new scope.</param>
        /// <param name="text">Additional log text for the scope entering.</param>
        /// <param name="dynamicLocation">A delegate to be used for getting the location string for each new log entry within the scope.</param>
        /// <returns>A logger to use for the scope.</returns>
        ILoggerScope LogEntering(bool isHighLevel, string location, string text, LoggerDynamicLocationSource dynamicLocation);
        void EnteredParallelTask(string text);
        void LogExit(string text);
        ILogEntry FirstLogEntryInScope { get; }
        /// <summary>
        /// Indicates whether any errors were logged directly in this scope.
        /// </summary>
        bool ErrorsLogged { get; }
    }

    internal interface IProtectedLogger : ILoggerScope
    {
        void DisposeProtected();
        IProtectedLogger GetProtectedLogger();
    }
}
