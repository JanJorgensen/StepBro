using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Data;

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

        [Public]
#pragma warning disable IDE1006 // Naming Styles
        public static void delay([Implicit] ICallContext context, TimeSpan time)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (time >= g_fiveSeconds)
            {
                if (context != null && context.LoggingEnabled)
                {
                    context.Logger.Log("delay", $"{(long)time.TotalMilliseconds}ms");
                }
                var reporter = context.StatusUpdater.CreateProgressReporter("", time);
                using (reporter)
                {
                    bool skipClicked = false;
                    reporter.AddActionButton("Skip Delay", Controls.ButtonActivationType.ToggleWhenClicked, b => { skipClicked |= b; });
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
        public static DataReport StartReport([Implicit] ICallContext context, string ID, string title)
        {
            var internalContext = context as ScriptCallContext;
            return internalContext.AddReport(new DataReport(ID));
        }

        [Public]
        public static DataReport GetReport([Implicit] ICallContext context, string ID)
        {
            var internalContext = context as ScriptCallContext;
            var report = internalContext.ListReports().FirstOrDefault(r => String.Equals(ID, r.ID, StringComparison.InvariantCulture));
            if (report == null)
            {
                context.ReportError(description: "Failed to find report named \"" + ID + "\".");
            }
            return report;
        }

        [Public]
        public static void SaveAsPlainText(this DataReport report, string filepath)
        {
            throw new NotImplementedException();
        }
    }
}
