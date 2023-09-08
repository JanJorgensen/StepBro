using StepBro.Core.Api;
using StepBro.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: StepBro.Core.Api.IsUIModule]  // Only load this module if host is a WPF application.

namespace StepBro.PanelCreator.UI
{
    [Public]
    public class PanelCreator : CustomPanelCreator
    {
        private class PanelCreatorPanelType : CustomPanelType<PanelView, PanelViewModel, Panel>
        {
            public PanelCreatorPanelType() : base("User Defined UI Panel", "Creates a panel that shows the GUI elements defined in a PanelCreator panel, defined in a script file.", false)
            {
            }

            public override bool SetPanelObjectBinding(PanelView control, Panel @object)
            {
                control.Model.PanelDefinition = @object;
                return true;
            }
        }

        protected override IEnumerable<CustomPanelType> CreatePanelList()
        {
            yield return new PanelCreatorPanelType();
        }
    }
}
