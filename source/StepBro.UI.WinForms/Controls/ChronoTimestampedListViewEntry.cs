using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoTimestampedListViewEntry : ChronoListViewEntry
    {
        public abstract DateTime TimeStamp { get; }

        public override void DoPaint(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, bool selected)
        {
            var width = view.ViewSettings.TimeStampWidth;

            // Draw timestamp here

            var timestamp = this.TimeStamp.ToSecondsTimestamp(view.ViewSettings.ZeroTime);

            //s = e.GetTimeString((e.Previous != null) ? e.Previous.TimeStamp : e.TimeStamp, m_timestampType, m_zeroTime, m_firstSelectedTime) + ": ";

            var strWidth = (int)(pe.Graphics.MeasureString(timestamp, view.NormalFont, rect.Width, ChronoListViewEntry.NormalStringFormat).Width);
            if (strWidth > width)
            {
                width = strWidth;
                view.ViewSettings.TimeStampWidth = width;
            }
            pe.Graphics.DrawString(
               timestamp,
               view.NormalFont,
               Brushes.White,
               (rect.X + width) - strWidth, // Right adjust.
               rect.Y,
               ChronoListViewEntry.NormalStringFormat);

            //var brush = selected ? Brushes.Blue : Brushes.Black;

            rect.X += width + 4;
            this.PaintRest(pe, view, ref rect, selected);
        }

        protected abstract void PaintRest(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, bool selected);
    }
}
