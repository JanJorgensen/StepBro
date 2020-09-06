using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.Host
{
    public abstract class HostAccessBase : IHost
    {
        public abstract IEnumerable<NamedData<object>> ListHostCodeModuleInstances();

        public abstract IEnumerable<Type> ListHostCodeModuleTypes();
    }
}
