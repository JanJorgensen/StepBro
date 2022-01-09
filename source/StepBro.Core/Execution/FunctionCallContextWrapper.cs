using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    internal class FunctionCallContextWrapper : IScriptCallContext
    {
        private IScriptCallContext m_context;
        public FunctionCallContextWrapper(IScriptCallContext context, int line, int column)
        {
            m_context = context;
        }

        public ContextLogOption CallLoggingOption
        {
            get
            {
                return ContextLogOption.Disabled;
            }
        }

        public CallEntry CurrentCallEntry
        {
            get
            {
                return CallEntry.Subsequent;
            }
        }

        public ProcedureResult Result
        {
            get
            {
                return m_context.Result;
            }
        }

        public int CurrentScriptFileLine
        {
            get
            {
                return m_context.CurrentScriptFileLine;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int CurrentTestStepIndex
        {
            get
            {
                return m_context.CurrentTestStepIndex;
            }
        }

        public string CurrentTestStepTitle
        {
            get
            {
                return m_context.CurrentTestStepTitle;
            }
        }

        public bool DebugBreakIsSet
        {
            get
            {
                return m_context.DebugBreakIsSet;
            }
        }

        public IHost HostApplication
        {
            get
            {
                return m_context.HostApplication;
            }
        }

        public ILoadedFilesManager LoadedFiles
        {
            get
            {
                return m_context.LoadedFiles;
            }
        }

        public ILogger Logger
        {
            get
            {
                return m_context.Logger;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return false;
            }
        }

        public IFileProcedure Self
        {
            get
            {
                return m_context.Self;
            }
        }

        public IProcedureThis This { get { return m_context.This; } }


        public IExecutionScopeStatusUpdate StatusUpdater
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TaskManager TaskManager => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            throw new NotImplementedException();
        }

        public IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall, object[] arguments)
        {
            throw new NotImplementedException();
        }

        public IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall, object[] arguments)
        {
            throw new NotImplementedException();
        }

        public void EnterStatement(int line, int column)
        {
        }

        public void EnterTestStep(int line, int column, int index, string title)
        {
        }

        public IEnumerable<IFolderShortcut> GetFolders()
        {
            return m_context.GetFolders();
        }

        public void InternalDispose()
        {
        }

        public void Log(string text)
        {
        }

        public void LogDetail(string text)
        {
        }

        public void LogError(string text)
        {
        }

        public bool ReportParsingError(ErrorID error = null, string description = "", Exception exception = null)
        {
            return true;
        }

        public void ReportExpectResult(string title, string expected, string actual, Verdict verdict)
        {
            m_context.ReportExpectResult(title, expected, actual, verdict);
        }

        public bool SetResultFromSub(IScriptCallContext sub)
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
    }
}
