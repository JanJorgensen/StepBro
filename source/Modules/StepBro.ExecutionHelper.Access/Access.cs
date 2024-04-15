using StepBro.Core.IPC;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;

namespace StepBro.ExecutionHelper
{
    public class Access
    {
        EventHandler closeEventHandler = null;
        private Pipe m_executionHelperPipe = null;
        bool m_executionHelperStarted = false;

        public bool CreateExecutionHelper()
        {
            bool result = true;

            closeEventHandler = (sender, e) =>
            {
                m_executionHelperPipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.Close);
                Thread.Sleep(1000);     // Leave some time for the execution helper application to receive the command.
            };

            // m_sidekickLogger = StepBroMain.Logger.RootLogger.CreateSubLocation("SideKick");

            AppDomain.CurrentDomain.ProcessExit += closeEventHandler;

            var hThis = GetConsoleWindow();

            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(path);

            string pipename = hThis.ToString("X");
            m_executionHelperPipe = Pipe.StartServer("StepBroExecutionHelper", pipename);
            var executionHelper = new System.Diagnostics.Process();
            executionHelper.StartInfo.FileName = Path.Combine(folder, "../StepBro.ExecutionHelper.exe"); //../ because ExecutionHelper is in the main bin folder and this is the Modules folder
            executionHelper.StartInfo.Arguments = pipename;
            m_executionHelperStarted = executionHelper.Start();

            if (m_executionHelperStarted)
            {
                // commandObjectDictionary = new Dictionary<string, ITextCommandInput>();

                while (!m_executionHelperPipe.IsConnected())
                {
                    System.Threading.Thread.Sleep(200);
                    // TODO: Timeout
                }

                // StartLogDumpTask();

                // m_mode = Mode.WorkbenchWithSidekick;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool CreateVariable(string variableName, object initialValue)
        {
            bool result = true;

            // TODO: Error checking
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.CreateVariable(variableName, initialValue));

            return result;
        }

        public bool IncrementVariable(string variableName)
        {
            bool result = true;

            // TODO: Error checking
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.IncrementVariable(variableName));

            return result;
        }

        public object GetVariable(string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.GetVariable(variableName));

            object variable = 0;

            // TODO: Timeout
            var input = m_executionHelperPipe.TryGetReceived();
            while (input == null)
            {
                // Wait
                Thread.Sleep(1);
                input = m_executionHelperPipe.TryGetReceived();
            }

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

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
