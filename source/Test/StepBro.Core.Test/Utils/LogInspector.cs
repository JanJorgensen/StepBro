using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Logging;

namespace StepBroCoreTest.Utils
{
    public class LogInspector : StepBro.Utils.SequenceInspector<string>
    {
        public LogInspector(Logger logger, bool dump = false) :
            base(ListEntries(logger.GetFirst().Item2).Select(LogEntryToString))
        {
            this.SetExpectFailureAction(FailureHandler);
        }

        public LogInspector(ILogger logger, bool dump = false) : this(Logger.Root(logger), dump)
        {

        }

        private static void FailureHandler(string description)
        {
            Assert.Fail(description);
        }

        public static string LogEntryToString(ITimestampedData entry)
        {
            var e = entry as LogEntry;
            if (String.IsNullOrEmpty(e.Location))
            {
                if (String.IsNullOrEmpty(e.Text))
                {
                    return String.Format("{0} - {1}",
                        e.IndentLevel,
                        e.EntryType);
                }
                else
                {
                    return String.Format("{0} - {1} - {2}",
                        e.IndentLevel,
                        e.EntryType,
                        e.Text);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(e.Text))
                {
                    return String.Format("{0} - {1} - {2}",
                        e.IndentLevel,
                        e.EntryType,
                        e.Location);
                }
                else
                {
                    return String.Format("{0} - {1} - {2} - {3}",
                        e.IndentLevel,
                        e.EntryType,
                        e.Location,
                        e.Text);
                }
            }
        }

        private static IEnumerable<LogEntry> ListEntries(ITimestampedData first)
        {
            LogEntry e = first as LogEntry;
            while (e != null)
            {
                yield return e;
                e = e.Next;
            }
        }

        public void DebugDump()
        {
            foreach (var e in m_source)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }
    }
}
