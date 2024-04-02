using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.Host
{
    //public abstract class ServiceBase<TService, TThis> where TThis : ServiceBase<TService, TThis>, TService
    public abstract class HostAccessBase<TThis> : 
        ServiceBase<IHost, TThis>, IHost where TThis : HostAccessBase<TThis>, IHost
    {
        protected HostAccessBase(string name, out IService serviceAccess, params Type[] dependencies) :
            base(name, out serviceAccess, dependencies)
        { }
        public abstract HostType Type { get; }
        public abstract IEnumerable<NamedData<object>> ListHostCodeModuleInstances();

        public abstract IEnumerable<Type> ListHostCodeModuleTypes();
    }
}
