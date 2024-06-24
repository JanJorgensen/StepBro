using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.IPC;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace StepBro.ExecutionHelper
{
    /// <summary>
    /// Access class to Execution Helper.
    /// This is used to bind the StepBro script to an instance of an ExecutionHelper executable running on the machine.
    /// If no instance is running, it will automatically start an instance of ExecutionHelper, when <see cref="CreateExecutionHelper" /> is called.
    /// </summary>
    public class Access : INameable, IDisposable
    {
        /// <summary>
        /// The pipe used to communicate with the ExecutionHelper instance.
        /// </summary>
        private Pipe m_executionHelperPipe = null;
        /// <summary>
        /// Determines whether this script should stop if ExecutionHelper is closed.
        /// </summary>
        private bool m_closeWhenExecutionHelperCloses = false;
        /// <summary>
        /// EventHandler to handle incoming messages from ExecutionHelper
        /// </summary>
        private EventHandler m_executionHelperClosedEventHandler = null;

        /// <summary>
        /// Name of the Access variable.
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// Prefix used for variable names and similar, so multiple accesses to the ExecutionHelper can have the same variable names.
        /// If Name is set but Prefix is not set, Name will be used as a prefix.
        /// </summary>
        public string Prefix { get; set; } = null;



        /// <summary>
        /// ReceivedData is called when ExecutionHelper sends data to Access.
        /// </summary>
        /// <param name="message">A message from ExecutionHelper to Access</param>
        private void ReceivedData(Tuple<string, string> message)
        {
            if (message.Item1 == nameof(StepBro.ExecutionHelper.Messages.ShortCommand))
            {
                var cmd = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.ShortCommand>(message.Item2);
                if (cmd == StepBro.ExecutionHelper.Messages.ShortCommand.CloseApplication && m_closeWhenExecutionHelperCloses)
                {
                    m_executionHelperPipe.Dispose();
                }
            }
        }

        /// <summary>
        /// Used to create an ExecutionHelper instance and connect to it.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="arguments">Arguments sent to the ExecutionHelper instance, for example -drc to tell ExecutionHelper not to run a specified command</param>
        /// <param name="closeWhenExecutionHelperCloses">Specifies whether the script should stop when ExecutionHelper is stopped</param>
        /// <returns>True if ExecutionHelper was created/connected to properly, false otherwise</returns>
        public bool CreateExecutionHelper([Implicit] ICallContext context = null, string arguments = "", bool closeWhenExecutionHelperCloses = false)
        {
            // If no prefix is set during construction, we use the name of the variable instead
            if (Prefix == null)
            {
                Prefix = Name;
            }

            m_closeWhenExecutionHelperCloses = closeWhenExecutionHelperCloses;
            bool result = true;

            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(path);

            if (System.Diagnostics.Process.GetProcessesByName("StepBro.ExecutionHelper").Length == 0)
            {
                var executionHelper = new System.Diagnostics.Process();
                executionHelper.StartInfo.FileName = Path.Combine(folder, "../StepBro.ExecutionHelper.exe"); //../ because ExecutionHelper is in the main bin folder and this is the Modules folder
                executionHelper.StartInfo.Arguments = arguments;
                var m_executionHelperStarted = executionHelper.Start();
                if (!m_executionHelperStarted)
                {
                    result = false;
                }
            }
            
            m_executionHelperPipe = Pipe.StartClient("StepBroExecutionHelper", null);

            m_executionHelperPipe.ReceivedData += (sender, e) =>
            {
                ReceivedData(e);
            };

            m_executionHelperClosedEventHandler = (sender, e) =>
            {
                context.Logger.LogError("ExecutionHelper closed unexpectedly");
            };

            m_executionHelperPipe.OnConnectionClosed += m_executionHelperClosedEventHandler;

            if (result)
            {
                int timeoutMs = 2500;
                while (!m_executionHelperPipe.IsConnected() && timeoutMs > 0)
                {
                    int waitTimeMs = 200;
                    System.Threading.Thread.Sleep(waitTimeMs);
                    timeoutMs -= waitTimeMs;
                }
                result = timeoutMs > 0;
            }

            return result;
        }

        /// <summary>
        /// Creates or Sets the value of a variable on the ExecutionHelper side.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="variableName">Name of variable to create or set</param>
        /// <param name="value">Value the variable should have</param>
        /// <returns>True if successful</returns>
        public bool CreateOrSetVariable([Implicit] ICallContext context, string variableName, object value)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.CreateOrSetVariable(Prefix + variableName, value));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        /// <summary>
        /// Increments a variable by 1.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="variableName">Variable to increment</param>
        /// <returns>True if successful</returns>
        public bool IncrementVariable([Implicit] ICallContext context, string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.IncrementVariable(Prefix + variableName));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        /// <summary>
        /// Gets the value of a variable.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="variableName">Name of variable to get value of</param>
        /// <returns>Variable value</returns>
        /// <exception cref="NotSupportedException">Exception thrown if the type of the variable is not supported (yet)</exception>
        public object GetVariable([Implicit] ICallContext context, string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.GetVariable(Prefix + variableName));

            object variable = -1L;

            int timeoutMs = 2500;
            Tuple<string, string> input = null;
            do
            {
                input = m_executionHelperPipe.TryGetReceived();
                if (input != null)
                {
                    break;
                }
                // Wait
                Thread.Sleep(1);
                timeoutMs--;
            } while (timeoutMs > 0);

            if (input.Item1 == nameof(StepBro.ExecutionHelper.Messages.SendVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SendVariable>(input.Item2);
                variable = data.Variable;
            }

            if (variable is JsonElement v)
            {
                switch(v.ValueKind)
                {
                    case JsonValueKind.String:
                        variable = variable.ToString(); break;
                    case JsonValueKind.Number:
                        long longVariable = 0;
                        Int64.TryParse(variable.ToString(), out longVariable);
                        variable = longVariable;
                        break;
                    case JsonValueKind.True:
                        variable = true;
                        break;
                    case JsonValueKind.False:
                        variable = false;
                        break;
                    default:
                        throw new NotSupportedException("Variable of kind: " + v.ValueKind.ToString() + " is not supported yet!");
                }
            }

            return variable;
        }

        /// <summary>
        /// Saves variables to a file from ExecutionHelper so it can access them at a later point.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="fileName">Name of the file to save</param>
        /// <returns>True if successful</returns>
        public bool SaveFile([Implicit] ICallContext context, string fileName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SaveFile(Prefix + fileName));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        /// <summary>
        /// Loads file into ExecutionHelper so it has updated variables from a file.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="fileName">Name of the file to load</param>
        /// <param name="reportError">Whether we should give StepBro an error if the file could not be loaded (For example if it has not been created yet)</param>
        /// <returns>True if successful</returns>
        public bool LoadFile([Implicit] ICallContext context, string fileName, bool reportError = true)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.LoadFile(Prefix + fileName));

            // reportError is by default true, but can be turned off if we use LoadFile at the start of a script
            // and it may not have been created yet
            bool result = WaitForAcknowledge(context, reportError);
            return result;
        }

        /// <summary>
        /// Sets a command to run when <see cref="RunPeriodicCheck" /> is called.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="command">The command, as a string, for ExecutionHelper to run</param>
        /// <returns>True if successful</returns>
        public bool SetCommandToRun([Implicit] ICallContext context, string command)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SetCommandToRun(command));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        /// <summary>
        /// Closes ExecutionHelper.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool CloseApplication()
        {
            m_executionHelperPipe.OnConnectionClosed -= m_executionHelperClosedEventHandler;
            m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.CloseApplication);

            m_executionHelperPipe.Dispose();
            return true;
        }

        /// <summary>
        /// Stops ExecutionHelper from autosaving.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool SuspendAutosave()
        {
            m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.SuspendAutosave);
            return true;
        }

        /// <summary>
        /// Resumes autosaving in ExecutionHelper.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool ResumeAutosave()
        {
            m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.ResumeAutosave);
            return true;
        }

        /// <summary>
        /// Tells ExecutionHelper to run a periodic check, for example for Windows Update or similar.
        /// ExecutionHelper runs the command set by <see cref="SetCommandToRun(ICallContext, string)" /> after the check is done.
        /// </summary>
        /// <returns>True if successful</returns>
        public bool RunPeriodicCheck()
        {
            m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.RunPeriodicCheck);
            return true;
        }

        /// <summary>
        /// Waits for an "Acknowledge" from ExecutionHelper. Used for error handling.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reportError"></param>
        /// <returns>True if acknowledge was received within a timeout limit, false otherwise</returns>
        bool WaitForAcknowledge(ICallContext context, bool reportError = true)
        {
            bool result = false;

            var input = m_executionHelperPipe.TryGetReceived();
            int timeoutMs = 2500;
            while (input == null && timeoutMs > 0)
            {
                // Wait
                Thread.Sleep(1);
                timeoutMs--;
                input = m_executionHelperPipe.TryGetReceived();
            }

            result = timeoutMs > 0;

            if (result && input.Item1 == nameof(StepBro.ExecutionHelper.Messages.ShortCommand))
            {
                var cmd = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.ShortCommand>(input.Item2);
                if (cmd == StepBro.ExecutionHelper.Messages.ShortCommand.Acknowledge)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else if (input.Item1 == nameof(StepBro.ExecutionHelper.Messages.Error))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.Error>(input.Item2);
                if (reportError)
                {
                    context.ReportError(data.Message);
                }
                result = false;
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Disposes ExecutionHelper
        /// </summary>
        public void Dispose()
        {
            if (m_executionHelperPipe != null)
            {
                m_executionHelperPipe.OnConnectionClosed -= m_executionHelperClosedEventHandler;
                if (m_executionHelperPipe.IsConnected())
                {
                    m_executionHelperPipe.Dispose();
                }
            }
        }
    }
}
