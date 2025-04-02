using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputStepBroPersistanceAddon : IOutputFormatterTypeAddon
    {
        static public string Name { get { return "StepBroPersistance"; } }

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Converts execution log or report to a no-loss StepBro persistance format (*.sbl or *.sbr files)."; } }

        public OutputType FormatterType { get { return OutputType.Text; } }

        public string LogFileExtension { get { return "sbl"; } }
        public string ReportFileExtension { get { return "sbr"; } }

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
                m_writer.WriteLine(entry.ToPersistanceString());
                return true;
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
