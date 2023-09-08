using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for HorizontalSplit.xaml
    /// </summary>
    public partial class HorizontalSplit : UserControl, IPanelControl
    {
        private IPanelControlContext m_context;

        public HorizontalSplit()
        {
            InitializeComponent();
        }

        public IPanelControl LeftPanel
        {
            get
            {
                return (grid.Children[0] as Canvas).Children[0] as IPanelControl;
            }
            set
            {
                (grid.Children[0] as Canvas).Children.Clear();
                var c = value as Control;
                (grid.Children[0] as Canvas).Children.Add(c);
            }
        }
        public IPanelControl RightPanel
        {
            get
            {
                return (grid.Children[2] as Canvas).Children[0] as IPanelControl;
            }
            set
            {
                (grid.Children[2] as Canvas).Children.Clear();
                var c = value as Control;
                (grid.Children[2] as Canvas).Children.Add(c);
            }
        }

        public IPanelControlInteraction Interaction { get; private set; } = null;

        public void SetupPanelControl(IPanelControlContext context)
        {
            m_context = context;
            this.Interaction = new PanelControlInteraction(this);
        }


        private class PanelControlInteraction : GenericPanelControlInteraction
        {
            public PanelControlInteraction(IPanelControl control) : base(control)
            {
            }

            protected override IEnumerable<Tuple<string, PropertyInfo>> ListChildProperties()
            {
                yield return new Tuple<string, PropertyInfo>("Left", typeof(HorizontalSplit).GetProperty(nameof(HorizontalSplit.LeftPanel)));
                yield return new Tuple<string, PropertyInfo>("Right", typeof(HorizontalSplit).GetProperty(nameof(HorizontalSplit.RightPanel)));
            }
        }
    }
}
