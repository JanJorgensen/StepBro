using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    internal class ScriptCallContext : IScriptCallContext, IProcedureThis
    {
        private ScriptTaskContext m_task;
        private ScriptCallContext m_parentContext;
        private ContextLogOption m_callLoggingOption;
        private IFileProcedure m_procedure = null;
        private readonly bool m_isDynamicCall;
        private TaskManager m_taskManager;
        private int m_fileLine = -1;
        private ILogger m_loggerOnEntry;
        private ILogger m_loggerInside;
        private ILoggerScope m_loggerInsideScope;
        private IExecutionScopeStatusUpdate m_statusUpdaterOnEntry;
        //Stack<ITaskStatusUpdate> m_createdStatusUpdaters = null;
        //ITaskStatusUpdate m_currentStatusUpdater = null;
        private int m_currentStatementLine = 0;
        private int m_currentStatementColumn = 0;
        private int m_currentTestStepIndex = 0;
        private string m_currentTestStepTitle = "";
        private bool m_loggingEnabled;
        private int m_expectFailCount = 0;
        private readonly int m_errorCount = 0;
        private RuntimeErrorListener m_errorListener = null;

        internal ScriptCallContext(
            ScriptTaskContext task,
            ILogger logger,
            ContextLogOption callLoggingOption,
            IExecutionScopeStatusUpdate statusUpdater,
            IFileProcedure procedure,
            TaskManager taskManager)
        {
            m_task = task;
            m_parentContext = null;
            m_callLoggingOption = callLoggingOption;
            m_loggerInside = m_loggerOnEntry = logger;
            m_statusUpdaterOnEntry = statusUpdater;
            m_loggingEnabled = true;    // Initial
            m_procedure = procedure;
            m_taskManager = taskManager;
            m_isDynamicCall = false;
            this.SetupFromProcedure();
        }

        internal ScriptCallContext(ScriptCallContext parent, IFileProcedure procedure, ContextLogOption callLoggingOption, bool isDynamicCall)
        {
            m_task = parent.m_task;
            m_parentContext = parent;

            m_taskManager = parent.m_taskManager;
            m_callLoggingOption = callLoggingOption;
            m_loggerOnEntry = parent.Logger;
            m_loggerInside = m_loggerOnEntry;
            m_statusUpdaterOnEntry = parent.StatusUpdater;
            m_loggingEnabled = parent.LoggingEnabled;
            m_errorListener = parent.m_errorListener;

            m_procedure = procedure;
            m_isDynamicCall = isDynamicCall;
            this.SetupFromProcedure();

            //m_createdlogger = logger.LogEntering(procedure.ElementName, procedure.Purpose);
            //if (separateStateLevel)
            //{
            //    m_currentStatusUpdater = parent.StatusUpdate.CreateSubTaskStatusReporter(procedure.Purpose);
            //    m_currentStatusUpdater.Disposed += M_currentStatusUpdater_Disposed;
            //    m_createdStatusUpdaters = new Stack<ITaskStatusUpdate>(4);
            //    m_createdStatusUpdaters.Push(m_currentStatusUpdater);
            //}
            //else
            //{
            //    m_currentStatusUpdater = parent.StatusUpdate;
            //}
        }

        private void SetupFromProcedure()
        {
            m_loggingEnabled = GetContextLoggingState(
                m_parentContext != null ? m_parentContext.m_loggingEnabled : true,
                m_loggerOnEntry.IsDebugging,
                m_callLoggingOption,
                m_procedure.LogOption);

            if (m_loggingEnabled || m_parentContext == null || m_parentContext.LoggingEnabled)
            {
                m_loggerInsideScope = m_loggerOnEntry.LogEntering((m_isDynamicCall ? "<DYNAMIC CALL> " : "") + m_procedure.FullName, "<arguments>");
                m_loggerInside = m_loggerInsideScope;
            }
            else
            {
                m_loggerInsideScope = null;
            }
        }

        public void InternalDispose()
        {
            m_statusUpdaterOnEntry.ClearSublevels();
            m_loggerInsideScope?.Dispose();
            m_loggerInside = null;
            m_loggerInsideScope = null;
            //m_createdlogger.Dispose();
            //if (m_firstCreatedStatusUpdater != null)
            //{
            //    m_firstCreatedStatusUpdater.Dispose();
            //}
        }

        public void Dispose()
        {
            throw new NotSupportedException("The ScriptCallContext is attempted disposed by an object without the authority to do so.");
        }

        public void SetErrorListener(RuntimeErrorListener listener)
        {
            m_errorListener = listener;
        }

        public static bool GetContextLoggingState(bool callerLogging, bool isDebugging, ContextLogOption callOption, ContextLogOption procedureOption)
        {
            // ForceAlways, Normal, DebugOnly, Disabled

            if (callOption == ContextLogOption.ForceAlways || procedureOption == ContextLogOption.ForceAlways)
            {
                return true;
            }
            else if (callOption == ContextLogOption.Disabled || procedureOption == ContextLogOption.Disabled)
            {
                return false;
            }
            else if (callOption == ContextLogOption.DebugOnly || procedureOption == ContextLogOption.DebugOnly)
            {
                return callerLogging && isDebugging;
            }
            else
            {
                return callerLogging;
            }
        }

        public CallEntry CurrentCallEntry
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IFileProcedure Self
        {
            get
            {
                return m_procedure;
            }
        }

        public IProcedureThis This { get { return (IProcedureThis)this; } }

        public int CurrentScriptFileLine
        {
            get
            {
                return m_fileLine;
            }
            set
            {
                m_fileLine = value;
            }
        }

        public ContextLogOption CallLoggingOption
        {
            get { return m_callLoggingOption; }
            set { m_callLoggingOption = value; }
        }

        public IHost HostApplication
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILogger Logger
        {
            get
            {
                return m_loggerInside;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return m_loggingEnabled;
            }
        }

        public IExecutionScopeStatusUpdate StatusUpdater
        {
            get
            {
                return m_statusUpdaterOnEntry;
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
                throw new NotImplementedException();
            }
        }

        public string CurrentTestStepTitle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            throw new NotImplementedException();
        }

        public IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption callerLoggingOption, bool isDynamicCall)
        {
            return new ScriptCallContext(this, procedure, callerLoggingOption, isDynamicCall);
        }

        public IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption callerLoggingOption, bool isDynamicCall)
        {
            //if (isDynamicCall)
            //{
            //    this.LogDetail("Dynamic Procedure Call");
            //}
            return new ScriptCallContext(this, procedure.ProcedureData, callerLoggingOption, isDynamicCall);
        }

        public IEnumerable<IFolderShortcut> GetFolders()
        {
            throw new NotImplementedException();
        }

        public bool ReportError(ErrorID error = null, string description = "", Exception exception = null)
        {
            m_errorListener?.Invoke(m_procedure, m_currentStatementLine, error, description, exception);
            this.LogError(description);
            return false;
        }

        public void ReportExpectResult(string title, string expression, bool success)
        {
            string result = success ? "PASS" : "FAIL";
            if (!success) m_expectFailCount++;
            if (String.IsNullOrEmpty(title))
            {
                m_loggerInside.Log(m_currentStatementLine.ToString(), $"EXPECT: {result}; {expression}");
            }
            else
            {
                m_loggerInside.Log(m_currentStatementLine.ToString(), $"EXPECT \"{title}\": {result}; {expression}");
            }
        }

        public void ReportExpectResult(string title, string expected, string actual, bool success)
        {
            throw new NotImplementedException();
        }

        public void EnterTestStep(int line, int column, int index, string title)
        {
            this.EnterStatement(line, column);
            m_currentTestStepIndex = index;
            m_currentTestStepTitle = title;
            if (this.LoggingEnabled)
            {
                if (String.IsNullOrWhiteSpace(title))
                {
                    m_loggerInside.Log(m_currentStatementLine.ToString(), String.Concat("STEP #", m_currentTestStepIndex.ToString()));
                }
                else
                {
                    m_loggerInside.Log(m_currentStatementLine.ToString(), String.Concat("STEP #", m_currentTestStepIndex.ToString(), " : " + title));
                }
            }
        }

        public void EnterStatement(int line, int column)
        {

            m_currentStatementLine = line;
            m_currentStatementColumn = column;
            if (this.Logger != null && this.Logger.IsDebugging && m_procedure.IsBreakpointOnLine(line))
            {
                throw new NotImplementedException();
            }
        }

        public void Log(string text)
        {
            m_loggerInside.Log(m_currentStatementLine.ToString(), text);
        }

        public void LogDetail(string text)
        {
            m_loggerInside.LogDetail(m_currentStatementLine.ToString(), text);
        }

        public void LogError(string text)
        {
            m_loggerInside.LogError(m_currentStatementLine.ToString(), text);
        }

        private TestResult CurrentProcedureResult { get; }

        TestResult IScriptCallContext.CurrentProcedureResult
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ILoadedFilesManager LoadedFiles
        {
            get
            {
                return m_task.LoadedFilesManager;
            }
        }

        bool IProcedureThis.HasFails { get { return m_expectFailCount > 0; } }

        bool IProcedureThis.HasErrors { get { return m_errorCount > 0; } }

        bool IProcedureThis.HasFailsOrErrors { get { return m_expectFailCount > 0 | m_errorCount > 0; } }

        ErrorID IProcedureThis.LastError { get { throw new NotImplementedException(); } }

        public TaskManager TaskManager { get { return m_taskManager; } }
    }
}
