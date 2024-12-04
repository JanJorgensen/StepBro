using Avalonia.Media;
using StepBro.Core.Data;
using StepBro.HostSupport;

namespace StepBro.UI.Controls
{
    public abstract class ChronoTimestampedListViewEntry : ChronoListViewEntry
    {
        public override void DoPaint(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState selected)
        {
            var width = view.ViewSettings.TimeStampWidth;
            var timestamp = this.TimeStamp.ToSecondsTimestamp(view.ViewSettings.ZeroTime);
            var w = DrawTextField(context, view, view.NormalTextColor, timestamp, ref rect, width);
            if (w > width)
            {
                width = w;
                view.ViewSettings.TimeStampWidth = width;
            }

            rect = new Avalonia.Rect(new Avalonia.Point((int)(rect.X) + width + 4, (int)rect.Y), rect.Size);
            this.PaintRest(context, view, ref rect, selected);
        }

        protected abstract void PaintRest(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState selected);
    }
}
