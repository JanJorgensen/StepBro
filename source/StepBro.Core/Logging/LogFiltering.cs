using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public delegate bool LogFilter(LogEntry entry);

    public static class LogFilters
    {
        public static bool Normal(LogEntry entry)
        {
            return true;
        }

        public static bool NormalWithoutDetailed(LogEntry entry)
        {
            return entry.EntryType != LogEntry.Type.Detail;
        }
    }
}
