using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoListViewEntry
    {
        public object DataObject { get; }
        public abstract int Height { get; }
        public abstract void OnPaint(PaintEventArgs pe, ChronoListViewPort.IView view, int top, bool selected);
    }
}
