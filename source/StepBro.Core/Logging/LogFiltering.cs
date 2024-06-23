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
            return entry.EntryType != LogEntry.Type.Post;
        }

        public static bool NormalWithoutDetailedAndComm(LogEntry entry)
        {
            switch (entry.EntryType)
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
                    return true;
                case LogEntry.Type.Post:
                case LogEntry.Type.Detail:
                case LogEntry.Type.CommunicationOut:
                case LogEntry.Type.CommunicationIn:
                default:
                    return false;
            }
        }
    }
}
