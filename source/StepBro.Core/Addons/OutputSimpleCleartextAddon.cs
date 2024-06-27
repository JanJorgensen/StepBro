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

        public IOutputFormatter Create(OutputFormatOptions options, ITextWriter writer = null)
        {
            if (writer != null)
            {
                return new Outputter(options, writer);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private class Outputter : IOutputFormatter
        {
            private OutputFormatOptions m_options;
            private ITextWriter m_writer;
            
            public Outputter(OutputFormatOptions options, ITextWriter writer)
            {
                m_options = options;
                m_writer = writer;
            }
            public bool WriteLogEntry(LogEntry entry, DateTime zero)
            {
                var line = m_options.UseLocalTime ? entry.ToClearText(entry.Timestamp.ToLocalTime().AsHMSm(), false) : entry.ToClearText(zero, false);
                if (line != null)
                {
                    m_writer.WriteLine(line);
                    return true;
                }
                return false;
            }

            public void WriteReport(DataReport report, bool shouldLogReport = false, string fileName = null)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                if (m_writer != null)
                {
                    m_writer.Dispose();
                    m_writer = null;
                }
            }

            public void Flush()
            {
                m_writer.Flush();
            }
        }
    }
}
