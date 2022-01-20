using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputSimpleCleartextAddon : IOutputFormatterTypeAddon
    {
        static public string Name {  get { return "SimpleCleartext"; } } 

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Converts log entry to relative timestamped and indented cleartext."; } }

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
            ITextWriter m_writer;
            public Outputter(ITextWriter writer)
            {
                m_writer = writer;
            }
            public void LogEntry(LogEntry entry, DateTime zero)
            {
                m_writer.WriteLine(entry.ToClearText(zero, false));
            }
        }
    }
}
