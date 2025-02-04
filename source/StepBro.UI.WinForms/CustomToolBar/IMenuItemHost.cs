using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal interface IMenuItemHost
    {
        void Add(ToolStripItem item);
        void Add(ToolStripTextBox item);
    }
}
