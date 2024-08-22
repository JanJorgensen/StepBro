using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.Core.Data
{
    public static class StringUtils
    {
        //private const long TicksPer10thMillisecond = TimeSpan.TicksPerMillisecond / 10;
        private const long TicksPerHalfMillisecond = TimeSpan.TicksPerMillisecond / 2;
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

        public static string ToFileName(this DateTime time, string extension = null)
        {
            return time.ToString("yyyyMMdd_HHmmss") + ((extension != null) ? ("." + extension) : "");
        }
        public static string ToGeneralFormat(this DateTime time)
        {
            return time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool TryParseLiteral(this string value, out object literal)
        {
            if (String.IsNullOrEmpty(value))
            {
                literal = null;
                return false;
            }
            if (Char.IsDigit(value[0]))
            {
                long i;
                double d;
                if (value.StartsWith("0x") && Int64.TryParse(value[2..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out i))
                {
                    literal = i;
                    return true;
                }
                else if (value.TryParseInt64(out i))
                {
                    literal = i;
                    return true;
                }
                else if (value.TryParseFloat(out d))
                {
                    literal = d;
                    return true;
                }
                else
                {
                    literal = null;
                    return false;
                }
            }
            else if (Char.IsLetter(value[0]))
            {
                bool b;
                if (value.TryParse(out b))
                {
                    literal = b;
                    return true;
                }
                literal = value;
                return true;
            }
            else if (value[0] == '\"')
            {
                if (value[value.Length - 1] == '\"')
                {
                    try
                    {
                        string s = value[1..^1].DecodeLiteral();
                        literal = s;
                        return true;
                    }
                    catch
                    {
                        literal = null;
                        return false;
                    }
                }
                else
                {
                    literal = null;
                    return false;
                }
            }
            else
            {
                literal = null;
                return false;
            }
        }

        public static bool TryParseInt64(this string text, out long result)
        {
            char last = text[text.Length - 1];
            if (Char.IsLetter(last))
            {
                int dotIndex = text.IndexOf('.');
                long value;
                long valueFromDecimals = 0;

                long factor = 0;
                int maxDecimals = 0;
                switch (last)
                {
                    case 'K': factor = 1000L; maxDecimals = 3; break;
                    case 'M': factor = 1000000L; maxDecimals = 6; break;
                    case 'G': factor = 1000000000L; maxDecimals = 9; break;
                    case 'T': factor = 1000000000000L; maxDecimals = 12; break;
                    case 'P': factor = 1000000000000000L; maxDecimals = 15; break;
                    default:
                        result = 0L;
                        return false;
                }

                if (dotIndex > 0)
                {
                    value = Int64.Parse(text.Substring(0, dotIndex));
                    string decimals = text.Substring(dotIndex + 1, text.Length - (dotIndex + 2));
                    if (decimals.Length < 1 || decimals.Length > maxDecimals)
                    {
                        result = 0L;
                        return false;
                    }
                    valueFromDecimals = Int64.Parse(decimals);
                    long decimalfactor = factor;
                    for (int i = 0; i < decimals.Length; i++) decimalfactor /= 10;
                    value = value * factor + valueFromDecimals * decimalfactor;
                }
                else
                {
                    value = Int64.Parse(text.Substring(0, text.Length - 1)) * factor;
                }

                result = value;
                return true;
            }
            else
            {
                return Int64.TryParse(text, out result);
            }

        }

        public static bool TryParseFloat(this string text, out double result)
        {
            var strVal = text;
            char last = text[text.Length - 1];
            var factor = 1.0;
            if (Char.IsLetter(last))
            {
                switch (last)
                {
                    case 'P': factor = 1000000000000000.0; break;
                    case 'T': factor = 1000000000000.0; break;
                    case 'G': factor = 1000000000.0; break;
                    case 'M': factor = 1000000.0; break;
                    case 'K': factor = 1000.0; break;
                    case 'm': factor = 0.001; break;
                    case 'u': factor = 0.000001; break;
                    case 'n': factor = 0.000000001; break;
                    case 'p': factor = 0.000000000001; break;
                    default:
                        break;
                }
                strVal = text.Substring(0, text.Length - 1);
            }
            double value = 0.0;
            if (Double.TryParse(strVal, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                result = value * factor;
                return true;
            }
            else
            {
                result = 0.0;
                return true;
            }
        }

        public static string DecodeLiteral(this string s)
        {
            int l = s.Length;
            StringBuilder decoded = new(l);
            for (int i = 0; i < l; i++)
            {
                if (s[i] == '\\')
                {
                    switch (s[++i])
                    {
                        case '\\': decoded.Append('\\'); break;
                        case '\"': decoded.Append('\"'); break;
                        case '\'': decoded.Append('\''); break;
                        case 'r': decoded.Append('\r'); break;
                        case 'n': decoded.Append('\n'); break;
                        case 't': decoded.Append('\t'); break;
                        case 'v': decoded.Append('\v'); break;
                        case 'a': decoded.Append('\a'); break;
                        case 'b': decoded.Append('\b'); break;
                        case 'f': decoded.Append('\f'); break;
                        case 'u':
                        case 'x':
                            i++;
                            if ((l - i) < 4)
                            {
                                throw new FormatException(String.Format("Error in unicode escape sequence at index {0}", i));
                            }
                            else
                            {
                                if (!UInt16.TryParse(s.Substring(i, 4), System.Globalization.NumberStyles.HexNumber, null, out ushort uc))
                                {
                                    throw new FormatException(String.Format("Error in unicode escape sequence at index {0}", i));
                                }
                                decoded.Append((char)uc);
                                i += 3;  // for loop will also increment...
                            }

                            break;
                        default:
                            decoded.Append(s[i]);
                            break;
                    }
                }
                else
                {
                    decoded.Append(s[i]);
                }
            }
            return decoded.ToString();
        }

        public static string EscapeString(this string input)
        {
            return input.Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\"", "\\\"")
                .Replace("\'", "\\\'");
        }

        public static string ObjectToString(object value, bool identifierBare = false)
        {
            if (value == null) return "<null>";
            var t = value.GetType();
            if (t == typeof(string)) return "\"" + ((string)value).EscapeString() + "\"";
            if (t == typeof(Identifier)) return identifierBare ? ((Identifier)value).Name : ("'" + ((Identifier)value).Name + "'");
            if (t == typeof(DateTime)) return ((DateTime)value).ToGeneralFormat();
            if (t == typeof(ArgumentList))
            {
                var list = value as ArgumentList;
                var args = new List<string>();
                foreach (var arg in list)
                {
                    args.Add(ObjectToString(arg));
                }
                if (args.Count > 0)
                {
                    StringBuilder argText = new();
                    argText.Append('(');
                    argText.Append(String.Join(", ", args));
                    argText.Append(')');
                    return argText.ToString();
                }
                else return "(<empty>)";
            }
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t)) return ListToString((System.Collections.IEnumerable)value);
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static bool IsIdentifier(this string value)
        {
            if (String.IsNullOrEmpty(value)) return false;
            if (!Char.IsLetter(value[0]) || value[0] == '_') return false;      // First char must be letter or underscore.
            return value.All(c => c == '_' || Char.IsLetterOrDigit(c) || c == '.');
        }

        public static string ListToString(System.Collections.IEnumerable list)
        {
            return String.Join(", ", list.Cast<object>().Select(o => (o != null) ? Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture) : "<null>"));
        }

        public static string ResultText(this ProcedureResult result, object returnvalue)
        {
            if (returnvalue != null)
            {
                return $"Return value: {StringUtils.ObjectToString(returnvalue)}";
            }
            else
            {
                if (result.ErrorID != null)
                {
                    if (String.IsNullOrEmpty(result.Description))
                    {
                        return $"Result: {result.Verdict}, {result.ErrorID.Name}";
                    }
                    else
                    {
                        return $"Result: {result.Verdict}, {result.ErrorID.Name}, {result.Description}";
                    }
                }
                else
                {
                    if (result.Verdict > Verdict.Unset)
                    {
                        if (String.IsNullOrEmpty(result.Description))
                        {
                            return $"Result: {result.Verdict}";
                        }
                        else
                        {
                            return $"Result: {result.Verdict}, {result.Description}";
                        }
                    }
                    else
                    {
                        return "Success.";
                    }
                }

            }
        }

        public static string ResultText(this IExecutionResult result)
        {
            return result.ProcedureResult.ResultText(result.ReturnValue);
        }

        public static string ToClearText(this LogEntry entry, DateTime zero, bool forceShow = false, bool showErrorAndFailType = true)
        {
            var timestamp = entry.Timestamp.ToSecondsTimestamp(zero);
            return entry.ToClearText(new string(' ', Math.Max(0, 8 - timestamp.Length)) + timestamp, forceShow, showErrorAndFailType);
        }

        public static string ToClearText(this LogEntry entry, string timestamp, bool forceShow = false, bool showErrorAndFailType = true)
        {
            StringBuilder text = new(1000);
            text.Append(timestamp);
            text.Append(new string(' ', 1 + entry.IndentLevel * 3));
            string type = entry.EntryType switch
            {
                LogEntry.Type.Async => "<A> ",
                LogEntry.Type.CommunicationOut => "                                <Out> ",
                LogEntry.Type.CommunicationIn => "                                <In>  ",
                LogEntry.Type.TaskEntry => "TaskEntry - ",
                LogEntry.Type.Error => showErrorAndFailType ? "Error - " : "",
                LogEntry.Type.Failure => showErrorAndFailType ? "Fail - " : "",
                LogEntry.Type.UserAction => "UserAction - ",
                _ => ""
            };
            text.Append(type);

            if (entry.Text != null)
            {
                if (entry.Location != null)
                {
                    text.Append(entry.Location);
                    text.Append(" - ");
                }
                text.Append(entry.Text);
            }
            else
            {
                //if (forceShow || !String.IsNullOrEmpty(type) || entry.Location != null)
                if (forceShow && (entry.Location != null || !String.IsNullOrEmpty(type)))
                {
                    if (entry.Location != null)
                    {
                        text.Append(entry.Location);
                    }
                }
                else
                {
                    return null;    // "Don't show this entry"
                }
            }
            return text.ToString();
        }

        public static string ToStringTextAndLocation(this LogEntry entry)
        {
            StringBuilder text = new();
            if (entry.Text != null)
            {
                if (entry.Location != null)
                {
                    text.Append(entry.Location);
                    text.Append(" - ");
                }
                text.Append(entry.Text);
            }
            else
            {
                if (entry.Location != null)
                {
                    text.Append(entry.Location);
                }
            }
            return text.ToString();
        }

        #region String Comparison

        public static Func<string, string> CreateComparer(this string text)
        {
            var numStars = text.Count(ch => ch == '*');
            if (numStars == 1)
            {
                if (text.StartsWith('*'))
                {
                    return new EndsWithStringMatch(text.Substring(1)).Matches;
                }
                else if (text.EndsWith('*'))
                {
                    return new StartsWithStringMatch(text.Substring(0, text.Length - 1)).Matches;
                }
                else
                {
                    int index = text.IndexOf('*');
                    return new StartsWithAndEndsWithStringMatch(text.Substring(0, index), text.Substring(index + 1)).Matches;
                }
            }
            return new EqualsStringMatch(text).Matches;
        }


        public class EqualsStringMatch
        {
            private string m_text;
            public EqualsStringMatch(string text)
            {
                m_text = text;
            }
            public string Matches(string text)
            {
                return text.Equals(m_text) ? text : null;
            }
        }
        public class StartsWithStringMatch
        {
            private string m_start;
            public StartsWithStringMatch(string start)
            {
                m_start = start;
            }
            public string Matches(string text)
            {
                return text.StartsWith(m_start) ? text.Substring(m_start.Length) : null;
            }
        }
        public class EndsWithStringMatch
        {
            private string m_end;
            public EndsWithStringMatch(string end)
            {
                m_end = end;
            }
            public string Matches(string text)
            {
                return text.EndsWith(m_end) ? text.Substring(0, text.Length - m_end.Length) : null;
            }
        }
        public class StartsWithAndEndsWithStringMatch
        {
            private string m_start;
            private string m_end;
            public StartsWithAndEndsWithStringMatch(string start, string end)
            {
                m_start = start;
                m_end = end;
            }
            public string Matches(string text)
            {
                if (text.StartsWith(m_start) && text.EndsWith(m_end))
                {
                    return text.Substring(m_start.Length, text.Length - (m_start.Length + m_end.Length));
                }
                else return null;
            }
        }
        public class ContainsStringMatch
        {
            private string m_text;
            public ContainsStringMatch(string text)
            {
                m_text = text;
            }
            public bool Matches(string text)
            {
                return text.Contains(m_text, StringComparison.InvariantCulture);
            }
        }

        #endregion
    }
}
