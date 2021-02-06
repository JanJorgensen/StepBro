using StepBro.Core.Attributes;
using System.Windows.Controls;

namespace StepBro.TestInterface.Controls
{
    /// <summary>
    /// Interaction logic for CommandTerminal.xaml
    /// </summary>
    [ObjectPanel(allowMultipleInstances: false)]
    public partial class CommandTerminal : UserControl
    {
        public CommandTerminal()
        {
            this.InitializeComponent();
        }
    }
}
