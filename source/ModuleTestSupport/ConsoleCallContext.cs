using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModuleTestSupport
{
    public class ConsoleCallContext : ICallContext, ILogger
    {
        public CallEntry CurrentCallEntry => throw new NotImplementedException();

        public int CurrentTestStepIndex => throw new NotImplementedException();

        public string CurrentTestStepTitle => throw new NotImplementedException();

        public IHost HostApplication => throw new NotImplementedException();

        public ILogger Logger { get { return this as ILogger; } }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        public IExecutionScopeStatusUpdate StatusUpdater => throw new NotImplementedException();

        public bool DebugBreakIsSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TaskManager TaskManager => throw new NotImplementedException();

        bool ILogger.IsDebugging
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            return this;
        }

        public IEnumerable<IFolderShortcut> GetFolders()
        {
            throw new NotImplementedException();
        }

        public bool ReportError(ErrorID error = null, string description = "", Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public void ReportExpectResult(string title, string expected, string actual, Verdict verdict)
        {
            throw new NotImplementedException();
        }

        void ILogger.Log(string location, string text)
        {
            var t = new StringBuilder();
            t.Append("Log ");
            if (!String.IsNullOrEmpty(location))
            {
                if (!String.IsNullOrEmpty(text))
                {
                    t.Append(location);
                    t.Append(" - ");
                    t.Append(text);
                }
                else
                {
                    t.Append(location);
                }
            }
            else if (!String.IsNullOrEmpty(text))
            {
                t.Append(text);
            }
            else
            {
                t.Append("<nothing>");
            }
            Console.WriteLine(t.ToString());
        }

        void ILogger.LogDetail(string location, string text)
        {
            throw new NotImplementedException();
        }

        ILoggerScope ILogger.LogEntering(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogError(string location, string text)
        {
            ((ILogger)this).Log("Error " + location, text);
        }

        void ILogger.LogSystem(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogUserAction(string location, string text)
        {
            throw new NotImplementedException();
        }
    }
}
