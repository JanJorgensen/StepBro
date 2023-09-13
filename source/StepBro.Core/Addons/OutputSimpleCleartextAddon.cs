using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputSimpleCleartextAddon : IOutputFormatterTypeAddon
    {
        static public string Name { get { return "SimpleCleartext"; } }

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Converts log entry to relative timestamped and indented cleartext."; } }

        public OutputType FormatterType { get { return OutputType.Text; } }

        public IOutputFormatter Create()
        {
            throw new NotImplementedException();
        }

        public IOutputFormatter Create(bool createHighLevelLogSections, ITextWriter writer = null)
        {
            if (writer != null)
            {
                return new Outputter(writer);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private class Outputter : IOutputFormatter
        {
            readonly ITextWriter m_writer;
            public Outputter(ITextWriter writer)
            {
                m_writer = writer;
            }
            public bool WriteLogEntry(LogEntry entry, DateTime zero)
            {
                var line = entry.ToClearText(zero, false);
                if (line != null)
                {
                    m_writer.WriteLine(line);
                    return true;
                }
                return false;
            }

            public void WriteReport(DataReport report)
            {
                throw new NotImplementedException();
            }
        }
    }
}
