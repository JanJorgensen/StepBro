using StepBro.Core.Data;
using StepBro.TestInterface;
using StepBro.TestInterface.Controls;
using StepBro.UI.Panels;
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
            //yield return new ObjectPanelInfoWinForms<CommandTerminal_WinForms,SerialTestConnection>("Test Interface Terminal", "", false);
            //yield return new ObjectPanelInfoWinForms<LoggedValuesView_WinForms, SerialTestConnection>("Logged Values View", "", false);
            yield break;
        }
    }
}
