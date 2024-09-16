using System;
using System.Linq;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using StepBro.Core.Logging;
using StepBro.Core.Execution;

namespace StepBro.Core.Host
{
    //public abstract class ServiceBase<TService, TThis> where TThis : ServiceBase<TService, TThis>, TService
    public abstract class HostAccessBase<TThis> :
        ServiceBase<IHost, TThis>, IHost where TThis : HostAccessBase<TThis>, IHost
    {
        private ILogger m_logger;

        protected HostAccessBase(string name, out IService serviceAccess, params Type[] dependencies) :
            base(name, out serviceAccess, dependencies.Concat(new Type[] { typeof(ILogger) }).ToArray())
        { }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_logger = manager.Get<ILogger>();
        }

        public abstract HostType Type { get; }
        public abstract IEnumerable<NamedData<object>> ListHostCodeModuleInstances();

        public abstract IEnumerable<Type> ListHostCodeModuleTypes();

        public virtual void LogSystem(string text)
        {
            m_logger.LogSystem(text);
        }

        public virtual void LogUserAction(string text)
        {
            m_logger.LogUserAction(text);
        }

        public virtual bool SupportsUserInteraction { get { return false; } }


        public virtual UserInteraction SetupUserInteraction(ICallContext context, string header)
        {
            context.ReportError("The used host application dors not support user ainteraction from scripts.");
            return null;
        }

    }
}
