using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
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
            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), ".."));

            //string pipename = hThis.ToString("X");
            //m_sideKickPipe = SideKickPipe.StartServer(pipename);
            var bridge = new System.Diagnostics.Process();
            bridge.StartInfo.FileName = Path.Combine(folder, "StepBro.VISABridge.exe");
            bridge.StartInfo.Arguments = "--automate";
            var started = bridge.Start();

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
