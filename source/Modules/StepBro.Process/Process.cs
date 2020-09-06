using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using StepBro.Core.Logging;
using StepBro.Core.Api;

namespace StepBro.Process
{
    public sealed class Process : ITaskControl
    {
        private System.Diagnostics.Process m_process;
        private ILogger m_logger;
        private TaskExecutionState m_state;
        private DateTime m_lastRefresh = DateTime.MinValue;
        private object m_sync = new object();

        public event EventHandler CurrentStateChanged;

        private Process(ILogger logger, System.Diagnostics.Process process)
        {
            m_process = process;
            m_logger = logger;
            m_state = TaskExecutionState.Started;
            process.Exited += Process_Exited;
            //process.ErrorDataReceived += Process_ErrorDataReceived;
            //process.OutputDataReceived += Process_OutputDataReceived;
        }

        private void SetState(TaskExecutionState state)
        {
            lock (m_sync)
            {
                if (state != m_state)
                {
                    m_state = state;
                    this.CurrentStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public System.Diagnostics.Process DotNetProcess { get { return m_process; } }

        private void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
        }

        private void Process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            m_process.Exited -= Process_Exited;
            this.SetState(TaskExecutionState.Ended);
            //m_process.ErrorDataReceived -= Process_ErrorDataReceived;
            //m_process.OutputDataReceived -= Process_OutputDataReceived;
        }

        private void ForceRefresh()
        {
            lock (m_sync)
            {
                m_lastRefresh = DateTime.UtcNow;
                m_process.Refresh();
            }
        }

        private void RefreshIfNecessary()
        {
            lock (m_sync)
            {
                DateTime now = DateTime.UtcNow;
                if (now > (m_lastRefresh + TimeSpan.FromMilliseconds(300)))
                {
                    m_lastRefresh = now;
                    m_process.Refresh();
                    switch (m_state)
                    {
                        case TaskExecutionState.AwaitingStartCondition:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            else if (m_process.Responding)
                            {
                                this.SetState(TaskExecutionState.Running);
                            }
                            break;
                        case TaskExecutionState.Running:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            else if (!m_process.Responding)
                            {
                                this.SetState(TaskExecutionState.RunningNotResponding);
                            }
                            break;
                        case TaskExecutionState.RunningNotResponding:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            else if (m_process.Responding)
                            {
                                this.SetState(TaskExecutionState.Running);
                            }
                            break;
                        case TaskExecutionState.PauseRequested:
                            break;
                        case TaskExecutionState.Paused:
                            break;
                        case TaskExecutionState.StopRequested:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            break;
                        case TaskExecutionState.KillRequested:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            break;
                        case TaskExecutionState.Terminating:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            break;
                        case TaskExecutionState.Ended:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool IsRunning { get { lock (m_sync) { this.RefreshIfNecessary(); return !m_process.HasExited; } } }
        public int ID { get { return m_process.Id; } }
        public string ExecutablePath { get { return "c:\\temp"; } }
        public TimeSpan TotalExecutionTime { get { this.RefreshIfNecessary(); return m_process.TotalProcessorTime; } }
        public DateTime StartTime { get { return m_process.StartTime; } }
        public DateTime EndTime { get { return m_process.ExitTime; } }


        public TaskExecutionState CurrentState
        {
            get
            {
                lock (m_sync)
                {

                    this.RefreshIfNecessary();
                    return TaskExecutionState.Running;
                }
            }
        }

        public BreakOption BreakOptions
        {
            get
            {
                return BreakOption.Stop | BreakOption.Kill;
            }
        }

        public bool AwaitStart(ILogger logger = null, TimeSpan timeout = default(TimeSpan))
        {
            if (m_state == TaskExecutionState.Started)
            {
                DateTime entryTime = DateTime.UtcNow;

                this.SetState(TaskExecutionState.AwaitingStartCondition);

                if (m_process.WaitForInputIdle((int)timeout.TotalMilliseconds))
                {
                    this.SetState(TaskExecutionState.Running);
                }
                else
                {
                    this.SetState(TaskExecutionState.Started);
                    return false;
                }



                this.ForceRefresh();

                while (m_process.MainWindowHandle == IntPtr.Zero)
                {
                    //context.Log("Waiting for started process to open a main window.");

                    TimeSpan time = DateTime.UtcNow - entryTime;
                    if (time > timeout)
                    {
                        //context.ReportFailureOrError(new TimeoutWaitingForInputIdleError(), String.Format("Process did not open a main window in {0} seconds.", timeout));
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                    m_process.Refresh();
                }


            }
            else
            {
                throw new Exception("Process is not in the right state (" + m_state.ToString() + ") for an 'AwaitStart' operation.");
            }
            return false;
        }

        public bool RequestPause()
        {
            return false;   // Request denied; processes don't support pause.
        }

        public bool RequestContinue()
        {
            return false;   // Request denied; processes don't support pause.
        }

        public bool RequestStop()
        {
            switch (m_state)
            {
                case TaskExecutionState.Started:
                case TaskExecutionState.AwaitingStartCondition:
                case TaskExecutionState.Running:
                case TaskExecutionState.RunningNotResponding:
                case TaskExecutionState.PauseRequested:
                case TaskExecutionState.Paused:
                    this.SetState(TaskExecutionState.StopRequested);
                    if (m_process.CloseMainWindow())
                    {

                    }
                    else
                    {

                    }
                    break;
                case TaskExecutionState.StopRequested:
                case TaskExecutionState.KillRequested:
                case TaskExecutionState.Terminating:
                case TaskExecutionState.Ended:
                    //default:
                    break;
            }
            return false;
        }

        public bool Kill()
        {
            switch (m_state)
            {
                case TaskExecutionState.Started:
                case TaskExecutionState.AwaitingStartCondition:
                case TaskExecutionState.Running:
                case TaskExecutionState.RunningNotResponding:
                case TaskExecutionState.PauseRequested:
                case TaskExecutionState.Paused:
                case TaskExecutionState.StopRequested:
                case TaskExecutionState.Terminating:
                    // Simply kill the process.
                    throw new NotImplementedException();
                //break;
                case TaskExecutionState.KillRequested:
                case TaskExecutionState.Ended:
                    //default:
                    break;
            }
            return false;
        }

        public static Process Start(ILogger logger = null, string filename = "", string arguments = "")
        {
            return ObjectMonitorManager.Register(
                new Process(logger, System.Diagnostics.Process.Start(filename, arguments)));
        }

        public static Process Start(
            [Implicit] StepBro.Core.Execution.ICallContext context,
            string filename,
            string arguments = "",
            TimeSpan startTimeout = new TimeSpan(),
            TimeSpan exitTimeout = new TimeSpan()
            // bool moveWindow = false
            )
        {
            var process = ObjectMonitorManager.Register(
                new Process((ILogger)context, System.Diagnostics.Process.Start(filename, arguments)));

            if (startTimeout > TimeSpan.Zero)
            {
                if (!process.AwaitStart(context.Logger, startTimeout))
                {
                    //context.RegisterResult()
                    return process;
                }
            }

            return process;
        }
    }
}


//Run(Static)
//  Execute a commandline program/script
//Start Application(Static)
//  Start a local windows application, optionally waiting for it to open a new window.
//Stop Application(Static)
//  Stop application and wait for it to finish.
//Query process(Static)
//  Retrieve information about a process running on the local machine.
//Open Document (Static)
//  Open a document using the default handler for that document type.
//Run Command(Static)
//  Execute a commandline program/script
