using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StepBro.PanelCreator.UI
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:StepBro.PanelCreator.UI"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:StepBro.PanelCreator.UI;assembly=StepBro.PanelCreator.UI"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:ProcedureActivationButton/>
    ///
    /// </summary>
    public class ProcedureActivationButton : Button, IPanelControl
    {
        private IPanelControlContext m_context;

        static ProcedureActivationButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProcedureActivationButton), new FrameworkPropertyMetadata(typeof(ProcedureActivationButton)));
        }

        public ProcedureActivationButton()
        {
            this.Background = Brushes.Chocolate;
        }

        public string Procedure { get; set; }

        public string Model { get; set; }

        public string Text { get { return this.Content as string; } set { this.Content = value; } }

        public string Arguments { get; set; }

        public IPanelControlInteraction Interaction { get; private set; } = null;

        public void SetupPanelControl(IPanelControlContext context)
        {
            m_context = context;
            this.Interaction = new GenericPanelControlInteraction(this);
        }
    }
}
