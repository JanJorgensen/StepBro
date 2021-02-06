using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Utils
{
    public static class StringFormatter
    {
        public static string ToMinutesTimestamp(this TimeSpan time)
        {
            return String.Concat(time.Minutes.ToString("00"), ":", time.Seconds.ToString("00"), ".", time.Milliseconds.ToString("000"));
        }
        public static string ToMinutesTimestamp(this DateTime time, DateTime zero)
        {
            if (time >= zero) return (time - zero).ToMinutesTimestamp();
            else return "-" + ((zero - time).ToMinutesTimestamp());
        }
    }
}
