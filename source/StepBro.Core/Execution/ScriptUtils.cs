using StepBro.Core.Api;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    [Public]
    public static class ScriptUtils
    {
        static ScriptUtils()
        {
            g_fiveSeconds = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * 5);
        }

        private static TimeSpan g_fiveSeconds;

        [Public]
        public static void delay([Implicit] ICallContext context, TimeSpan time)
        {
            if (time >= g_fiveSeconds)
            {
                context.Logger.Log("delay", "TODO: update state info when delay > 5s");
                System.Threading.Thread.Sleep(time);
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
        public static byte[] ToByteArray(this List<long> input)
        {
            return input.ConvertAll(v => (byte)(ulong)v).ToArray();
        }
    }
}
