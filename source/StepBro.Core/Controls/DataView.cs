using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.Core.Controls
{
    public partial class DataView : UserControl
    {
        public DataView()
        {
            InitializeComponent();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            dataViewControl.UpdateView();
        }

        public DataViewControl ViewControl { get { return dataViewControl; } }
    }
}
