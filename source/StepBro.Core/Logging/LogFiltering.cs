using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class LogFilters
    {
        public static bool ShowAll(ITimestampedData entry)
        {
            return true;
        }
        public static bool Normal(ITimestampedData entry)
        {
            var e = entry as LogEntry;
            if (e.EntryType == LogEntry.Type.Component) return false;
            return (e.EntryType != LogEntry.Type.Post || e.IndentLevel < 3) && (e.Text != null || e.Location != null);
        }

        public static bool Level2Max(ITimestampedData entry)
        {
            return ((entry as LogEntry).IndentLevel < 2);
        }
        public static bool Level3Max(ITimestampedData entry)
        {
            return ((entry as LogEntry).IndentLevel < 3);
        }
        public static bool Level4Max(ITimestampedData entry)
        {
            return ((entry as LogEntry).IndentLevel < 4);
        }
        public static bool Level5Max(ITimestampedData entry)
        {
            return ((entry as LogEntry).IndentLevel < 5);
        }

        public static bool NormalWithoutDetailedAndComm(ITimestampedData entry)
        {
            var e = (LogEntry)entry;
            switch (e.EntryType)
            {   
                case LogEntry.Type.Normal:
                case LogEntry.Type.Pre:
                case LogEntry.Type.PreHighLevel:
                case LogEntry.Type.TaskEntry:
                case LogEntry.Type.Async:
                case LogEntry.Type.Error:
                case LogEntry.Type.Failure:
                case LogEntry.Type.UserAction:
                case LogEntry.Type.System:
                    return e.Text != null || e.Location != null;
                case LogEntry.Type.Component:
                case LogEntry.Type.Post:
                case LogEntry.Type.Detail:
                case LogEntry.Type.CommunicationOut:
                case LogEntry.Type.CommunicationIn:
                case LogEntry.Type.Special:
                default:
                    return false;
            }
        }
    }
}
