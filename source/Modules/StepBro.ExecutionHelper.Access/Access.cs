﻿using StepBro.Core.IPC;
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
        private Pipe m_executionHelperPipe = null;
        bool m_executionHelperStarted = false;
        bool m_closeWhenExecutionHelperCloses = false;
        EventHandler<Tuple<string, string>> receivedDataEventHandler = null;

        private void ReceivedData(Tuple<string, string> message)
        {
            if (message.Item1 == nameof(StepBro.ExecutionHelper.Messages.ShortCommand))
            {
                var cmd = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.ShortCommand>(message.Item2);
                if (cmd == StepBro.ExecutionHelper.Messages.ShortCommand.Close && m_closeWhenExecutionHelperCloses)
                {
                    Thread.Sleep(100);
                    // m_executionHelperPipe.Dispose();
                    System.Environment.Exit(0); // Close the application gracefully
                }
            }
        }

        public bool CreateExecutionHelper(bool closeWhenExecutionHelperCloses = false)
        {
            m_closeWhenExecutionHelperCloses = closeWhenExecutionHelperCloses;
            bool result = true;

            var hThis = GetConsoleWindow();

            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(path);

            string pipename = hThis.ToString("X");
            m_executionHelperPipe = Pipe.StartServer("StepBroExecutionHelper", pipename);
            var executionHelper = new System.Diagnostics.Process();
            executionHelper.StartInfo.FileName = Path.Combine(folder, "../StepBro.ExecutionHelper.exe"); //../ because ExecutionHelper is in the main bin folder and this is the Modules folder
            executionHelper.StartInfo.Arguments = pipename;
            m_executionHelperStarted = executionHelper.Start();

            receivedDataEventHandler = (sender, e) =>
            {
                ReceivedData(e);
            };

            Pipe.ReceivedData += receivedDataEventHandler;

            if (m_executionHelperStarted)
            {
                int timeoutMs = 2500;
                while (!m_executionHelperPipe.IsConnected() && timeoutMs > 0)
                {
                    int waitTimeMs = 200;
                    System.Threading.Thread.Sleep(waitTimeMs);
                    timeoutMs -= waitTimeMs;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool CreateVariable(string variableName, object initialValue)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.CreateVariable(variableName, initialValue));

            bool result = WaitForAcknowledge();
            return result;
        }

        public bool IncrementVariable(string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.IncrementVariable(variableName));

            bool result = WaitForAcknowledge();
            return result;
        }

        public bool SetVariable(string variableName, object value)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SetVariable(variableName, value));

            bool result = WaitForAcknowledge();
            return result;
        }

        public object GetVariable(string variableName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.GetVariable(variableName));

            object variable = -1;

            int timeoutMs = 2500;
            var input = m_executionHelperPipe.TryGetReceived();
            while (input == null)
            {
                // Wait
                Thread.Sleep(1);
                timeoutMs--;
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

        public bool SaveFile(string fileName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.SaveFile(fileName));

            bool result = WaitForAcknowledge();
            return result;
        }

        public bool LoadFile(string fileName)
        {
            m_executionHelperPipe.Send(new StepBro.ExecutionHelper.Messages.LoadFile(fileName));

            bool result = WaitForAcknowledge();
            return result;
        }

        bool WaitForAcknowledge()
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

            if (input.Item1 == nameof(StepBro.ExecutionHelper.Messages.ShortCommand))
            {
                var cmd = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.ShortCommand>(input.Item2);
                if (cmd == StepBro.ExecutionHelper.Messages.ShortCommand.Acknowledge)
                {
                    result = true;
                }
            }

            return result;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
