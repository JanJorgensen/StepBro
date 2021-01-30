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
        public LogInspector(LoggerRoot logger, bool dump = false) :
            base(ListEntries(logger.GetOldestEntry()).Select(LogEntryToString))
        {
            this.SetExpectFailureAction(FailureHandler);
        }

        public LogInspector(ILogger logger, bool dump = false) : this(LoggerRoot.Root(logger), dump)
        {

        }

        private static void FailureHandler(string description)
        {
            Assert.Fail(description);
        }

        public static string LogEntryToString(LogEntry entry)
        {
            if (String.IsNullOrEmpty(entry.Location))
            {
                if (String.IsNullOrEmpty(entry.Text))
                {
                    return String.Format("{0} - {1}",
                        entry.IndentLevel,
                        entry.EntryType);
                }
                else
                {
                    return String.Format("{0} - {1} - {2}",
                        entry.IndentLevel,
                        entry.EntryType,
                        entry.Text);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(entry.Text))
                {
                    return String.Format("{0} - {1} - {2}",
                        entry.IndentLevel,
                        entry.EntryType,
                        entry.Location);
                }
                else
                {
                    return String.Format("{0} - {1} - {2} - {3}",
                        entry.IndentLevel,
                        entry.EntryType,
                        entry.Location,
                        entry.Text);
                }
            }
        }

        private static IEnumerable<LogEntry> ListEntries(LogEntry first)
        {
            LogEntry e = first;
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
