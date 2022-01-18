using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    internal class ScriptExecutionTask : IScriptExecution, ITaskControl, IExecutionResult
    {
        private Task m_task = null;
        private TaskExecutionState m_currentState = TaskExecutionState.Created;
        private readonly object m_sync = new object();
        private ILogger m_logger;
        //private readonly ILogSinkManager m_logSinkManager;
        private ScriptTaskContext m_taskContext;
        private ExecutionScopeStatusUpdaterMock m_statusUpdate;
        private readonly ILoadedFilesManager m_filesManager;
        private readonly TaskManager m_taskManager;
        private IFileElement m_targetElement;
        private readonly object[] m_arguments;
        private DateTime m_start = DateTime.MinValue;
        private DateTime m_end = DateTime.MinValue;
        private object m_returnValue = null;

        public ITaskControl Task { get { return this as ITaskControl; } }
        public IExecutionResult Result { get { return this as IExecutionResult; } }

        TaskExecutionState ITaskControl.CurrentState { get { return m_currentState; } }

        BreakOption ITaskControl.BreakOptions => throw new NotImplementedException();

        ProcedureResult IExecutionResult.ProcedureResult { get { return m_taskContext.Result; } }

        TimeSpan IExecutionResult.ExecutionTime { get { return m_end - m_start; } }

        object IExecutionResult.ReturnValue { get { return m_returnValue; } }

        public IFileElement TargetElement { get { return m_targetElement; } }

        DateTime ITaskControl.StartTime { get { return m_start; } }

        DateTime ITaskControl.EndTime { get { return m_end; } }

        public ScriptExecutionTask(
            ILogger logger,
            //ILogSinkManager logSinkManager,
            ILoadedFilesManager filesManager,
            TaskManager taskManager,
            IFileElement targetElement,
            object[] arguments)
        {
            m_logger = logger;
            //m_logSinkManager = logSinkManager;
            m_filesManager = filesManager;
            m_taskManager = taskManager;
            m_targetElement = targetElement;
            m_arguments = arguments;

            m_taskContext = new ScriptTaskContext();
            m_statusUpdate = new ExecutionScopeStatusUpdaterMock();
        }

        public void ExecuteSynchronous()
        {
            if (m_currentState != TaskExecutionState.Created) throw new InvalidOperationException("Execution has already been started.");

            this.SetState(TaskExecutionState.Started);

            this.ProcedureExecutionTask();
        }

        public void StartExecution()
        {
            lock (m_sync)
            {
                if (m_currentState != TaskExecutionState.Created) throw new InvalidOperationException("Execution has already been started.");
                m_task = new Task(this.AsyncExecutionHandler);
                m_task.Start();
                this.SetState(TaskExecutionState.Started);
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
            var logger = m_logger.LogEntering("Script Execution", m_targetElement.Name);
            m_taskContext.Setup(logger, ContextLogOption.Normal, m_statusUpdate, m_filesManager, m_taskManager);
            m_start = DateTime.Now;
            this.SetState(TaskExecutionState.Running);

            try
            {
                m_returnValue = m_taskContext.CallProcedure(m_targetElement as IFileProcedure, m_arguments);
                this.SetState(TaskExecutionState.Ended);
            }
            catch
            {
                this.SetState(TaskExecutionState.EndedByException);
            }
            finally
            {
                m_end = DateTime.Now;
                logger.LogExit("Script execution ended. " + m_taskContext.Result.ResultText(m_returnValue));
            }
        }

        bool ITaskControl.RequestPause()
        {
            m_statusUpdate.PauseRequested = true;
            return true;
        }

        bool ITaskControl.RequestContinue()
        {
            m_statusUpdate.PauseRequested = false;
            return true;
        }

        bool ITaskControl.RequestStop()
        {
            throw new NotImplementedException();
        }

        bool ITaskControl.Kill()
        {
            throw new NotImplementedException();
        }

        private class ExecutionScopeStatusUpdaterMock : StepBro.Core.Execution.IExecutionScopeStatusUpdate
        {
            private int m_level = 0;
            public int m_disposeCount = 0;
            public ExecutionScopeStatusUpdaterMock m_child = null;
            public string m_text = null;
            private bool m_pauseRequested = false;
            private DateTime m_entryTime;
            public TimeSpan m_expectedTime = default(TimeSpan);
            public long m_progressMax = -1;
            public long m_progress = -1;
            public long m_progressPokeCount = 0;
            public Func<long, string> m_progressFormatter = null;
            public List<Tuple<string, Func<bool, bool>>> m_buttons = new List<Tuple<string, Func<bool, bool>>>();

            public AttentionColor ProgressColor
            {
                get;
                set;
            }

            public bool PauseRequested
            {
                get { return m_pauseRequested; }
                set { m_pauseRequested = value; }
            }

            public event EventHandler Disposed;
            public event EventHandler ExpectedTimeExceeded;

            public void AddActionButton(string title, Func<bool, bool> activationAction)
            {
                System.Diagnostics.Debug.WriteLine("AddActionButton " + title);
                // TODO: Implement ....
                //throw new AccessViolationException();
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").AddActionButton: " + title);
                //m_buttons.Add(new Tuple<string, ButtonActivationType, Action<bool>>(title, type, activationAction));
            }

            public void ClearSublevels()
            {
                if (m_child != null)
                {
                    m_child.Dispose();
                }
            }

            public IExecutionScopeStatusUpdate CreateProgressReporter(string text = "", TimeSpan expectedTime = default(TimeSpan), long progressMax = -1L, Func<long, string> progressFormatter = null)
            {
                if (m_child != null) throw new Exception("Child status already active.");
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").CreateProgressReporter( " + text + " )");
                m_child = new ExecutionScopeStatusUpdaterMock();
                m_child.m_level = m_level + 1;
                m_child.m_text = text;
                m_entryTime = DateTime.Now;
                m_child.m_expectedTime = expectedTime;
                m_child.m_progressMax = progressMax;
                m_child.m_progressFormatter = progressFormatter;
                m_child.Disposed += this.M_child_Disposed;
                return m_child;
            }

            private void M_child_Disposed(object sender, EventArgs e)
            {
                if (Object.ReferenceEquals(sender, m_child))
                {
                    m_child.Disposed -= this.M_child_Disposed;
                    m_child = null;
                }
            }

            public void Dispose()
            {
                m_disposeCount++;
                if (m_disposeCount == 1)
                {
                    if (this.Disposed != null) this.Disposed(this, EventArgs.Empty);
                }
            }

            public void ProgressAliveSignal()
            {
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").ProgressAliveSignal");
                m_progressPokeCount++;
            }

            public void UpdateStatus(string text = null, long progress = -1)
            {
                if (text != null) m_text = text;
                if (progress >= 0) m_progress = progress;
                if (progress == 99999) this.ExpectedTimeExceeded?.Invoke(this, EventArgs.Empty);    // TODO
                //MiniLogger.Instance.Add(String.Format("TaskUpdate({0}).UpdateStatus: {1}, {2}", m_level, String.IsNullOrEmpty(text) ? "<no text>" : text, (progress >= 0) ? progress.ToString() : "<no progress>"));
            }

            public bool EnterPauseIfRequested(string state)
            {
                throw new NotImplementedException();
            }

            public void ProgressSetup(long start, long length, Func<long, string> toText)
            {
                throw new NotImplementedException();
            }
        }
    }
}
