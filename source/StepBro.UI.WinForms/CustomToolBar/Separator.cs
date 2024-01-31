using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class Separator : ToolStripSeparator
    {
        public Separator(string name) : base()
        {
            this.Name = name;
            this.Margin = new Padding(1, 0, 1, 0);
        }

        public void Setup(PropertyBlock definition)
        {
        }
    }
}
