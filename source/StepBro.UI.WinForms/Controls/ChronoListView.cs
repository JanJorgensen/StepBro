using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Controls
{
    public partial class ChronoListView : UserControl
    {
        public ChronoListView()
        {
            InitializeComponent();
            panelHorizontal.Height = vScrollBar.Width;
        }
    }
}
