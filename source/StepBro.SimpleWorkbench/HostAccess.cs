using StepBro.Core.Data;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Execution;

namespace StepBro.SimpleWorkbench
{
    public class HostAccess : StepBro.Core.Host.HostAccessBase<HostAccess>
    {
        public HostAccess(out IService serviceAccess) : base("Host", out serviceAccess)
        {
        }

        public override HostType Type { get { return HostType.WinForms; } }

        public override IEnumerable<NamedData<object>> ListHostCodeModuleInstances()
        {
            // TODO: yield return new NamedData<object>("Host.Console", m_app);
            yield break;
        }

        public override IEnumerable<Type> ListHostCodeModuleTypes()
        {
            yield break;
        }

        public override bool SupportsUserInteraction {  get { return true; } }

        public override UserInteraction SetupUserInteraction(ICallContext context, string header)
        {
            return null;
        }
    }
}
