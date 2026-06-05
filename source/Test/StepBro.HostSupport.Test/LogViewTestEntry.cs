using StepBro.Core.Logging;
using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Test;

public class LogViewTestEntry : ITimestampedViewEntry
{
    LogEntry m_entry;

    public LogViewTestEntry(ITimestampedData entry)
    {
        m_entry = entry as LogEntry;
    }

    public DateTime TimeStamp => m_entry.Timestamp;

    public ITimestampedData DataObject => m_entry;

    public LogEntry LogEntry => m_entry;

    public string GetTextForSearchMatching(bool includeExtraFields)
    {
        StringBuilder text = new StringBuilder();
        if (includeExtraFields)
        {
            if (!String.IsNullOrEmpty(m_entry.Location))
            {
                text.Append(m_entry.Location);
            }
        }
        if (!String.IsNullOrEmpty(m_entry.Text))
        {
            if (text.Length > 0)
            {
                text.Append(" ");
                text.Append(m_entry.Text);
            }
        }
        return text.ToString();
    }

    public override string ToString()
    {
        return "ViewEntry " + m_entry.ToString();
    }
}

public class LogViewTestEntryFactory : IChronoListViewEntryFactory<LogViewTestEntry>
{
    public void CreatePresentationEntry(ITimestampedData entry, long sourceIndex, Action<LogViewTestEntry> adder)
    {
        adder(new LogViewTestEntry(entry));
    }
}