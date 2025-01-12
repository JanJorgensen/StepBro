using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.HostSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public class LogViewEntry : ChronoTimestampedListViewEntry
    {
        protected LogEntry m_entry;
        protected long m_sourceIndex;

        private static Pen s_parentPen = new Pen(Color.Orange, 3.0f);
        private static Pen s_siblingPen = new Pen(Color.Yellow, 1.0f);

        public LogViewEntry(LogEntry entry, long index) : base()
        {
            m_entry = entry;
            m_sourceIndex = index;
        }

        public override ITimestampedData DataObject { get { return m_entry; } }

        public override string GetTextForSearchMatching(bool includeExtraFields)
        {
            return m_entry.Text;
        }

        protected virtual string GetHeaderText()
        {
            string headerText = m_entry.EntryType switch
            {
                //LogEntry.Type.Async => "<A>",
                LogEntry.Type.CommunicationOut => "<Out>",
                LogEntry.Type.CommunicationIn => "<In>",
                LogEntry.Type.TaskEntry => "TaskEntry",
                LogEntry.Type.Error => "Error",
                LogEntry.Type.Failure => "Fail",
                LogEntry.Type.UserAction => "UserAction",
                _ => ""
            };
            return headerText;
        }

        protected virtual string GetLocationText()
        {
            return m_entry.Location;
        }
        protected virtual string GetDetailsText()
        {
            return m_entry.Text;
        }

        protected override void PaintRest(PaintEventArgs pe, IChronoListView view, ref Rectangle rect, EntryMarkState markings)
        {
            var color = ((markings & EntryMarkState.Selected) != EntryMarkState.None) ? Brushes.White : GetDefaultEntryTypeColor(m_entry.EntryType);

            string headerText = this.GetHeaderText();
            var width = view.DynamicSettings.LineHeaderWidth;
            var w = DrawTextField(pe.Graphics, view.NormalFont, color, headerText, ChronoListViewEntry.NormalStringFormat, ref rect, width);
            if (w > width)
            {
                width = w;
                view.DynamicSettings.LineHeaderWidth = width;
            }
            rect.X += width + 4 + (m_entry.IndentLevel * 40);

            if ((markings & EntryMarkState.Parent) != EntryMarkState.None)
            {
                pe.Graphics.DrawLine(s_parentPen, new Point(rect.X - 3, rect.Top), new Point(rect.X - 3, rect.Bottom));
            }
            if ((markings & EntryMarkState.Sibling) != EntryMarkState.None)
            {
                pe.Graphics.DrawLine(s_siblingPen, new Point(rect.X - 3, rect.Top), new Point(rect.X - 3, rect.Bottom));
            }

            var location = this.GetLocationText();
            var text = this.GetDetailsText();
            if (location != null)
            {
                w = DrawTextField(pe.Graphics, view.NormalFont, color, location, ChronoListViewEntry.NormalStringFormat, ref rect);
                if (text != null)
                {
                    rect.X += w + 15;
                    w = DrawTextField(pe.Graphics, view.NormalFont, color, "-", ChronoListViewEntry.NormalStringFormat, ref rect);
                    rect.X += w + 15;
                }
            }
            if (text != null)
            {
                w = DrawTextField(pe.Graphics, view.NormalFont, color, text, ChronoListViewEntry.NormalStringFormat, ref rect);
            }
        }

        public static Brush GetDefaultEntryTypeColor(LogEntry.Type type)
        {
            switch (type)
            {
                case LogEntry.Type.Pre:
                case LogEntry.Type.PreHighLevel:
                case LogEntry.Type.TaskEntry:
                    return Brushes.Cyan;
                case LogEntry.Type.Normal:
                case LogEntry.Type.Post:
                    return Brushes.White;
                case LogEntry.Type.Async:
                case LogEntry.Type.CommunicationOut:
                case LogEntry.Type.CommunicationIn:
                    return Brushes.DarkKhaki;
                case LogEntry.Type.Error:
                case LogEntry.Type.Failure:
                    return Brushes.OrangeRed;
                case LogEntry.Type.UserAction:
                    return Brushes.DeepSkyBlue;
                case LogEntry.Type.Detail:
                    return Brushes.LightGray;
                case LogEntry.Type.System:
                    return Brushes.Plum;
                case LogEntry.Type.Special:
                    return Brushes.Pink;
                default:
                    return Brushes.White;
            }

        }
    }
}
