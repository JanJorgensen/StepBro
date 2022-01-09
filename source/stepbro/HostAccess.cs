using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Host;
using StepBro.Core.Logging;

namespace StepBro.Cmd
{
    public class HostAccess : StepBro.Core.Host.HostAccessBase<HostAccess>
    {
        public HostAccess(out IService serviceAccess) : base("Host", out serviceAccess, typeof(ILogger))
        {
        }

        public override bool IsWPFApplication { get { return false; } }

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
