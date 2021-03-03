using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Utils
{
    public static class StringFormatter
    {
        private const long TicksPer10thMillisecond = TimeSpan.TicksPerMillisecond * 10;
        private const long TicksPerHalfMillisecond = TimeSpan.TicksPerMillisecond * 2;
        private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;

        public static string ToMinutesTimestamp(this TimeSpan time)
        {
            return String.Concat(time.Minutes.ToString("00"), ":", time.Seconds.ToString("00"), ".", time.Milliseconds.ToString("000"));
        }

        public static string ToMinutesTimestamp(this DateTime time, DateTime zero)
        {
            if (time >= zero) return (time - zero).ToMinutesTimestamp();
            else return "-" + ((zero - time).ToMinutesTimestamp());
        }

        public static string ToSecondsTimestamp(this DateTime time, DateTime zero)
        {
            var ticks = time.Ticks;
            var ticksZero = zero.Ticks;
            long first, second;
            string prefix = "";
            if (ticks >= ticksZero)
            {
                first = ticksZero;
                second = ticks;
            }
            else
            {
                first = ticks;
                second = ticksZero;
                prefix = "-";
            }
            var ms = ((second - first) + TicksPerHalfMillisecond) / TicksPerMillisecond;
            var seconds = ms / 1000;
            var fraction = ms % 1000;
            return String.Concat(prefix, seconds.ToString(), ".", fraction.ToString("000")); 
        }

        public static string ToFileName(this DateTime time)
        {
            return time.ToString("yyyyMMdd_HHmmss");
        }
    }
}
