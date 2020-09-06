using StepBro.Core.Data;
using StepBro.TestInterface;
using StepBro.TestInterface.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stepbro.TestInterface
{
    public class PanelCreator : ObjectPanelCreator
    {
        protected override IEnumerable<ObjectPanelInfo> CreatePanelList()
        {
            yield return new ObjectPanelInfo<CommandTerminal,SerialTestConnection>("Test Interface Terminal", "", true, false);
        }
    }
}
