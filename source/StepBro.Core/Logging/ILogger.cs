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
        ILoggerScope LogEntering(string location, string text);
        void Log(string location, string text);
        void LogDetail(string location, string text);
        void LogError(string location, string text);
        void LogUserAction(string location, string text);
        void LogSystem(string location, string text);
    }

    public interface ILoggerScope : ILogger, IDisposable
    {
        void EnteredParallelTask(string location, string text);
        void LogExit(string location, string text);
        object FirstLogEntryInScope { get; }
    }

    internal interface IProtectedLogger : ILoggerScope
    {
        void DisposeProtected();
        IProtectedLogger GetProtectedLogger();
    }
}
