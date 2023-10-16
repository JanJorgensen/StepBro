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
using System.Text;

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
        private DateTime m_startTime;
        private DateTime m_endTime = DateTime.MaxValue;
        private int m_fileLine = -1;
        private ILoggerScope m_loggerOnEntry;
        private ILogger m_loggerInside;
        private ILoggerScope m_loggerInsideScope;
        private IExecutionScopeStatusUpdate m_statusUpdaterOnEntry;
        private Stack<IExecutionScopeStatusUpdate> m_statusUpdaterStack = new Stack<IExecutionScopeStatusUpdate>();
        private int m_currentStatementLine = 0;
        private int m_currentStatementColumn = 0;
        private string m_currentLogLocation = "0";
        private int m_currentTestStepIndex = -1;
        private string m_currentTestStepTitle = null;
        private bool m_loggingEnabled;
        private Verdict m_verdict = Verdict.Unset;
        private int m_failureLine = -1;
        private string m_failureDescription = null;
        private ErrorID m_failureID = null;
        private Exception m_errorException = null;
        private int m_failCount = 0;
        private int m_errorCount = 0;
        private RuntimeErrorListener m_errorListener = null;
        private DataReport m_currentReport = null;
        private ProcedureResult m_lastCallResult = null;
        private string m_nextCallHighLevelType = null;
        private IFolderShortcutsSource m_folderShortcuts = null;
        private string m_loopExitReason = null;

        internal ScriptCallContext(
            ScriptTaskContext task,
            ILoggerScope logger,
            ContextLogOption callLoggingOption,
            IExecutionScopeStatusUpdate statusUpdater,
            IFileProcedure procedure,
            TaskManager taskManager,
            object[] arguments)
        {
            m_task = task;
            m_parentContext = null;
            m_callLoggingOption = callLoggingOption;
            m_loggerInside = m_loggerOnEntry = logger;
            m_statusUpdaterOnEntry = statusUpdater;
            m_statusUpdaterStack.Push(m_statusUpdaterOnEntry);
            m_loggingEnabled = true;    // Initial
            m_procedure = procedure;
            m_taskManager = taskManager;
            m_isDynamicCall = false;
            this.SetupFromProcedure(arguments);
        }

        internal ScriptCallContext(
            ScriptCallContext parent,
            IFileProcedure procedure,
            ContextLogOption callLoggingOption,
            bool isDynamicCall,
            object[] arguments)
        {
            m_task = parent.m_task;
            m_parentContext = parent;

            m_taskManager = parent.m_taskManager;
            m_callLoggingOption = callLoggingOption;
            m_loggerOnEntry = parent.Logger as ILoggerScope;
            m_loggerInside = m_loggerOnEntry;
            m_statusUpdaterOnEntry = parent.StatusUpdater;
            m_statusUpdaterStack.Push(m_statusUpdaterOnEntry);
            m_loggingEnabled = parent.LoggingEnabled;
            m_errorListener = parent.m_errorListener;

            if (Object.ReferenceEquals(procedure, parent.m_procedure))
            {
                m_folderShortcuts = parent.m_folderShortcuts;
            }

            m_procedure = procedure;
            m_isDynamicCall = isDynamicCall;
            this.SetupFromProcedure(arguments);

            m_currentReport = parent.m_currentReport;
        }

        private void SetupFromProcedure(object[] arguments)
        {
            m_loggingEnabled = GetContextLoggingState(
                m_parentContext != null ? m_parentContext.m_loggingEnabled : true,
                m_loggerOnEntry.IsDebugging,
                m_callLoggingOption,
                m_procedure.LogOption);

            if (m_loggingEnabled || m_parentContext == null || m_parentContext.LoggingEnabled)
            {
                StringBuilder argText = new StringBuilder();
                if (arguments != null && arguments.Length > 0)
                {
                    argText.Append("( ");
                    var args = new List<string>();
                    foreach (var a in arguments)
                    {
                        args.Add(StringUtils.ObjectToString(a));
                    }
                    argText.Append(String.Join(", ", args));
                    argText.Append(" )");
                }
                else
                {
                    argText.Append("<no arguments>");
                }
                var isHighlevel = m_parentContext?.m_nextCallHighLevelType != null;
                var textPrefix = isHighlevel ? (m_parentContext.m_nextCallHighLevelType + " - ") : "";
                m_loggerInsideScope = m_loggerOnEntry.LogEntering(
                    isHighlevel,
                    (m_isDynamicCall ? "<DYNAMIC CALL> " : "") + m_procedure.FullName,
                    textPrefix + argText.ToString(),
                    new LoggerDynamicLocationSource(this.GetDynamicLogLocation));
                if (isHighlevel)
                {
                    m_statusUpdaterStack.Push(m_statusUpdaterStack.Peek().CreateProgressReporter(m_parentContext.m_nextCallHighLevelType + ": " + m_procedure.FullName));
                }
                m_loggerInside = m_loggerInsideScope;
                m_startTime = m_loggerInsideScope.FirstLogEntryInScope.Timestamp;   // Same timestamp as the loggeg entry.
            }
            else
            {
                m_loggerInsideScope = null;
                m_startTime = DateTime.Now;
            }
        }

        public void InternalDispose()
        {
            m_statusUpdaterOnEntry.ClearSublevels();
            m_loggerInsideScope?.Dispose();
            m_loggerInside = null;
            m_loggerInsideScope = null;
            m_endTime = DateTime.Now;
        }

        public void Dispose()
        {
            throw new NotSupportedException("The ScriptCallContext is attempted disposed by an object without the authority to do so.");
        }

        internal Logger GetRootLogger()
        {
            return (m_loggerInside as LoggerScope).Logger;
        }

        public string GetDynamicLogLocation()
        {
            return m_currentLogLocation;
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
                //return m_statusUpdaterStack.Peek();
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
            var newContext = new CallContext(this, CallEntry.Subsequent, false, this.GetDynamicLogLocation() + " " + shortDescription);
            return newContext;
        }

        public virtual IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption callerLoggingOption, bool isDynamicCall, object[] arguments)
        {
            try
            {
                return new ScriptCallContext(this, procedure, callerLoggingOption, isDynamicCall, arguments);
            }
            finally
            {
                m_nextCallHighLevelType = null;
            }
        }

        public IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption callerLoggingOption, bool isDynamicCall, object[] arguments)
        {
            return this.EnterNewScriptContext(procedure.ProcedureData, callerLoggingOption, isDynamicCall, arguments);
        }

        public IEnumerable<IFolderShortcut> ListShortcuts()
        {
            if (m_folderShortcuts == null)
            {
                var commonShortcuts = ServiceManager.Global.Get<IFolderManager>();
                m_folderShortcuts = new FolderCollection(FolderShortcutOrigin.ScriptFile, commonShortcuts, m_procedure.ParentFile.FolderShortcuts);
            }
            return m_folderShortcuts.ListShortcuts();
        }

        public string ShortLocationDescription()
        {
            if (m_currentStatementLine > 0)
            {
                return m_currentStatementLine.ToString();
            }
            else
            {
                return m_procedure.Name;
            }
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

        public void SetNextProcedureAsHighLevel(string type)
        {
            m_nextCallHighLevelType = type;
        }

        #region Report

        public void AddReport(DataReport report)
        {
            if (m_currentReport != null)
            {
                throw new OperationNotAllowedException("A report is already registered.");
            }
            m_currentReport = report;
        }

        public void RemoveReport()
        {
            if (m_currentReport == null)
            {
                throw new ArgumentException("No report registered.");
            }
            m_currentReport = null;
        }

        public DataReport GetReport()
        {
            if (m_currentReport == null)
            {
                throw new Exception("No report is registered.");
            }
            return m_currentReport;
        }

        public DataReport TryGetReport()
        {
            return m_currentReport;
        }

        private void AddToReport(ReportData data)
        {
            if (m_currentReport != null)
            {
                m_currentReport.AddData(this, data);
            }
        }

        #endregion

        public bool ReportParsingError(ErrorID error = null, string description = "", Exception exception = null)
        {
            m_errorListener?.Invoke(m_procedure, m_currentStatementLine, error, description, exception);
            this.LogError(description);
            return false;
        }

        public void ReportExpectResult(string title, string expected, Verdict verdict)
        {
            string resultDescription;
            string actual = null;
            actual = this.ExpectStatementValue;
            if (this.IsSimpleExpectStatementWithValue && !String.IsNullOrEmpty(actual))
            {
                if (String.IsNullOrEmpty(title))
                {
                    resultDescription = $"EXPECT: {expected}; Value: {actual}, Verdict: {verdict}";
                }
                else
                {
                    resultDescription = $"EXPECT {title}: {expected}; Value: {actual}, Verdict: {verdict}";
                }
            }
            else
            {
                if (String.IsNullOrEmpty(title))
                {
                    resultDescription = $"EXPECT: {expected}; Verdict: {verdict}";
                }
                else
                {
                    resultDescription = $"EXPECT {title}: {expected}; Verdict: {verdict}";
                }
            }

            if (verdict == Verdict.Error)
            {
                this.ReportError(resultDescription, null, null);
            }
            else if (verdict >= Verdict.Fail)
            {
                this.ReportFailure(resultDescription);
            }
            else
            {
                m_loggerInside.Log(resultDescription);
                this.SetPassVerdict();  // To indicate that the procedure actually has a verdict set now.
            }

            this.AddToReport(new ExpectResultData(this.GetLocationDescription(), title, expected, actual, verdict));
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
                    m_loggerInside.Log(String.Concat("STEP #", m_currentTestStepIndex.ToString()));
                }
                else
                {
                    m_loggerInside.Log(String.Concat("STEP #", m_currentTestStepIndex.ToString(), " : " + title));
                }
            }
        }

        public void EnterStatement(int line, int column)
        {
            m_currentStatementLine = line;
            m_currentLogLocation = line.ToString();   // TODO: Add "Line " to the text.
            m_currentStatementColumn = column;
            if (this.Logger != null && this.Logger.IsDebugging && m_procedure.IsBreakpointOnLine(line))
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSimpleExpectStatementWithValue { get; set; }

        public string ExpectStatementValue { get; set; }

        public void LogStatement(string text)
        {
            m_currentLogLocation = m_currentStatementLine.ToString() + " Log";
            m_loggerInside.Log(text);
            m_currentLogLocation = m_currentStatementLine.ToString();
        }

        public void Log(string text)
        {
            m_loggerInside.Log(text);
        }

        public void LogDetail(string text)
        {
            m_loggerInside.LogDetail(text);
        }

        public void LogError(string text)
        {
            m_loggerInside.LogError(text);
        }

        public void SetPassVerdict()
        {
            if (m_verdict == Verdict.Unset)
            {
                m_verdict = Verdict.Pass;
            }
        }

        public void ReportFailure(string failureDescription, ErrorID id = null)
        {
            m_failCount++;
            if (m_verdict <= Verdict.Pass)  // Only override if no failures reported before.
            {
                m_verdict = Verdict.Fail;
                m_failureDescription = failureDescription;
                m_failureLine = m_currentStatementLine;
                m_failureID = id;
            }
            this.LogError(failureDescription);
        }

        public void ReportError(string errorDescription, ErrorID id = null)
        {
            this.ReportError(errorDescription, id, null);
        }

        public void ReportError(string errorDescription, ErrorID id = null, Exception ex = null)
        {
            m_errorCount++;
            if (m_verdict <= Verdict.Fail)  // Only override if no errors reported before.
            {
                m_verdict = Verdict.Error;
                m_failureDescription = errorDescription;
                m_failureLine = m_currentStatementLine;
                m_failureID = id;
                m_errorException = ex;
                m_errorListener?.Invoke(m_procedure, m_currentStatementLine, id, errorDescription, ex);
            }
            if (ex != null)
            {
                this.LogError(errorDescription + (errorDescription.EndsWith(".") ? " " : ".") + "\n\t" + ex.GetType().Name + " - " + ex.Message + "\n" + ex.StackTrace);
            }
            else
            {
                this.LogError(errorDescription);
            }
        }

        public bool SetResultFromSub(IScriptCallContext sub)
        {
            m_lastCallResult = sub.Result;
            bool setVerdict = false;
            if ((this.Self.Flags & ProcedureFlags.NoSubResultInheritance) == ProcedureFlags.NoSubResultInheritance)
            {
                if (sub.Result.Verdict > Verdict.Fail)
                {
                    setVerdict = true;
                }
            }
            else
            {
                if (sub.Result.Verdict > m_verdict)
                {
                    setVerdict = true;
                }
            }
            if (setVerdict)
            {
                m_verdict = sub.Result.Verdict;
                if (sub.Result.Verdict >= Verdict.Fail)
                {
                    m_failureLine = m_currentStatementLine;
                    m_failureID = sub.Result.ErrorID;
                    m_failureDescription = $"Failure in called procedure \"{sub.Self.FullName}\".";

                    if (m_verdict == Verdict.Error) return true;
                    else if (m_verdict == Verdict.Fail)
                    {
                        return (m_procedure.Flags & ProcedureFlags.ContinueOnFail) == ProcedureFlags.None;
                    }
                }
            }
            return false;
        }

        public ProcedureResult Result
        {
            get
            {
                return new ProcedureResult(m_procedure.FullName, m_failureLine, m_currentTestStepTitle, m_verdict, m_failureDescription, m_failureID, m_startTime, m_endTime);
            }
        }

        public ILoadedFilesManager LoadedFiles
        {
            get
            {
                return m_task.LoadedFilesManager;
            }
        }
        public void SetLoopExitReason(string reason)
        {
            m_loopExitReason = reason;
        }

        bool IProcedureThis.HasFails { get { return m_failCount > 0; } }

        bool IProcedureThis.HasErrors { get { return m_errorCount > 0; } }

        bool IProcedureThis.HasFailsOrErrors { get { return m_failCount > 0 | m_errorCount > 0; } }

        ErrorID IProcedureThis.LastError { get { throw new NotImplementedException(); } }
        string IProcedureThis.Name { get { return m_procedure.Name; } }

        ProcedureResult IProcedureThis.LastCallResult { get { return m_lastCallResult; } }

        bool IProcedureThis.SetResult(Verdict verdict, string description)
        {
            if (m_verdict <= Verdict.Pass && verdict > m_verdict)
            {
                m_verdict = verdict;
                m_failureDescription = description;
                return true;
            }
            else
            {
                return false;
            }
        }
        string IProcedureThis.LastLoopExitReason
        {
            get { return m_loopExitReason; }
        }


        string IProcedureThis.FileName
        {
            get
            {
                return m_procedure.ParentFile.FileName;
            }
        }
        string IProcedureThis.FileDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(m_procedure.ParentFile.FilePath);
            }
        }

        public TaskManager TaskManager { get { return m_taskManager; } }
    }
}
