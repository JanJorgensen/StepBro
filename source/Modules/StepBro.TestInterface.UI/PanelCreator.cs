using StepBro.Core.Data;
using StepBro.TestInterface;
using StepBro.TestInterface.Controls;
using StepBro.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

[assembly: StepBro.Core.Api.IsUIModule]  // Only load this module if host is a WPF application.

namespace StepBro.TestInterface.UI
{
    public class PanelCreator : CustomPanelCreator
    {
        private class CommandTerminalPanelType : CustomPanelType<CommandTerminalView, CommandTerminalViewModel, SerialTestConnection>
        {
            public CommandTerminalPanelType() : base("Test Interface Terminal", "", false) { }

            public override bool SetPanelObjectBinding(CommandTerminalView control, SerialTestConnection @object)
            {
                (control.DataContext as CommandTerminalViewModel).Connection = @object;
                return true;
            }
        }

        protected override IEnumerable<CustomPanelType> CreatePanelList()
        {
            yield return new CommandTerminalPanelType();
            //yield return new CustomPanelType<LoggedValuesView_WinForms, SerialTestConnection>("Logged Values View", "", false);
        }
    }
}
