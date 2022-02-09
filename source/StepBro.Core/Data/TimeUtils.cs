using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public static class TimeUtils
    {
        public static TimeSpan TimeTill(this DateTime now, DateTime time)
        {
            if (now >= time) return TimeSpan.Zero;
            else return time - now;
        }

        public static TimeSpan TimeTill(DateTime time)
        {
            return DateTime.Now.TimeTill(time);
        }

        public static TimeSpan Multiply(this TimeSpan ts, long multiplier)
        {
            return TimeSpan.FromTicks(ts.Ticks * multiplier);
        }

        public static TimeSpan Multiply(this TimeSpan ts, int multiplier)
        {
            return TimeSpan.FromTicks(ts.Ticks * multiplier);
        }

        public static TimeSpan Multiply(this TimeSpan ts, double multiplier)
        {
            return TimeSpan.FromTicks((long)Math.Round(ts.Ticks * multiplier));
        }

        public static TimeSpan Divide(this TimeSpan ts, long divider)
        {
            return TimeSpan.FromTicks((long)(ts.Ticks / (double)divider));
        }

        public static TimeSpan Divide(this TimeSpan ts, int divider)
        {
            return TimeSpan.FromTicks((long)(ts.Ticks / (double)divider));
        }

        public static TimeSpan Divide(this TimeSpan ts, double divider)
        {
            return TimeSpan.FromTicks((long)(ts.Ticks / divider));
        }

        public static bool TryParse(string s, out TimeSpan result)
        {
            if (String.IsNullOrEmpty(s))
            {
                result = TimeSpan.Zero;
                return false;
            }
            if (Char.IsDigit(s[0]))
            {
                if (s.EndsWith("ms"))
                {
                    int decimalIndex = s.IndexOf('.');
                    if (decimalIndex > 0)
                    {
                        long ticks = long.Parse(s.Substring(0, decimalIndex)) * TimeSpan.TicksPerMillisecond;
                        var decimalString = s.Substring(decimalIndex + 1, s.Length - decimalIndex - 3);
                        long decimalValue = long.Parse(decimalString);
                        int decimalWidth = decimalString.Length;
                        switch (decimalWidth)
                        {
                            case 1: ticks += (decimalValue * TimeSpan.TicksPerMillisecond / 10); break;
                            case 2: ticks += (decimalValue * TimeSpan.TicksPerMillisecond / 100); break;
                            case 3: ticks += (decimalValue * TimeSpan.TicksPerMillisecond / 1000); break;
                            case 4: ticks += (decimalValue * TimeSpan.TicksPerMillisecond / 10000); break;
                            default:
                                throw new FormatException("The maximum number of decimals is 4, for a timespan value in milliseconds.");
                        }
                        result = TimeSpan.FromTicks(ticks);
                        return true;
                    }
                    else
                    {
                        long v = long.Parse(s.Substring(0, s.Length - 2));
                        result = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * v);
                        return true;
                    }
                }
                else if (s.EndsWith("s"))
                {
                    int decimalIndex = s.IndexOf('.');
                    if (decimalIndex > 0)
                    {
                        long ticks = long.Parse(s.Substring(0, decimalIndex)) * TimeSpan.TicksPerSecond;
                        var decimalString = s.Substring(decimalIndex + 1, s.Length - decimalIndex - 2);
                        ticks += TicksFromDecimalString(decimalString);
                        result = TimeSpan.FromTicks(ticks);
                        return true;
                    }
                    else
                    {
                        long v = long.Parse(s.Substring(0, s.Length - 1));
                        result = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * v);
                        return true;
                    }
                }
            }
            else if (s[0] == '@' && s.Length > 1 && Char.IsDigit(s[1]))
            {
                int sep1 = s.IndexOf(':');
                if (sep1 > 0)
                {
                    int sep2 = s.IndexOf(':', sep1 + 1);
                    if (sep2 > 0)
                    {
                        int dot = s.IndexOf('.', sep2);
                        if (dot > 0)
                        {
                            result = new TimeSpan(
                                int.Parse(s.Substring(1, sep1 - 1)),
                                int.Parse(s.Substring(sep1 + 1, sep2 - sep1 - 1)),
                                int.Parse(s.Substring(sep2 + 1, dot - sep2 - 1)))
                                + TimeSpan.FromTicks(TicksFromDecimalString(s.Substring(dot + 1)));
                            return true;
                        }
                        else
                        {
                            result = new TimeSpan(
                                int.Parse(s.Substring(1, sep1 - 1)),
                                int.Parse(s.Substring(sep1 + 1, sep2 - sep1 - 1)),
                                int.Parse(s.Substring(sep2 + 1)));
                            return true;
                        }
                    }
                    else
                    {
                        int dot = s.IndexOf('.', sep1);
                        if (dot > 0)
                        {
                            result = new TimeSpan(
                                0,
                                int.Parse(s.Substring(1, sep1 - 1)),                // From after the '@' until ':'
                                int.Parse(s.Substring(sep1 + 1, dot - sep1 - 1)))   // Between the two ':'
                                + TimeSpan.FromTicks(TicksFromDecimalString(s.Substring(dot + 1)));     // From '.' and rest
                            return true;
                        }
                        else
                        {
                            result = new TimeSpan(
                            0,
                            int.Parse(s.Substring(1, sep1 - 1)),    // From after the '@' until ':'
                            int.Parse(s.Substring(sep1 + 1)));      // From ':' and rest
                            return true;
                        }
                    }
                }
            }
            result = TimeSpan.Zero;
            return false;
        }

        public static TimeSpan ParseTimeSpan(string s)
        {
            TimeSpan ts;
            if (TryParse(s, out ts))
            {
                return ts;
            }
            else
            {
                throw new FormatException();
            }
        }

        private static long TicksFromDecimalString(string decimals)
        {
            long decimalValue = long.Parse(decimals);
            int decimalWidth = decimals.Length;
            switch (decimalWidth)
            {
                case 1: return (decimalValue * TimeSpan.TicksPerSecond / 10);
                case 2: return (decimalValue * TimeSpan.TicksPerSecond / 100);
                case 3: return (decimalValue * TimeSpan.TicksPerSecond / 1000);
                case 4: return (decimalValue * TimeSpan.TicksPerSecond / 10000);
                case 5: return (decimalValue * TimeSpan.TicksPerSecond / 100000);
                case 6: return (decimalValue * TimeSpan.TicksPerSecond / 1000000);
                case 7: return (decimalValue * TimeSpan.TicksPerSecond / 10000000);
                default:
                    throw new FormatException("The maximum number of decimals is 7, for a timespan value in seconds.");
            }
        }

        public static DateTime ParseDateTime(string s, int offset)
        {
            // DateTimeLiteral: '@' DateTimeYear MINUS DateTimeMonth MINUS DateTimeDayOfMonth(' ' TimeOfDay('UTC') ? ) ? ;
            // @1999-01-01 15:23:07.762

            DateTimeKind kind = DateTimeKind.Unspecified;
            TimeSpan dt = TimeSpan.Zero;
            var iColon = s.IndexOf(':', offset + 10);
            if (iColon > 0)
            {
                int end = s.IndexOf(' ', offset + 15);
                if (end > 0)
                {
                    dt = TimeSpan.Parse(s.Substring(offset + 11, end - offset - 11));
                    if (s.EndsWith("UTC")) { kind = DateTimeKind.Utc; }
                    else if (s.EndsWith("Local")) { kind = DateTimeKind.Local; }
                }
                else
                {
                    dt = TimeSpan.Parse(s.Substring(offset + 11));
                }
            }
            DateTime time = new DateTime(int.Parse(s.Substring(offset, 4)), int.Parse(s.Substring(offset + 5, 2)), int.Parse(s.Substring(offset + 8, 2)), 0, 0, 0, kind);
            time += dt;
            return time;
        }
    }
}
