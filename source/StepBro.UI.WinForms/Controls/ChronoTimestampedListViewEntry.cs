using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.HostSupport;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoTimestampedListViewEntry : ChronoListViewEntry
    {
        public override void DoPaint(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, EntryMarkState selected)
        {
            var width = view.ViewSettings.TimeStampWidth;
            var timestamp = this.TimeStamp.ToSecondsTimestamp(view.ViewSettings.ZeroTime);
            var w = DrawTextField(pe.Graphics, view.NormalFont, view.NormalTextColor, timestamp, ChronoListViewEntry.NormalStringFormat, ref rect, width);
            if (w > width)
            {
                width = w;
                view.ViewSettings.TimeStampWidth = width;
            }

            rect.X += width + 4;
            this.PaintRest(pe, view, ref rect, selected);
        }

        protected abstract void PaintRest(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, EntryMarkState selected);
    }
}
