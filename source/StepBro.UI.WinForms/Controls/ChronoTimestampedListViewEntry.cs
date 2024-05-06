using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoTimestampedListViewEntry : ChronoListViewEntry
    {
        public override void OnPaint(PaintEventArgs pe, ChronoListViewPort.IView view, int top, bool selected)
        {
            // Draw timestamp here

            this.DrawRest(pe, view, top, selected);
        }

        protected abstract void DrawRest(PaintEventArgs pe, ChronoListViewPort.IView view, int top, bool selected);
    }
}
