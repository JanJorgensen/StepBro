using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoListViewEntry
    {
        public abstract object DataObject { get; }
        public abstract void DoPaint(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, EntrySelectionState selected);


        public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s)
        {
            return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font).Width - graphics.MeasureString("<>", font).Width);
        }

        public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s, StringFormat format)
        {
            return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font, -1, format).Width - graphics.MeasureString("<>", font, -1, format).Width);
        }

        public static int DrawTextField(System.Drawing.Graphics graphics, Font font, Brush color, string s, StringFormat format, ref Rectangle rect, int width = 0)
        {
            var strWidth = (int)(graphics.MeasureString(s, font, rect.Width, ChronoListViewEntry.NormalStringFormat).Width);
            if (strWidth > width)
            {
                width = strWidth;
            }
            graphics.DrawString(
                s,
                font,
                color,
                (rect.X + width) - strWidth, // Right adjust (maybe).
                rect.Y,
                format);
            return width;
        }

        static ChronoListViewEntry()
        {
            m_NormalStringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
        }

        private static StringFormat m_NormalStringFormat;
        public static StringFormat NormalStringFormat { get { return m_NormalStringFormat; } }

    }
}
