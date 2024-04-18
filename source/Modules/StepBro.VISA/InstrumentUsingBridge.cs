using StepBro.Core.Api;
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
using System.Threading.Tasks;

namespace StepBro.VISA
{
    public class Instrument : INameable, INamedObject
    {
        private string m_resource = "";
        private string m_name = "instrument";
        private Pipe m_visaPipe = null;

        private void ReceivedData(Tuple<string, string> message)
        {

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

            Pipe.ReceivedData += (sender, e) =>
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

            return started;
        }

        public void Close([Implicit] ICallContext context = null)
        {

        }

        public string Query([Implicit] ICallContext context, string command)
        {
            return "";
            //return m_instrument.Query(command);
        }

        public void Write([Implicit] ICallContext context, string command)
        {
            //m_instrument.Write(command);
        }

        public string Read([Implicit] ICallContext context)
        {
            return "";
            //return m_instrument.Read();
        }

        public string[] ListAvailableResources()
        {
            return new string[0];
        }
    }
}
