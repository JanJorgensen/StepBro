using StepBro.Core.Api;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Addons
{
    /// <summary>
    /// Common interface for all addons that can convert a log entry to a textual representation.
    /// </summary>
    public interface ILogEntryToTextAddon : IAddon
    {
        /// <summary>
        /// Creates a cleartext representation for a specified log entry.
        /// </summary>
        /// <param name="entry">The log entry to create a textual representation for.</param>
        /// <param name="zero">The start time for the returned relatime time stamp.</param>
        /// <returns>Textual representation of the specified log entry, or <code>null</code> if the entry should not be shown.</returns>
        string Convert(LogEntry entry, DateTime zero);
    }
}
