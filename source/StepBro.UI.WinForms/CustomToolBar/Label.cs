using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class Label : ToolStripLabel
    {
        public Label(string name) : base()
        {
            this.Name = name;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.Margin = new Padding(0, 1, 0, 2);
        }
    }
}
