using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public abstract class ChronoListViewEntry
    {
        public abstract object DataObject { get; }
        public abstract int Height { get; }
        public abstract void DoPaint(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, bool selected);


        public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s)
        {
            return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font).Width - graphics.MeasureString("<>", font).Width);
        }

        public static int GetWidth(System.Drawing.Graphics graphics, Font font, string s, StringFormat format)
        {
            return (int)Math.Ceiling(graphics.MeasureString("<>" + s, font, -1, format).Width - graphics.MeasureString("<>", font, -1, format).Width);
        }

        static ChronoListViewEntry()
        {
            m_NormalStringFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
        }

        private static StringFormat m_NormalStringFormat;
        public static StringFormat NormalStringFormat { get { return m_NormalStringFormat; } }

    }
}
