using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.Controls
{
    public class LogViewEntrySpecial : LogViewEntry
    {
        public LogViewEntrySpecial(LogEntry entry, long index) : base(entry, index)
        {
        }

        protected override string GetHeaderText()
        {
            int i = m_entry.Location.IndexOf(',');
            return m_entry.Location.Substring(i + 1).Trim();
        }

        protected override string GetLocationText()
        {
            return null;
        }
        protected override string GetDetailsText()
        {
            return m_entry.Text;
        }
    }
}
