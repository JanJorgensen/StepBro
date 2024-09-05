using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class LogFilters
    {
        public static bool ShowAll(LogEntry entry)
        {
            return true;
        }
        public static bool Normal(LogEntry entry)
        {
            return (entry.EntryType != LogEntry.Type.Post || entry.IndentLevel < 3) && (entry.Text != null || entry.Location != null);
        }

        public static bool Level2Max(LogEntry entry)
        {
            return (entry.IndentLevel < 2);
        }
        public static bool Level3Max(LogEntry entry)
        {
            return (entry.IndentLevel < 3);
        }
        public static bool Level4Max(LogEntry entry)
        {
            return (entry.IndentLevel < 4);
        }
        public static bool Level5Max(LogEntry entry)
        {
            return (entry.IndentLevel < 5);
        }

        public static bool NormalWithoutDetailedAndComm(LogEntry entry)
        {
            switch (entry.EntryType)
            {   
                case LogEntry.Type.Normal:
                case LogEntry.Type.Pre:
                case LogEntry.Type.PreHighLevel:
                case LogEntry.Type.TaskEntry:
                //case LogEntry.Type.Async:
                case LogEntry.Type.Error:
                case LogEntry.Type.Failure:
                case LogEntry.Type.UserAction:
                case LogEntry.Type.System:
                    return entry.Text != null || entry.Location != null;
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
