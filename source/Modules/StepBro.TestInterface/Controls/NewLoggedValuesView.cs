using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StepBro.Core.Attributes;

namespace StepBro.TestInterface.Controls
{
    [ObjectPanel(allowMultipleInstances: false)]
    public partial class NewLoggedValuesView : StepBro.Core.Controls.ObjectPanel
    {
        public NewLoggedValuesView()
        {
            InitializeComponent();
        }
    }
}
