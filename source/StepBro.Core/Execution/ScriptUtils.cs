using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StepBro.Core;
using StepBro.Core.Addons;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Data.Report;
using StepBro.Core.File;
using static StepBro.Core.Data.StringUtils;

namespace StepBro.Core.Execution
{
    [Public]
    public static class ScriptUtils
    {
        static ScriptUtils()
        {
            g_fiveSeconds = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 5);
            g_50mills = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 50);
            g_80mills = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 80);
        }

        private static TimeSpan g_fiveSeconds;
        private static TimeSpan g_50mills;
        private static TimeSpan g_80mills;

        internal static ScriptCallContext ToScriptContext(ICallContext context)
        {
            var ctx = context;
            while (ctx != null)
            {
                if (ctx is ScriptCallContext) return ctx as ScriptCallContext;
                ctx = (ctx as CallContext).ParentContext;
            }
            return null;
        }

        [Public]
        public static bool UserRequestStop(ICallContext context)
        {
            return ToScriptContext(context).StopRequested();
        }

        [Public]
#pragma warning disable IDE1006 // Naming Styles
        public static void delay([Implicit] ICallContext context, TimeSpan time, string purpose = null)
#pragma warning restore IDE1006 // Naming Styles
        {
            bool isLongDelay = time >= g_fiveSeconds;
            if (context != null && context.LoggingEnabled)
            {
                string purposetext = String.IsNullOrEmpty(purpose) ? "" : " - " + purpose;
                string expiryText = "";
                if (isLongDelay)
                {
                    expiryText = " (expires at " + (DateTime.Now + time).ToString() + ")";
                }
                context.Logger.Log($"{(long)time.TotalMilliseconds}ms{expiryText}{purposetext}");
            }
            if (isLongDelay)
            {
                var reporter = context.StatusUpdater.CreateProgressReporter(String.IsNullOrEmpty(purpose) ? "delay" : purpose, time);
                using (reporter)
                {
                    bool skipClicked = false;
                    bool pauseClicked = false;
                    reporter.AddActionButton("Skip Delay", b => { skipClicked |= b; return b; });
                    reporter.AddActionButton("Pause Delay", b => { pauseClicked |= b; return b; });
                    var entry = DateTime.Now;
                    var timeout = entry + time;
                    var timeLeft = time;
                    while (timeLeft > TimeSpan.Zero && !skipClicked)
                    {
                        if (timeLeft > g_80mills)
                        {
                            System.Threading.Thread.Sleep(g_50mills);   // Still some time left, so sleep a short while.
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(timeLeft);    // Last time to sleep; take whats left.
                        }
                        if (pauseClicked)
                        {
                            context.Logger.LogUserAction("    User pressed the \"Pause delay\" button.");
                            while (pauseClicked)    // Loop as long as the user has paused the delay.
                            {
                                System.Threading.Thread.Sleep(g_50mills);
                            }
                            context.Logger.LogUserAction("    User terminated the delay pause.");
                        }
                        timeLeft = DateTime.Now.TimeTill(timeout);
                    }
                    if (skipClicked)
                    {
                        context.Logger.LogUserAction("    User pressed the \"Skip delay\" button.");
                    }
                }
            }
            else
            {
                System.Threading.Thread.Sleep(time);
            }
        }

        [Public]
        public static DateTime Now()
        {
            return DateTime.Now;
        }

        [Public]
        public static TimeSpan TimeTillNow(DateTime before)
        {
            return DateTime.Now - before;
        }

        [Public]
        public static TimeSpan TimeTill(DateTime time)
        {
            return DateTime.Now.TimeTill(time);
        }

        [Public]
        public static byte[] ToByteArray(this List<long> input)
        {
            return input.ConvertAll(v => (byte)(ulong)v).ToArray();
        }

        [Public]
        public static long ToInt(this string text, [Implicit] ICallContext context)
        {
            long v = 0L;
            if (Int64.TryParse(text, out v))
            {
                return v;
            }
            else
            {
                context?.ReportError($"Could not parse string \"{text}\" to an integer value.");
                return 0;
            }
        }

        [Public]
        public static string GetFullPath(this string filepath, [Implicit] ICallContext context)
        {
            string error = null;
            var result = context.ListShortcuts().GetFullPath(filepath, ref error);
            if (result == null)
            {
                context.ReportError(error);
            }
            return result;
        }

        [Public]
        public static void NextProcedureIsHighLevel([Implicit] ICallContext context, string type)
        {
            var internalContext = ToScriptContext(context);
            internalContext.SetNextProcedureAsHighLevel(type);
        }

        public static DataReport StartReport([Implicit] ICallContext context, string type, string title)
        {
            var internalContext = ToScriptContext(context);
            var report = new DataReport(type, title);
            try
            {
                internalContext.AddReport(report);
                return report;
            }
            catch
            {
                report.Dispose();
                throw;
            }
        }

        public static DataReport GetReport([Implicit] ICallContext context)
        {
            var internalContext = context as ScriptCallContext;
            var report = internalContext.TryGetReport();
            if (report == null)
            {
                context.ReportError("No has been registered.");
            }
            return report;
        }

        public static ReportTestSummary CreateTestSummary([Implicit] ICallContext context)
        {
            var internalContext = (context is ScriptCallContext) ? context as ScriptCallContext : (context as CallContext).ParentContext as ScriptCallContext;
            var report = internalContext.TryGetReport();
            if (report != null)
            {
                return report.CreateTestSummary();
            }
            else
            {
                return new ReportTestSummary(DateTime.Now);
            }
        }

        public static void ReportMeasurement([Implicit] ICallContext context, string id, string instance, string unit, object value)
        {
            var internalContext = (context is ScriptCallContext) ? context as ScriptCallContext : (context as CallContext).ParentContext as ScriptCallContext;
            var report = internalContext.TryGetReport();
            if (report != null)
            {
                DataReport.AddMeasurement(context, report, id, instance, unit, value);
            }
        }

        [Public]
        public static void Save(this DataReport report, string format, string filepath)
        {
            var addon = StepBro.Core.Main.GetService<Core.Api.IAddonManager>().TryGetAddon<IOutputFormatterTypeAddon>(format);
            if (addon != null)
            {
                if (addon.FormatterType == OutputType.Text)
                {
                    //var formatter = addon.Create(false, writer);
                }
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
                //ConsoleWriteErrorLine("Error: Output format \'" + selectedOutputAddon + "\' was not found.");
                //var available = String.Join(", ", StepBroMain.GetService<Core.Api.IAddonManager>().Addons.Where(a => a is IOutputFormatterTypeAddon).Select(a => a.ShortName));
                //ConsoleWriteErrorLine("    Available options: " + available);
                //retval = -1;
                //throw new ExitException();
            }
        }

        #region LineReader

        [Public]
        public static bool Matches(this ILineReader reader, string text)
        {
            var comparer = StringUtils.CreateComparer(text);
            return Matches(reader, comparer);
        }

        [Public]
        public static bool Matches(this ILineReader reader, Func<string, string> comparer)
        {
            ILineReaderEntry entry = reader.Current;
            if (entry == null) return false;
            return comparer(entry.Text) != null;
        }

        [Public]
        public static string Find(this ILineReader reader, [Implicit] ICallContext context, string text, bool flushIfNotFound = false, TimeSpan limit = default, TimeSpan timeout = default)
        {
            System.Diagnostics.Debug.WriteLine("Reader.Find: " + text);
            reader.DebugDump();
            //if (context != null && context.LoggingEnabled) context.Logger.Log("Find", "\"" + text + "\"");
            var comparer = StringUtils.CreateComparer(text);
            return Find(reader, context, comparer, flushIfNotFound, limit, timeout);
        }

        [Public]
        public static string Find(this ILineReader reader, [Implicit] ICallContext context, Func<string, string> comparer, bool flushIfNotFound = false, TimeSpan limit = default, TimeSpan timeout = default)
        {
            // Reference timestamp
            DateTime referenceTime = DateTime.MinValue;

            if (!reader.LinesHaveTimestamp && limit != default)
            {
                throw new NotSupportedException("If the used linereader does not have timestamps, we can not put a limit on the time.");
            }
            else if (reader.LinesHaveTimestamp)
            {
                referenceTime = reader.LatestTimeStamp; // Defaults to DateTime.MinValue
            }

            // Limit timestamp - The latest timestamp we will look for in the log
            DateTime limitTime = (limit == default || limit == TimeSpan.MaxValue) ? DateTime.MaxValue : referenceTime + limit;

            // Timeout timestamp - The amount of realtime we can look for the data, as the data may appear asynchronously in some use cases
            DateTime timeoutTime;

            if (timeout == default)
            {
                timeoutTime = DateTime.MinValue; // Default is we look through the log once and then exit
            }
            else if (timeout == TimeSpan.MaxValue)
            {
                timeoutTime = DateTime.MaxValue;
            }
            else
            {
                timeoutTime = DateTime.Now + timeout;
            }

            var peaker = reader.Peak();

            lock (reader.Sync)
            {
                ILineReaderEntry last = null;
                do
                {
                    if (timeout != default)
                    {
                        lock (reader.Sync)
                        {
                            if (reader.Current == null)
                            {
                                Monitor.Wait(reader.Sync, 50);
                            }
                        }
                    }

                    foreach (var entry in peaker)
                    {
                        // If the entry time was not set, because the reader was empty
                        // we set it to the first entry in the reader.
                        if (referenceTime == DateTime.MinValue && reader.LinesHaveTimestamp)
                        {
                            referenceTime = entry.Timestamp;
                            if (limitTime != DateTime.MaxValue)
                            {
                                limitTime = referenceTime + limit;
                            }
                        }

                        var result = comparer(entry.Text);
                        if (result != null && (!reader.LinesHaveTimestamp || entry.Timestamp.TimeTill(limitTime) > TimeSpan.Zero))
                        {
                            reader.Flush(entry);
                            return result;
                        }
                        last = entry;
                    }
                } while (DateTime.Now.TimeTill(timeoutTime) > TimeSpan.Zero);

                if (flushIfNotFound)
                {
                    reader.Flush(last);     // First flush until the last seen entry
                    reader.Next();          // ... then also flush the last seen.
                }
            }

            return null;
        }

        [Public]
        public static string Await(this ILineReader reader, [Implicit] ICallContext context, string text, TimeSpan timeout, bool skipCurrent = true, bool removeFound = false)
        {
            System.Diagnostics.Debug.WriteLine("Reader.Await: " + text);
            if (context != null && context.LoggingEnabled) context.Logger.Log("Await \"" + text + "\"");
            var comparer = StringUtils.CreateComparer(text);
            reader.DebugDump();

            // If we do not want to match with the current entry
            if (skipCurrent)
            {
                reader.NextUnlessNewEntry();
            }

            // As the purpose of Await is to wait for something in real time, we use DateTime.Now as our entry time
            DateTime entry = DateTime.Now;

            // The latest time the timestamp is allowed to be
            DateTime to = (timeout == TimeSpan.MaxValue) ? DateTime.MaxValue : entry + timeout;

            //bool sleep = false;
            do
            {
                lock (reader.Sync)
                {
                    if (reader.Current == null)
                    {
                        Monitor.Wait(reader.Sync, 50);
                    }
                }

                // We look for the string we want to find
                var result = reader.Find(context, comparer, true);

                // If the string was found
                if (result != null)
                {
                    if (removeFound)
                    {
                        reader.Next();
                    }
                    return result; // If we are here, we have found the result in the allotted time
                }
            } while (DateTime.Now.TimeTill(to) > TimeSpan.Zero); // We use DateTime.Now because we can not be sure that anything is in the log to give us a timestamp

            if (context != null)
            {
                var readerName = reader.Source.Name;
                if (readerName == null)
                {
                    readerName = "log reader";
                }
                context.ReportFailure($"No entry matching \"{text}\" was found in {readerName}.");
            }

            return String.Empty; // Return empty string instead of null to ensure we do not get null reference exceptions
        }

        [Public]
        public static ILineReader ToLineReader(this List<string> list)
        {
            return new StringListLineReader(list);
        }

        [Public]
        public static ILineReader ToLineReader(this List<Tuple<DateTime, string>> list)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region String Enumerable

        [Public]
        public static bool ContainsMatch(this IEnumerable<string> list, string text)
        {
            var comparer = StringUtils.CreateComparer(text);
            foreach (var s in list)
            {
                if (comparer(s) != null) return true;
            }
            return false;
        }

        [Public]
        public static string FindMatch(this IEnumerable<string> list, string text)
        {
            var comparer = StringUtils.CreateComparer(text);
            foreach (var s in list)
            {
                if (comparer(s) != null) return s;
            }
            return null;
        }

        #endregion
    }

    //[Public]
    //public delegate bool LinePredicate(DateTime timestamp, string text);
}
