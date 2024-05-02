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
    public class Access : INameable, IDisposable
    {
        private Pipe m_executionHelperPipe = null;
        private bool m_closeWhenExecutionHelperCloses = false;
        private EventHandler m_executionHelperClosedEventHandler = null;

        public string Name { get; set; } = null;
        public string Prefix { get; set; } = null;


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

        public bool CreateExecutionHelper([Implicit] ICallContext context = null, bool closeWhenExecutionHelperCloses = false)
        {
            // If constructor with no arguments were used, we use the name of the instance instead
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
                context.Logger.LogError("VISA closed unexpectedly");
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

        public bool CreateOrSetVariable([Implicit] ICallContext context, string variableName, object value)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.CreateOrSetVariable(Prefix + variableName, value));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        public bool IncrementVariable([Implicit] ICallContext context, string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.IncrementVariable(Prefix + variableName));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        public object GetVariable([Implicit] ICallContext context, string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.GetVariable(Prefix + variableName));

            object variable = -1;

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
                        throw new Exception("Variable of kind: " + v.ValueKind.ToString() + " is not supported yet!");
                }
            }

            return variable;
        }

        public bool SaveFile([Implicit] ICallContext context, string fileName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SaveFile(Prefix + fileName));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        public bool LoadFile([Implicit] ICallContext context, string fileName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.LoadFile(Prefix + fileName));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        public bool SetCommandRunOnStartup([Implicit] ICallContext context, string command)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SetCommandRunOnStartup(command));

            bool result = WaitForAcknowledge(context);
            return result;
        }

        public bool CloseApplication()
        {
            m_executionHelperPipe.OnConnectionClosed -= m_executionHelperClosedEventHandler;
            m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.CloseApplication);

            m_executionHelperPipe.Dispose();
            return true;
        }

        bool WaitForAcknowledge(ICallContext context)
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
                context.ReportError(data.Message);
                result = false;
            }
            else
            {
                result = false;
            }

            return result;
        }

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
