using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using StepBro.Core.Logging;
using StepBro.HostSupport;
using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.UI.Controls
{
    public abstract class ChronoListViewEntry : ITimestampedViewEntry
    {
        public abstract ITimestampedData DataObject { get; }

        public virtual DateTime TimeStamp { get { return this.DataObject.Timestamp; } }

        public abstract string GetTextForSearchMatching(bool includeExtraFields);

        public abstract void DoPaint(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState markings);

        public static double DrawTextField(
            DrawingContext context, 
            ChronoListViewPort.IView view, 
            IBrush color, string s, Rect rect, double width = 0)
        {
            var formattedText = new FormattedText(s, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, view.NormalFont, view.FontSize, color);
            var strWidth = formattedText.WidthIncludingTrailingWhitespace;
            if (strWidth > width)
            {
                width = strWidth;
            }
            context.DrawText(formattedText, rect.TopLeft);
            return width;
        }
    }
}
