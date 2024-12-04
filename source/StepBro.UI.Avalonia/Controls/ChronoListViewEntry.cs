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

namespace StepBro.UI.Controls
{
    public abstract class ChronoListViewEntry : ITimestampedViewEntry
    {
        public abstract ITimestampedData DataObject { get; }

        public virtual DateTime TimeStamp { get { return this.DataObject.Timestamp; } }


        public abstract void DoPaint(DrawingContext context, ChronoListViewPort.IView view, ref Avalonia.Rect rect, EntryMarkState selected);


        //public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s)
        //{
        //    return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font).Width - graphics.MeasureString("<>", font).Width);
        //}

        //public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s, StringFormat format)
        //{
        //    return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font, -1, format).Width - graphics.MeasureString("<>", font, -1, format).Width);
        //}

        public static FormattedText CreateFormattedText(string text, Typeface typeface, double? emSize, IBrush foreground)
        {
            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize.GetValueOrDefault(),
                foreground);
        }

        public static int DrawTextField(
            DrawingContext context, 
            ChronoListViewPort.IView view, 
            IBrush color, string s, ref Rect rect, int width = 0)
        {
            var ts = TextShaper.Current;
            ShapedBuffer shaped = ts.ShapeText(s, new TextShaperOptions(view.NormalFont.GlyphTypeface, view.FontSize));
            var strWidth = (int)(new ShapedTextRun(shaped, new GenericTextRunProperties(view.NormalFont, view.FontSize)).Size.Width);

            if (strWidth > width)
            {
                width = strWidth;
            }
            context.DrawText(
                CreateFormattedText(
                    s, 
                    view.NormalFont, 
                    view.FontSize, 
                    Brushes.Black), 
                new Point((rect.X + width) - strWidth, rect.Y));    // Possibly right-aligned.
            return width;
        }

        //static ChronoListViewEntry()
        //{
        //    m_NormalStringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
        //}

        //private static StringFormat m_NormalStringFormat;
        //public static StringFormat NormalStringFormat { get { return m_NormalStringFormat; } }

    }
}
