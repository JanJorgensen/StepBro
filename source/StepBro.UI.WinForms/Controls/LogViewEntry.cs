using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public class LogViewEntry : ChronoTimestampedListViewEntry
    {
        private LogEntry m_entry;
        private long m_sourceIndex;

        public LogViewEntry(LogEntry entry, long index) : base()
        {
            m_entry = entry;
            m_sourceIndex = index;
        }

        public override DateTime TimeStamp { get { return m_entry.Timestamp; } }

        public override object DataObject { get { return m_entry; } }

        protected override void PaintRest(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, EntrySelectionState selected)
        {
            var color = (selected != EntrySelectionState.Not) ? Brushes.White : GetDefaultEntryTypeColor(m_entry.EntryType);

            string headerText = m_entry.EntryType switch
            {
                LogEntry.Type.Async => "<A>",
                LogEntry.Type.CommunicationOut => "<Out>",
                LogEntry.Type.CommunicationIn => "<In>",
                LogEntry.Type.TaskEntry => "TaskEntry",
                LogEntry.Type.Error => "Error",
                LogEntry.Type.Failure => "Fail",
                LogEntry.Type.UserAction => "UserAction",
                _ => ""
            };
            var width = view.ViewSettings.LineHeaderWidth;
            var w = DrawTextField(pe.Graphics, view.NormalFont, color, headerText, ChronoListViewEntry.NormalStringFormat, ref rect, width);
            if (w > width)
            {
                width = w;
                view.ViewSettings.LineHeaderWidth = width;
            }
            rect.X += width + 4 + (m_entry.IndentLevel * 40);

            if (m_entry.Location != null)
            {
                w = DrawTextField(pe.Graphics, view.NormalFont, color, m_entry.Location, ChronoListViewEntry.NormalStringFormat, ref rect);
                if (m_entry.Text != null)
                {
                    rect.X += w + 15;
                    w = DrawTextField(pe.Graphics, view.NormalFont, color, "-", ChronoListViewEntry.NormalStringFormat, ref rect);
                    rect.X += w + 15;
                }
            }
            if (m_entry.Text != null)
            {
                w = DrawTextField(pe.Graphics, view.NormalFont, color, m_entry.Text, ChronoListViewEntry.NormalStringFormat, ref rect);
            }
        }

        public Brush GetDefaultEntryTypeColor(LogEntry.Type type)
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
                    return Brushes.LightGoldenrodYellow;
                case LogEntry.Type.Error:
                case LogEntry.Type.Failure:
                    return Brushes.Red;
                case LogEntry.Type.UserAction:
                    return Brushes.Blue;
                case LogEntry.Type.Detail:
                    return Brushes.DarkGray;
                case LogEntry.Type.System:
                    return Brushes.LightBlue;
                default:
                    return Brushes.White;
            }

        }
    }
}
