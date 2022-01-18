using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Data;
using StepBro.Core.Host;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    internal class CallContext : ICallContext
    {
        private ScriptCallContext m_parentScriptContext;
        private ICallContext m_parentContext;
        private ILoggerScope m_logger;
        private CallContext m_childContext = null;
        private CallEntry m_callEntry;
        private bool m_break;

        public CallContext(CallContext parent, string location)
        {
            m_parentScriptContext = parent.m_parentScriptContext;
            m_parentContext = parent;
            m_logger = parent.m_logger.LogEntering(location, null);
            m_callEntry = parent.CurrentCallEntry;
            m_break = false;
        }

        public CallContext(ScriptCallContext parent, CallEntry callEntry, bool breakSet, string location)
        {
            m_parentScriptContext = parent;
            m_parentContext = parent;
            m_logger = parent.Logger.LogEntering(location, null);
            m_callEntry = callEntry;
            m_break = breakSet;
            //if (String.IsNullOrEmpty(locationPrefix)) m_locationIdentification = parent.ShortLocationDescription();
            //else m_locationIdentification = parent.ShortLocationDescription() + "." + location;
        }

        internal event EventHandler Disposing;

        public void Dispose()
        {
            if (m_parentContext != null)
            {
                if (this.Disposing != null) this.Disposing(this, EventArgs.Empty);

                if (m_childContext != null)
                {
                    m_childContext.Dispose();
                    m_childContext = null;
                }

                m_parentContext = null;
            }
        }

        internal ICallContext ParentContext { get { return m_parentContext; } }

        public CallEntry CurrentCallEntry
        {
            get
            {
                return m_callEntry;
            }
        }

        public IHost HostApplication
        {
            get
            {
                return m_parentContext.HostApplication;
            }
        }

        public ILogger Logger
        {
            get
            {
                return m_logger;
            }
        }

        public bool LoggingEnabled
        {
            get { return m_parentContext.LoggingEnabled; }
        }

        public IExecutionScopeStatusUpdate StatusUpdater
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool DebugBreakIsSet
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //public int CurrentTestStepIndex
        //{
        //    get
        //    {
        //        return m_parentContext.CurrentTestStepIndex;
        //    }
        //}

        //public string CurrentTestStepTitle
        //{
        //    get
        //    {
        //        return m_parentContext.CurrentTestStepTitle;
        //    }
        //}

        public TaskManager TaskManager { get { return m_parentContext.TaskManager; } }

        public ICallContext EnterNewContext(string location, bool separateStateLevel)
        {
            if (m_childContext != null)
            {
                throw new Exception();
            }
            m_childContext = new CallContext(this, location);
            m_childContext.Disposing += M_childContext_Disposing;
            return m_childContext;
        }

        private void M_childContext_Disposing(object sender, EventArgs e)
        {
            m_childContext.Disposing -= M_childContext_Disposing;
            m_childContext = null;
        }

        public IEnumerable<IFolderShortcut> GetFolders()
        {
            return m_parentScriptContext.GetFolders();
        }

        public bool ReportParsingError(ErrorID error = null, string description = "", Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public void ReportExpectResult(string title, string expected, string actual, Verdict verdict)
        {
            throw new NotImplementedException();
        }

        public void ReportFailure(string failureDescription, ErrorID id = null)
        {
            m_parentScriptContext.ReportFailure(failureDescription, id);
        }

        public void ReportError(string errorDescription, ErrorID id = null, Exception exception = null)
        {
            m_parentScriptContext.ReportError(errorDescription, id, exception);
        }
    }
}
