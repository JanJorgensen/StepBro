using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Data;
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

        private static ScriptCallContext ToScriptContext(ICallContext context)
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
#pragma warning disable IDE1006 // Naming Styles
        public static void delay([Implicit] ICallContext context, TimeSpan time)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (context != null && context.LoggingEnabled)
            {
                context.Logger.Log($"{(long)time.TotalMilliseconds}ms");
            }
            if (time >= g_fiveSeconds)
            {
                var reporter = context.StatusUpdater.CreateProgressReporter("delay", time);
                using (reporter)
                {
                    bool skipClicked = false;
                    reporter.AddActionButton("Skip Delay", b => { skipClicked |= b; return b; });
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
        public static void NextProcedureIsHighLevel([Implicit] ICallContext context, string type)
        {
            var internalContext = ToScriptContext(context);
            internalContext.SetNextProcedureAsHighLevel(type);
        }

        [Public]
        public static DataReport StartReport([Implicit] ICallContext context, string ID, string title)
        {
            var internalContext = ToScriptContext(context);
            var report = new DataReport(ID);
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

        [Public]
        public static DataReport GetReport([Implicit] ICallContext context)
        {
            var internalContext = context as ScriptCallContext;
            var report = internalContext.TryGetReport();
            {
                context.ReportError("No has been registered.");
            }
            return report;
        }

        [Public]
        public static void SaveAsPlainText(this DataReport report, string filepath)
        {
            throw new NotImplementedException();
        }

        [Public]
        public static void SaveAsHtml(this DataReport report, string filepath)
        {
            throw new NotImplementedException();
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
        public static string Find(this ILineReader reader, [Implicit] ICallContext context, string text, bool flushIfNotFound = false)
        {
            System.Diagnostics.Debug.WriteLine("Reader.Find: " + text);
            reader.DebugDump();
            //if (context != null && context.LoggingEnabled) context.Logger.Log("Find", "\"" + text + "\"");
            var comparer = StringUtils.CreateComparer(text);
            return Find(reader, context, comparer, flushIfNotFound);
        }

        [Public]
        public static string Find(this ILineReader reader, [Implicit] ICallContext context, Func<string, string> comparer, bool flushIfNotFound = false)
        {
            var peaker = reader.Peak();
            ILineReaderEntry last = null;
            foreach (var entry in peaker)
            {
                var result = comparer(entry.Text);
                if (result != null)
                {
                    reader.Flush(entry);
                    return result;
                }
                last = entry;
            }
            if (flushIfNotFound)
            {
                reader.Flush(last);     // First flush until the last seen entry
                reader.Next();          // ... then also flush the last seen.
            }
            return null;
        }

        [Public]
        public static string Await(this ILineReader reader, [Implicit] ICallContext context, string text, TimeSpan timeout, bool removeFound = true)
        {
            System.Diagnostics.Debug.WriteLine("Reader.Await: " + text);
            if (context != null && context.LoggingEnabled) context.Logger.Log("Await \"" + text + "\"");
            var comparer = StringUtils.CreateComparer(text);
            reader.DebugDump();

            // If the reader has timestamp, set the timeout relative to the time of the current entry; otherwise just use current wall time.
            // TODO: Setting the entry to the readers current lines timestamp, can in extreme cases be way behind, making the await be very inconsistent
            //       for now we just use the current wall time until we find a way for the reader to use the correct entry time.
            // DateTime entry = (reader.LinesHaveTimestamp && reader.Current != null) ? reader.Current.Timestamp : DateTime.Now;
            DateTime entry = DateTime.Now;

            // The time where the timeout expires.
            DateTime to = (timeout == TimeSpan.MaxValue) ? DateTime.MaxValue : entry + timeout;

            //bool sleep = false;
            do
            {
                var result = reader.Find(null, comparer, true);
                if (result != null)
                {
                    if (removeFound)
                    {
                        reader.Next();
                    }
                    return result;
                }
                lock (reader.Sync)
                {
                    if (reader.Current == null)
                    {
                        Monitor.Wait(reader.Sync, 50);
                    }
                }
                //sleep = true;
            } while (DateTime.Now.TimeTill(to) > TimeSpan.Zero);

            if (context != null)
            {
                var readerName = reader.Source.Name;
                if (readerName == null)
                {
                    readerName = "log reader";
                }
                context.ReportFailure($"No entry matching \"{text}\" was found in {readerName}.");
            }
            return null;
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
