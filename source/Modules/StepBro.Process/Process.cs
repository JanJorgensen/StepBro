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
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using StepBro.Core.File;

namespace StepBro.Process
{
    public sealed class Process : ITaskControl, INameable, IDisposable
    {
        private class OSProcess : System.Diagnostics.Process
        {
            public StepBro.Process.Process m_parent = null;
        }

        private System.Diagnostics.Process m_process;
        private ILogger m_logger;
        private TaskExecutionState m_state;
        private DateTime m_lastRefresh = DateTime.MinValue;
        private object m_sync = new object();
        private LogLineLineReader m_processOutputReader = null;
        private object m_processOutputLogSync = new object();
        private LogLineData m_firstProcessOutputLine = null;
        private LogLineData m_lastProcessOutputLine = null;
        private bool m_hasErrorOutput = false;

        public event EventHandler CurrentStateChanged;

        private Process(ILogger logger, System.Diagnostics.Process process)
        {
            m_process = process;
            m_logger = logger;
            m_state = TaskExecutionState.StartRequested;
            m_process.Exited += Process_Exited;
            m_process.StartInfo.UseShellExecute = false;
            m_process.StartInfo.RedirectStandardOutput = true;
            m_process.OutputDataReceived += OsProcess_OutputDataReceived;
            m_process.StartInfo.RedirectStandardError = true;
            m_process.ErrorDataReceived += OsProcess_ErrorDataReceived;
        }

        public void Dispose()
        {
            if (m_process != null)
            {
                m_process.Dispose();
            }
            m_logger = null;
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

        public ILineReader OutputLines
        {
            get
            {
                if (m_processOutputReader == null)
                {
                    System.Diagnostics.Debug.WriteLine("Process.OutputLines is null !!");
                }
                return m_processOutputReader;
            }
        }

        public bool HasErrorOutput {  get { return m_hasErrorOutput; } }

        //public static Process Start(ILogger logger = null, string filename = "", string arguments = "")
        //{
        //    var osProcess = new OSProcess();
        //    osProcess.StartInfo.FileName = filename;
        //    if (!String.IsNullOrEmpty(arguments))
        //    {
        //        osProcess.StartInfo.Arguments = arguments;
        //    }
        //    return ObjectMonitorManager.Register(
        //        new Process(logger, System.Diagnostics.Process.Start(filename, arguments)));
        //}

        public static Process Start(
            [Implicit] StepBro.Core.Execution.ICallContext context,
            string filename,
            string arguments = "",
            bool useShell = false,
            TimeSpan startTimeout = new TimeSpan(),
            TimeSpan exitTimeout = new TimeSpan()
            // bool moveWindow = false
            )
        {
            if (context != null)
            {
                filename = FileReferenceUtils.ResolveShortcutPath(context.ListShortcuts(), filename);
            }

            var osProcess = new OSProcess();
            osProcess.StartInfo.FileName = filename;
            if (!String.IsNullOrEmpty(arguments))
            {
                osProcess.StartInfo.Arguments = arguments;
            }
            osProcess.StartInfo.UseShellExecute = useShell;

            if (context != null && context.LoggingEnabled)
            {
                if (String.IsNullOrEmpty(arguments))
                {
                    context.Logger.Log($"Starting process: {filename}");
                }
                else
                {
                    context.Logger.Log($"Starting process: {filename} {arguments}");
                }
            }

            ObjectMonitorManager.Register(osProcess);
            var process = new Process(context.Logger, osProcess);
            osProcess.m_parent = process;

            process.SetState(TaskExecutionState.StartRequested);
            try
            {
                osProcess.Start();
                osProcess.BeginErrorReadLine();
                osProcess.BeginOutputReadLine();
                if (startTimeout > TimeSpan.Zero)
                {
                    if (!process.AwaitStart(context, startTimeout))
                    {
                        //context.RegisterResult()
                        return process;
                    }
                }
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError("Error starting process.", exception: ex);
                }
                process.SetState(TaskExecutionState.ErrorStarting);
            }

            return process;
        }

        private static void OsProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            var line = e.Data;
            if (line != null)
            {
                var process = (sender as OSProcess).m_parent;
                process.LogProcessOutput(false, line);
            }
        }

        private static void OsProcess_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            var process = (sender as OSProcess).m_parent;
            process.m_hasErrorOutput = true;
            var line = e.Data;
            if (line != null)
            {
                process.LogProcessOutput(true, line);
            }
        }

        private void LogProcessOutput(bool error, string line)
        {
            lock (m_processOutputLogSync)
            {
                m_lastProcessOutputLine = new LogLineData(m_lastProcessOutputLine, error ? LogLineData.LogType.Error : LogLineData.LogType.Neutral, 0, line);
                if (m_processOutputReader == null)
                {
                    m_firstProcessOutputLine = m_lastProcessOutputLine;
                    m_processOutputReader = new LogLineLineReader(this, m_firstProcessOutputLine, m_processOutputLogSync);
                }
                m_processOutputReader.NotifyNew(m_lastProcessOutputLine);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Process Exited");
            m_process.Exited -= Process_Exited;
            this.SetState(TaskExecutionState.Ended);
        }

        public int ExitCode {  get { return m_process.ExitCode; } }

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
                        case TaskExecutionState.Created:
                        case TaskExecutionState.StartRequested:
                        case TaskExecutionState.AwaitingStartCondition:
                        case TaskExecutionState.Running:
                        case TaskExecutionState.RunningNotResponding:
                            if (m_process.HasExited)
                            {
                                this.SetState(TaskExecutionState.Ended);
                            }
                            else if (m_process.Responding)
                            {
                                this.SetState(TaskExecutionState.Running);
                            }
                            else
                            {
                                this.SetState(TaskExecutionState.RunningNotResponding);
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
                    return m_state;
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

        public string Name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool AwaitStart([Implicit] StepBro.Core.Execution.ICallContext context, TimeSpan timeout = default(TimeSpan))
        {
            if (m_state <= TaskExecutionState.AwaitingStartCondition)
            {
                DateTime entryTime = DateTime.UtcNow;

                if (context != null && context.LoggingEnabled)
                {
                    context.Logger.Log("Waiting for started process to enter idle state.");
                }
                if (m_process.WaitForInputIdle((int)timeout.TotalMilliseconds))
                {
                    if (context != null && context.LoggingEnabled)
                    {
                        context.Logger.Log("Process Idle.");
                    }
                }
                else
                {
                    if (context != null && context.LoggingEnabled)
                    {
                        context.Logger.Log("Process did not enter idle state.");
                    }
                    return false;
                }

                if (context != null && context.LoggingEnabled)
                {
                    context.Logger.Log("Waiting for started process to open a main window.");
                }
                while (m_process.MainWindowHandle == IntPtr.Zero)
                {
                    TimeSpan time = DateTime.UtcNow - entryTime;
                    if (time > timeout)
                    {
                        //context.ReportFailureOrError(new TimeoutWaitingForInputIdleError(), String.Format("Process did not open a main window in {0} seconds.", timeout));
                        return false;
                    }
                    System.Threading.Thread.Sleep(100);
                    m_process.Refresh();
                }
                if (context != null && context.LoggingEnabled)
                {
                    context.Logger.Log("Process main window opened.");
                }
                this.SetState(TaskExecutionState.Running);
                return true;
            }
            else
            {
                switch (m_state)
                {
                    case TaskExecutionState.Created:
                    case TaskExecutionState.StartRequested:
                    case TaskExecutionState.AwaitingStartCondition:
                        if (context != null && context.LoggingEnabled)
                        {
                            context.Logger.Log("Process not running as expected.");
                        }
                        return false;
                    case TaskExecutionState.ErrorStarting:
                        return false;
                    case TaskExecutionState.Running:
                    case TaskExecutionState.RunningNotResponding:
                    case TaskExecutionState.PauseRequested:
                    case TaskExecutionState.Paused:
                    case TaskExecutionState.StopRequested:
                    case TaskExecutionState.KillRequested:
                    case TaskExecutionState.Terminating:
                        if (context != null && context.LoggingEnabled)
                        {
                            context.Logger.Log("Process is started.");
                        }
                        break;
                    case TaskExecutionState.Ended:
                    case TaskExecutionState.EndedByException:
                        if (context != null && context.LoggingEnabled)
                        {
                            context.Logger.Log("Process ended.");
                        }
                        break;
                }
                return true;
            }
        }

        public bool WaitForExit(TimeSpan timeout = new TimeSpan())
        {
            if (timeout == TimeSpan.Zero)
            {
                m_process.WaitForExit();
                return true;
            }
            else
            {
                return m_process.WaitForExit((int)(timeout.Ticks / TimeSpan.TicksPerMillisecond));
            }
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
                case TaskExecutionState.StartRequested:
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
                case TaskExecutionState.StartRequested:
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
