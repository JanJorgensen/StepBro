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
        ICallContext m_parentContext;
        CallContext m_childContext = null;
        CallEntry m_callEntry;
        bool m_break;
        ILogger m_entryLogger;

        public CallContext(CallContext parent)
        {
            m_parentContext = parent;
            m_entryLogger = parent.Logger;
            m_callEntry = parent.CurrentCallEntry;
            m_break = false;
        }

        public CallContext(ScriptCallContext parent, CallEntry callEntry, bool breakSet)
        {
            m_parentContext = parent;
            m_entryLogger = parent.Logger;
            m_callEntry = callEntry;
            m_break = breakSet;
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
                return m_parentContext.Logger;
            }
        }

        public bool LoggingEnabled
        {
            get { throw new NotImplementedException(); }
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

        public int CurrentTestStepIndex
        {
            get
            {
                return m_parentContext.CurrentTestStepIndex;
            }
        }

        public string CurrentTestStepTitle
        {
            get
            {
                return m_parentContext.CurrentTestStepTitle;
            }
        }

        public TaskManager TaskManager => throw new NotImplementedException();

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            if (m_childContext != null)
            {
                throw new Exception();
            }
            m_childContext = new CallContext(this);
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
            return m_parentContext.GetFolders();
        }

        public bool ReportError(ErrorID error = null, string description = "", Exception exception = null)
        {
            throw new NotImplementedException();
        }

        public void ReportExpectResult(string title, string expression, bool success)
        {
            throw new NotImplementedException();
        }

        public void ReportExpectResult(string title, string expected, string actual, bool success)
        {
            throw new NotImplementedException();
        }
    }
}
