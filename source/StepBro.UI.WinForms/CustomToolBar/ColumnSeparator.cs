using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class ColumnSeparator : ToolStripSeparator
    {
        public ColumnSeparator(string name) : base()
        {
            this.Name = name;
        }

        public void Setup(PropertyBlock definition)
        {
        }
    }
}
