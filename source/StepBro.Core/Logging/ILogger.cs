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
        string Location { get; }
        ILoggerScope LogEntering(string location, string text);
        void Log(string text);
        void LogDetail(string text);
        void LogAsync(string text);
        void LogError(string text);
        void LogUserAction(string text);
        void LogSystem(string text);
    }

    public delegate string LoggerDynamicLocationSource();

    public interface ILoggerScope : ILogger, IDisposable
    {
        ILoggerScope LogEntering(string location, string text, LoggerDynamicLocationSource dynamicLocation);
        void EnteredParallelTask(string text);
        void LogExit(string text);
        object FirstLogEntryInScope { get; }
    }

    internal interface IProtectedLogger : ILoggerScope
    {
        void DisposeProtected();
        IProtectedLogger GetProtectedLogger();
    }
}
