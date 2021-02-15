using System;
using System.Linq;
using System.Text;

namespace StepBro.Core.Data
{
    public static class StringUtils
    {
        public static string DecodeLiteral(this string s)
        {
            int l = s.Length;
            StringBuilder decoded = new StringBuilder(l);
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
                                ushort uc;
                                if (!UInt16.TryParse(s.Substring(i, 4), System.Globalization.NumberStyles.HexNumber, null, out uc))
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
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t)) return ListToString((System.Collections.IEnumerable)value);
            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        }


        public static string ListToString(System.Collections.IEnumerable list)
        {
            return String.Join(", ", list.Cast<object>().Select(o => (o != null) ? Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture) : "<null>"));
        }

        public static Predicate<string> CreateComparer(this string text)
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
            public bool Matches(string text)
            {
                return text.Equals(m_text, StringComparison.InvariantCulture);
            }
        }
        public class StartsWithStringMatch
        {
            private string m_start;
            public StartsWithStringMatch(string start)
            {
                m_start = start;
            }
            public bool Matches(string text)
            {
                return text.StartsWith(m_start, StringComparison.InvariantCulture);
            }
        }
        public class EndsWithStringMatch
        {
            private string m_end;
            public EndsWithStringMatch(string end)
            {
                m_end = end;
            }
            public bool Matches(string text)
            {
                return text.EndsWith(m_end, StringComparison.InvariantCulture);
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
            public bool Matches(string text)
            {
                return text.StartsWith(m_start, StringComparison.InvariantCulture) &&  text.EndsWith(m_end, StringComparison.InvariantCulture);
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
    }
}
