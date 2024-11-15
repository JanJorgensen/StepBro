using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.Controls
{
    public class ChronoListViewPort : Control
    {
        public ChronoListViewPort()
        {
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.White, this.Bounds);

            var formatter = TextFormatter.Current;

            context.DrawText(CreateFormattedText(this, "Ello!!!!", default(Typeface), null, Brushes.Black), new Point(0, 0));
        }


        public static FormattedText CreateFormattedText(Control element, string text, Typeface typeface, double? emSize, IBrush foreground)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (typeface == default)
                typeface = element.CreateTypeface();
            if (emSize == null)
                emSize = TextElement.GetFontSize(element);
            if (foreground == null)
                foreground = TextElement.GetForeground(element);

            return new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize.Value,
                foreground);
        }

    }
}
