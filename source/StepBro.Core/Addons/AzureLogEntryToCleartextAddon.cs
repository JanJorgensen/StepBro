using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class AzureLogEntryToCleartextAddon : ILogEntryToTextAddon
    {
        public string ShortName { get { return "AzureLog"; } }

        public string FullName { get { return "LogEntryToText." + this.ShortName; } }

        public string Description { get { return "Azure Pipelines format log entry converter."; } }

        public string Convert(LogEntry entry, DateTime zero)
        {
            var prefix = "";
            switch (entry.EntryType)
            {
                case LogEntry.Type.Pre:
                    //prefix = "##[group]";
                    prefix = "##[section]";
                    break;
                //case LogEntry.Type.Post:
                //    prefix = "##[endgroup]";
                //    break;
                case LogEntry.Type.Error:
                case LogEntry.Type.Failure:
                    prefix = "##[error]";
                    break;
                case LogEntry.Type.UserAction:
                case LogEntry.Type.System:
                    prefix = "##[command]";
                    break;
                default:
                    break;
            }
            var txt = entry.ToClearText(zero, false);
            if (txt != null) return prefix + txt;
            else return (String.IsNullOrEmpty(prefix) ? null : prefix);
        }
    }
}
