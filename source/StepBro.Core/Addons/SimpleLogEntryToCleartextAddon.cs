using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class SimpleLogEntryToCleartextAddon : ILogEntryToTextAddon
    {
        static public string Name {  get { return "SimpleCleartext"; } } 

        public string ShortName { get { return Name; } }

        public string FullName { get { return "LogEntryToText." + this.ShortName; } }

        public string Description { get { return "Converts log entry to relative timestamped and indented cleartext."; } }

        public string Convert(LogEntry entry, DateTime zero)
        {
            return entry.ToClearText(zero, false);
        }
    }
}
