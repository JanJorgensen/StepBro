using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Host;
using StepBro.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.HostSupport.Models;

namespace StepBro.HostSupport.Test
{
    public class HostAccess : StepBro.Core.Host.HostAccessBase<HostAccess>
    {
        public HostAppModel m_appModel;
        public StepBroCoreTest.Data.DummyClass m_myTestDummy = null;
        public StepBroCoreTest.Data.DummyToolX m_myTestTool = null;

        public HostAccess(HostAppModel appModel, out IService serviceAccess) : base("Host", out serviceAccess)
        {
            m_appModel = appModel;
            this.AddObject("myTestDummy", m_myTestDummy = new StepBroCoreTest.Data.DummyClass());
            this.AddObject("myTestTool", m_myTestTool = new StepBroCoreTest.Data.DummyToolX());
        }

        public override HostType Type { get { return HostType.Mock; } }

        public override IEnumerable<Type> ListHostCodeModuleTypes()
        {
            yield return typeof(StepBroCoreTest.Data.DummyClass);
            yield return typeof(StepBroCoreTest.Data.DummyToolX);
        }

        public override bool SupportsUserInteraction { get { return false; } }

        public override UserInteraction SetupUserInteraction(ICallContext context, string header)
        {
            //var interaction = new UserInteraction();
            //interaction.HeaderText = header;
            //m_mainForm.OpenUserInteraction(interaction);
            //return interaction;
            throw new NotSupportedException();
        }
    }

}
