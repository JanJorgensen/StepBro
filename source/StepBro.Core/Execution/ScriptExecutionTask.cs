﻿using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    internal class ScriptExecutionTask : IScriptExecution, ITaskControl, IExecutionResult
    {
        private Task m_task = null;
        private TaskExecutionState m_currentState = TaskExecutionState.Created;
        private readonly object m_sync = new object();
        private ILoggerScope m_loggerOuterScope;
        private ILoggerScope m_loggerExecutionScope = null;
        private ScriptTaskContext m_taskContext;
        private ExecutionScopeStatusUpdater m_statusUpdateRoot;
        private ObservableCollection<IExecutionScopeStatus> m_executionStateStack;
        private ReadOnlyObservableCollection<IExecutionScopeStatus> m_executionStateStackReadOnly;
        private readonly ILoadedFilesManager m_filesManager;
        private readonly TaskManager m_taskManager;
        private readonly string m_targetTitle;
        private IFileProcedure m_targetProcedure;
        private readonly object[] m_arguments;
        private object m_returnValue = null;

        public ITaskControl Task { get { return this as ITaskControl; } }
        public ReadOnlyObservableCollection<IExecutionScopeStatus> StateStack
        {
            get
            {
                if (m_executionStateStackReadOnly == null)
                {
                    m_executionStateStackReadOnly = new ReadOnlyObservableCollection<IExecutionScopeStatus>(m_executionStateStack);
                }
                return m_executionStateStackReadOnly;
            }
        }

        public IExecutionResult Result { get { return this as IExecutionResult; } }
        public DataReport Report { get { return m_taskContext?.Report; } }

        TaskExecutionState ITaskControl.CurrentState { get { return m_currentState; } }

        BreakOption ITaskControl.BreakOptions => throw new NotImplementedException();

        ProcedureResult IExecutionResult.ProcedureResult { get { return m_taskContext.Result; } }

        TimeSpan IExecutionResult.ExecutionTime { get { return m_loggerExecutionScope.LastLogEntryInScope.Timestamp - m_loggerExecutionScope.FirstLogEntryInScope.Timestamp; } }

        Exception IExecutionResult.Exception { get { return m_taskContext.ExecutionExeception; } }

        object IExecutionResult.ReturnValue { get { return m_returnValue; } }

        public IFileElement TargetElement { get { return m_targetProcedure; } }

        DateTime ITaskControl.StartTime { get { return m_loggerExecutionScope.FirstLogEntryInScope.Timestamp; } }

        DateTime ITaskControl.EndTime { get { return m_loggerExecutionScope.LastLogEntryInScope.Timestamp; } }

        public LogEntry FirstLogEntry { get { return (LogEntry)m_loggerExecutionScope.FirstLogEntryInScope; } }
        public LogEntry LastLogEntry { get { return (LogEntry)m_loggerExecutionScope.LastLogEntryInScope; } }

        public ScriptExecutionTask(
            ILoggerScope logger,
            ILoadedFilesManager filesManager,
            TaskManager taskManager,
            IFileProcedure targetProcedure,
            string targetTitle,
            object[] arguments)
        {
            m_loggerOuterScope = logger;
            m_filesManager = filesManager;
            m_taskManager = taskManager;
            m_targetProcedure = targetProcedure;
            m_targetTitle = targetTitle;
            m_arguments = arguments;

            m_taskContext = new ScriptTaskContext();
            m_executionStateStack = new ObservableCollection<IExecutionScopeStatus>();
            m_statusUpdateRoot = new ExecutionScopeStatusUpdater(null, m_executionStateStack, 0, Constants.STEPBRO_SCRIPT_EXECUTION_LOG_LOCATION, default(TimeSpan), -1L, null);
        }

        public void ExecuteSynchronous()
        {
            if (m_currentState != TaskExecutionState.Created) throw new InvalidOperationException("Execution has already been started.");

            this.SetState(TaskExecutionState.StartRequested);

            this.ProcedureExecutionTask();
        }

        public void StartExecution()
        {
            lock (m_sync)
            {
                if (m_currentState != TaskExecutionState.Created) throw new InvalidOperationException("Execution has already been started.");
                m_task = new Task(this.AsyncExecutionHandler);
                m_task.Start();
                this.SetState(TaskExecutionState.StartRequested);
            }
        }

        private void AsyncExecutionHandler()
        {
            this.ProcedureExecutionTask();
        }

        public event EventHandler CurrentStateChanged;

        private void SetState(TaskExecutionState state)
        {
            lock (m_sync)
            {
                if (state != m_currentState)
                {
                    System.Diagnostics.Debug.WriteLine($"Script execution changed to {state} state");
                    m_currentState = state;
                    this.CurrentStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void ProcedureExecutionTask()
        {
            m_loggerExecutionScope = m_loggerOuterScope.LogEntering(LogEntry.Type.TaskEntry, Constants.STEPBRO_SCRIPT_EXECUTION_LOG_LOCATION, (m_targetTitle != null) ? m_targetTitle : m_targetProcedure.FullName, null);
            m_taskContext.Setup(m_loggerExecutionScope, ContextLogOption.Normal, m_statusUpdateRoot, m_filesManager, m_taskManager);
            this.SetState(TaskExecutionState.Running);

            m_returnValue = m_taskContext.CallProcedure(m_targetProcedure as IFileProcedure, m_arguments);
            this.SetState((m_taskContext.ExecutionExeception == null) ? TaskExecutionState.Ended : TaskExecutionState.EndedByException);
            m_statusUpdateRoot.Dispose();
            m_loggerExecutionScope.LogExit("Script execution ended. " + m_taskContext.Result.ResultText(m_returnValue));
        }

        bool ITaskControl.RequestPause()
        {
            //m_statusUpdate.PauseRequested = true;
            return true;
        }

        bool ITaskControl.RequestContinue()
        {
            //m_statusUpdate.PauseRequested = false;
            return true;
        }

        bool ITaskControl.RequestStop()
        {
            m_taskContext.RequestStop();
            return true;
        }

        bool ITaskControl.Kill()
        {
            throw new NotImplementedException();
        }
    }
}
