using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputAzurePipelineFormatAddon : IOutputFormatterTypeAddon
    {
        public string ShortName { get { return "AzureLog"; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Azure Pipelines formatter."; } }

        public OutputType FormatterType { get { return OutputType.Text; } }

        public IOutputFormatter Create()
        {
            throw new NotImplementedException();
        }

        public IOutputFormatter Create(ITextWriter writer)
        {
            return new Outputter(writer);
        }

        private class Outputter : IOutputFormatter
        {
            readonly ITextWriter m_writer;
            public Outputter(ITextWriter writer)
            {
                m_writer = writer;
            }
            public void LogEntry(LogEntry entry, DateTime zero)
            {
                var txt = entry.ToClearText(zero, false);
                if (txt != null)
                {
                    var prefix = "";
                    switch (entry.EntryType)
                    {
                        case Logging.LogEntry.Type.PreHighLevel:
                            prefix = "##[section]";
                            break;
                        case Logging.LogEntry.Type.Error:
                        case Logging.LogEntry.Type.Failure:
                            prefix = "##[error]";
                            break;
                        //case Logging.LogEntry.Type.UserAction:
                        //case Logging.LogEntry.Type.System:
                        //    prefix = "##[command]";
                        //    break;
                        default:
                            break;
                    }
                    m_writer.WriteLine(prefix + txt);
                }
            }
        }
    }
}
