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
        public override int Height { get { return 20; } }

        public override DateTime TimeStamp { get { return m_entry.Timestamp; } }

        public override object DataObject { get { return m_entry; } }

        protected override void PaintRest(PaintEventArgs pe, ChronoListViewPort.IView view, ref Rectangle rect, bool selected)
        {
        }
    }
}
