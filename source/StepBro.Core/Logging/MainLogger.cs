using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Tasks;

namespace StepBro.Core.Logging
{
    public interface IMainLogger 
    {
        void SetDebugState(bool isDebugging);
        LoggerRoot Logger { get; }
    }

    internal class MainLogger : ServiceBase<IMainLogger, MainLogger>, IMainLogger
    {
        private readonly LoggerRoot m_root;

        public MainLogger(out IService serviceAccess) : base("MainLogger", out serviceAccess)
        {
            m_root = new LoggerRoot("", false, "StepBro", "Main logger created");
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_root.RootLogger.Log("MainLogger", "Service started");
        }

        public LoggerRoot Logger { get { return m_root; } }

        public void SetDebugState(bool isDebugging)
        {
            m_root.IsDebugging = isDebugging;
        }
    }
}
