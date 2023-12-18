using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Data
{
    internal class UICallContext : ICallContext
    {
        private readonly ILogger m_logger;
        private readonly TaskManager m_taskManager;

        public UICallContext(ServiceManager serviceManager)
        {
            m_logger = serviceManager.Get<ILogger>();
            m_taskManager = serviceManager.Get<TaskManager>();
        }

        public CallEntry CurrentCallEntry => throw new NotImplementedException();

        public int CurrentTestStepIndex => throw new NotImplementedException();

        public string CurrentTestStepTitle => throw new NotImplementedException();

        public IHost HostApplication => throw new NotImplementedException();

        public ILogger Logger
        {
            get { return m_logger; }
        }

        public bool LoggingEnabled { get { return true; } }

        public IExecutionScopeStatusUpdate StatusUpdater => throw new NotImplementedException();

        public bool DebugBreakIsSet => throw new NotImplementedException();

        public TaskManager TaskManager { get { return m_taskManager; } }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFolderShortcut> ListShortcuts()
        {
            throw new NotImplementedException();
        }

        public bool ReportParsingError(ErrorID error = null, string description = "", Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public void ReportExpectResult(string title, string expected, Verdict verdict)
        {
            throw new NotImplementedException();
        }

        public void ReportFailure(string failureDescription, ErrorID id = null)
        {
            throw new NotImplementedException();
        }

        public void ReportError(string errorDescription, ErrorID id = null, Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public bool StopRequested() { throw new NotImplementedException(); }
    }
}
