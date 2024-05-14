using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Web;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputHtmlAddon : IOutputFormatterTypeAddon
    {
        static public string Name { get { return "HTML"; } }

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Converts log entry to relative timestamped and indented HTML text."; } }

        public OutputType FormatterType { get { return OutputType.Text; } }

        public IOutputFormatter Create()
        {
            throw new NotImplementedException();
        }

        public IOutputFormatter Create(OutputFormatOptions options, ITextWriter writer = null)
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
            private ITextWriter m_writer;
            public Outputter(ITextWriter writer)
            {
                m_writer = writer;
            }

            public void Dispose()
            {
                if (m_writer != null)
                {
                    m_writer.Dispose();
                    m_writer = null;
                }
            }

            public bool WriteLogEntry(LogEntry entry, DateTime zero)
            {
                var line = entry.ToClearText(zero, false);
                if (line != null)
                {
                    line = HttpUtility.HtmlEncode(line);
                    line = line.Replace("  ", " &nbsp;");
                    m_writer.WriteLine(line + "<br>\r\n");
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
