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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PanelView : UserControl
    {
        private PanelViewModel m_model = null;
        private Panel m_definition = null;

        public PanelView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(this.DataContext))
            {
                if (m_model != null)
                {
                    m_model.PanelDefinitionChanged -= Model_PanelDefinitionChanged;
                }
                m_model = this.Model;
                if (m_model != null)
                {
                    m_model.PanelDefinitionChanged += Model_PanelDefinitionChanged;
                }
            }
        }

        private void Model_PanelDefinitionChanged(object sender, PanelViewModel e)
        {
            if (m_definition != null && panelCanvas.Children.Count > 0)
            {
                panelCanvas.Children.Clear();
            }
            m_definition = e.PanelDefinition;
            if (m_definition != null)
            {
                //var control = new HorizontalSplit();
                //var tbl = new ProcedureActivationButton();
                //tbl.Text = DateTime.Now.ToShortTimeString();
                //control.LeftPanel = tbl;
                //var tbr = new ProcedureActivationButton();
                //tbr.Text = DateTime.Now.ToShortTimeString();
                //control.RightPanel = tbr;
                //panelCanvas.Children.Add(control);

                var tbl = new TextBox();
                tbl.Text = DateTime.Now.ToShortTimeString();
                panelCanvas.Children.Add(tbl);
            }
        }

        public PanelViewModel Model { get { return this.DataContext as PanelViewModel; } }
    }
}