using StepBro.Core.Api;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Addons
{
    public enum OutputType { Text, Console }

    public interface ITextWriter
    {
        void Write(string text);
        void WriteLine(string text);
    }

    public interface IOutputFormatterTypeAddon : IAddon
    {
        OutputType FormatterType { get; }
        IOutputFormatter Create();
        IOutputFormatter Create(ITextWriter writer);
    }

    public interface IOutputFormatter
    {
        /// <summary>
        /// Creates a cleartext representation for a specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to create a textual representation for.</param>
        /// <param name="zero">The start time for the returned relatime time stamp.</param>
        void LogEntry(LogEntry entry, DateTime zero);
    }
}
