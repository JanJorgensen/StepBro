using Avalonia.Media;
using StepBro.Core.Data;
using StepBro.HostSupport;
using System;

namespace StepBro.UI.Controls
{
    public abstract class ChronoTimestampedListViewEntry : ChronoListViewEntry
    {
        public override void DoPaint(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState markings)
        {
            var fontsize = view.FontSize;
            var width = view.ViewSettings.TimeStampWidth * fontsize;

            string timestamp = "";
            switch (view.ViewSettings.TimeFormat)
            {
                case HostSupport.Models.ChronoListViewTimestampFormat.Seconds:
                    timestamp = this.TimeStamp.ToSecondsTimestamp(view.ViewSettings.ZeroTime);
                    break;
                case HostSupport.Models.ChronoListViewTimestampFormat.SecondsDelta:
                    timestamp = this.TimeStamp.ToSecondsTimestamp(view.ViewSettings.ZeroTime);
                    break;
                case HostSupport.Models.ChronoListViewTimestampFormat.HoursMinutesSeconds:
                    timestamp = this.TimeStamp.ToHMSTimestamp(view.ViewSettings.ZeroTime);
                    break;
                case HostSupport.Models.ChronoListViewTimestampFormat.LocalTime:
                    timestamp = String.Concat(this.TimeStamp.Hour.ToString("00"), ":", this.TimeStamp.Minute.ToString("00"), ":", this.TimeStamp.Second.ToString("00"), ".", this.TimeStamp.Millisecond.ToString("000"));
                    break;
                case HostSupport.Models.ChronoListViewTimestampFormat.LocalDateTime:
                    timestamp = this.TimeStamp.ToGeneralFormat();
                    break;
                default:
                    break;
            }
            var w = DrawTextField(context, view, view.NormalTextColor, timestamp, rect, width);
            if (w > (width/fontsize))
            {
                width = w;
                view.ViewSettings.TimeStampWidth = width / fontsize;
            }

            rect = new Avalonia.Rect(new Avalonia.Point(rect.X + width + fontsize * 0.8, rect.Y), rect.Size);
            this.PaintRest(context, view, ref rect, markings);
        }

        protected abstract void PaintRest(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState markings);
    }
}
