using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Cmd
{
    public class HostAccess : StepBro.Core.Host.HostAccessBase
    {
        public HostAccess()
        {
        }

        public override IEnumerable<NamedData<object>> ListHostCodeModuleInstances()
        {
            //yield return new NamedData<object>("Host.Console", m_app);
            yield break;
        }

        public override IEnumerable<Type> ListHostCodeModuleTypes()
        {
            yield break;
        }
    }
}
