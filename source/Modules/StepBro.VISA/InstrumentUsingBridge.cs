﻿using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.IPC;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.VISA
{
    public class Instrument : INameable, INamedObject
    {
        private string m_resource = "";
        private string m_name = "instrument";
        private Pipe m_visaPipe = null;

        private void ReceivedData(Tuple<string, string> received)
        {
            switch (received.Item1)
            {
                case nameof(VISABridge.Messages.ShortCommand):
                    switch (System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.ShortCommand>(received.Item2))
                    {
                        case VISABridge.Messages.ShortCommand.None:
                            // Should not happen
                            break;
                        case VISABridge.Messages.ShortCommand.GetInstrumentList:
                            // Should not happen
                            break;
                        case VISABridge.Messages.ShortCommand.SessionClosed:
                            // TODO: Handle session closed

                            break;
                        case VISABridge.Messages.ShortCommand.Receive:
                            // Should not happen
                            break;
                    }
                    break;
                case nameof(VISABridge.Messages.OpenSession):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.CloseSession):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.ConnectedInstruments):
                    // Handled elsewhere
                    break;
                case nameof(VISABridge.Messages.Received):
                    // TODO: Handle Received
                    break;
                case nameof(VISABridge.Messages.Send):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.SessionOpened):
                    // TODO: Handle Session Opened

                    break;
            }
        }

        public string Resource
        {
            get { return m_resource; }
            set { m_resource = value; }
        }

        public string Name { get { return m_name; } set { m_name = value; } }

        public string ShortName { get { return this.Name; } }

        public string FullName { get { return this.Name; } }

        public bool Open([Implicit] ICallContext context = null)
        {
            m_visaPipe = Pipe.StartServer("StepBroVisaPipe", "1234");

            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), ".."));

            var bridge = new System.Diagnostics.Process();
            bridge.StartInfo.FileName = Path.Combine(folder, "StepBro.VISABridge.exe");
            bridge.StartInfo.Arguments = "--automate";
            var started = bridge.Start();

            m_visaPipe.ReceivedData += (sender, e) =>
            {
                ReceivedData(e);
            };

            if (started)
            {
                int timeoutMs = 2500;
                while (!m_visaPipe.IsConnected() && timeoutMs > 0)
                {
                    int waitTimeMs = 200;
                    System.Threading.Thread.Sleep(waitTimeMs);
                    timeoutMs -= waitTimeMs;
                }
                started = timeoutMs > 0;
            }

            m_visaPipe.Send(new VISABridge.Messages.OpenSession("TODO"));

            return started;
        }

        public void Close([Implicit] ICallContext context = null)
        {
            m_visaPipe.Send(new VISABridge.Messages.CloseSession("TODO"));
        }

        public string Query([Implicit] ICallContext context, string command)
        {
            m_visaPipe.Send(new VISABridge.Messages.Send(command));
            m_visaPipe.Send(VISABridge.Messages.ShortCommand.Receive);
            return "";
            //return m_instrument.Query(command);
        }

        public void Write([Implicit] ICallContext context, string command)
        {
            m_visaPipe.Send(new VISABridge.Messages.Send(command));
            //m_instrument.Write(command);
        }

        public string Read([Implicit] ICallContext context)
        {
            m_visaPipe.Send(VISABridge.Messages.ShortCommand.Receive);
            return "";
            //return m_instrument.Read();
        }

        public string[] ListAvailableResources()
        {
            m_visaPipe.Send(VISABridge.Messages.ShortCommand.GetInstrumentList);

            int timeoutMs = 2500;
            Tuple<string, string> input = null;
            do
            {
                input = m_visaPipe.TryGetReceived();
                if (input != null)
                {
                    break;
                }
                // Wait
                Thread.Sleep(1);
                timeoutMs--;
            } while (timeoutMs > 0);

            string[] instruments = null;
            if (input.Item1 == nameof(VISABridge.Messages.ConnectedInstruments))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.ConnectedInstruments>(input.Item2);
                instruments = data.Instruments;
            }

            return instruments;
        }
    }
}
