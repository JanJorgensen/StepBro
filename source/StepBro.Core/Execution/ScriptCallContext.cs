using StepBro.Core.Data;
using StepBro.Core.Data.Report;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Execution
{
    internal class ScriptCallContext : IScriptCallContext, IProcedureThis
    {
        private ScriptTaskContext m_task;
        private ScriptCallContext m_parentContext;
        private ContextLogOption m_callLoggingOption;
        private IFileProcedure m_procedure;
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
        private int m_currentTestStepIndex = -1;
        private string m_currentTestStepTitle = null;
        private bool m_loggingEnabled;
        private int m_expectFailCount = 0;
        private readonly int m_errorCount = 0;
        private RuntimeErrorListener m_errorListener = null;
        private List<DataReport> m_currentReports = null;

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

            m_currentReports = parent.m_currentReports; // Simply inherit the list.

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
                return m_currentTestStepIndex;
            }
        }

        public string CurrentTestStepTitle
        {
            get
            {
                return m_currentTestStepTitle;
            }
        }

        public ICallContext EnterNewContext(string shortDescription, bool separateStateLevel)
        {
            throw new NotImplementedException();
        }

        public virtual IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption callerLoggingOption, bool isDynamicCall)
        {
            return new ScriptCallContext(this, procedure, callerLoggingOption, isDynamicCall);
        }

        public IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption callerLoggingOption, bool isDynamicCall)
        {
            return this.EnterNewScriptContext(procedure.ProcedureData, callerLoggingOption, isDynamicCall);
        }

        public IEnumerable<IFolderShortcut> GetFolders()
        {
            throw new NotImplementedException();
        }

        private string GetLocationDescription()
        {
            if (m_currentTestStepIndex > 0)
            {
                return m_procedure.FullName + " step " + ((m_currentTestStepTitle != null) ? ("'" + m_currentTestStepTitle + "'") : m_currentTestStepIndex.ToString());
            }
            else
            {
                return m_procedure.FullName + " - " + m_currentStatementLine.ToString();
            }
        }

        public DataReport AddReport(DataReport report)
        {
            if (m_currentReports == null) m_currentReports = new List<DataReport>();
            m_currentReports.Add(report);
            return report;
        }

        public bool RemoveReport(string id)
        {
            if (m_currentReports != null)
            {
                var report = m_currentReports.FirstOrDefault(r => String.Equals(id, r.ID, StringComparison.InvariantCulture));
                if (report != null)
                {
                    m_currentReports.Remove(report);
                    return true;
                }
                else { return false; }
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<DataReport> ListReports()
        {
            if (m_currentReports == null) yield break;
            else
            {
                foreach (var report in m_currentReports)
                {
                    yield return report;
                }
            }
        }

        private void AddToReports(ReportData data)
        {
            foreach (var report in m_currentReports)
            {
                report.AddData(data);
            }
        }

        public bool ReportError(ErrorID error = null, string description = "", Exception exception = null)
        {
            m_errorListener?.Invoke(m_procedure, m_currentStatementLine, error, description, exception);
            this.LogError(description);
            return false;
        }

        public void ReportExpectResult(string title, string expected, string actual, Verdict verdict)
        {
            if (verdict >= Verdict.Fail) m_expectFailCount++;
            if (m_currentReports != null)
            {
                this.AddToReports(new ExpectResultData(this.GetLocationDescription(), title, expected, actual, verdict));
            }
            if (String.IsNullOrEmpty(title))
            {
                m_loggerInside.Log(m_currentStatementLine.ToString(), $"EXPECT: {actual} => {verdict}; Expected: {expected}");
            }
            else
            {
                m_loggerInside.Log(m_currentStatementLine.ToString(), $"EXPECT \"{title}\": {actual} => {verdict}; Expected: {expected}");
            }
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
        string IProcedureThis.Name { get { return m_procedure.Name; } }

        public TaskManager TaskManager { get { return m_taskManager; } }
    }
}
