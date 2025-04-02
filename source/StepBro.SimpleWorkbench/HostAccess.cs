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
        MainForm m_mainForm;

        public HostAccess(MainForm mainForm, out IService serviceAccess) : base("Host", out serviceAccess)
        {
            m_mainForm = mainForm;
        }

        public override HostType Type { get { return HostType.WinForms; } }

        public override IEnumerable<Type> ListHostCodeModuleTypes()
        {
            yield break;
        }

        public override bool SupportsUserInteraction { get { return true; } }

        public override UserInteraction SetupUserInteraction(ICallContext context, string header)
        {
            var interaction = new UserInteraction();
            interaction.HeaderText = header;
            m_mainForm.OpenUserInteraction(interaction);
            return interaction;
        }
    }
}
