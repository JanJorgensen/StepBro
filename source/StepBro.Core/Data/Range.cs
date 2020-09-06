using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class Range
    {
        public enum SectionType
        {
            Value,
            From,
            UpTill,
            FromTo
        }

        public class Section
        {
            SectionType m_type;
            long m_low;
            long m_high;

            internal Section(SectionType type, long low, long high)
            {
                m_type = type;
                m_low = low;
                m_high = high;
            }

            public SectionType Type { get { return m_type; } }
            public long Low { get { return m_low; } }
            public long High { get { return m_high; } }

            public bool Contains(long value)
            {
                switch (m_type)
                {
                    case SectionType.Value:
                        return (value == m_low);
                    case SectionType.From:
                        return (value >= m_low);
                    case SectionType.UpTill:
                        return (value <= m_high);
                    case SectionType.FromTo:
                        return (value >= m_low && value <= m_high);
                    default:
                        throw new NotImplementedException();
                }
            }

            public override string ToString()
            {
                switch (m_type)
                {
                    case SectionType.Value:
                        return m_low.ToString();
                    case SectionType.From:
                        return m_low.ToString() + "..";
                    case SectionType.UpTill:
                        return ".." + m_high.ToString();
                    case SectionType.FromTo:
                        return m_low.ToString() + ".." + m_high.ToString();
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        List<Section> m_conditions;

        private Range(IEnumerable<Section> sections)
        {
            m_conditions = new List<Section>(sections);
        }

        public static Range Create(string range, ref string error)
        {
            try
            {
                return new Range(new List<Section>(ParseString(range)));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public static IEnumerable<Section> ParseString(string range)
        {
            if (String.IsNullOrEmpty(range))
            {
                throw new Exception("Empty range specification.");
            }
            string[] parts = range.Split(',');
            foreach (string p in parts)
            {
                string s = p.Trim();
                if (s.StartsWith(".."))
                {
                    string s1 = s.Substring(2);
                    long n1 = 0;
                    if (!Int64.TryParse(s1, out n1))
                    {
                        throw new Exception("Error in range specification.");
                    }
                    yield return new Section(SectionType.UpTill, 0, n1);
                }
                else if (s.EndsWith(".."))
                {
                    string s1 = s.Substring(0, s.Length - 2);
                    long n1 = 0;
                    if (!Int64.TryParse(s1, out n1))
                    {
                        throw new Exception("Error in range specification.");
                    }
                    yield return new Section(SectionType.From, n1, 0);
                }
                else if (s.Contains(".."))
                {
                    int i = s.IndexOf("..");
                    string s1 = s.Substring(0, i);
                    string s2 = s.Substring(i + 2);
                    long n1 = 0;
                    long n2 = 0;
                    if (!Int64.TryParse(s1, out n1))
                    {
                        throw new Exception("Error in range specification.");
                    }
                    if (!Int64.TryParse(s2, out n2))
                    {
                        throw new Exception("Error in range specification.");
                    }
                    if (n1 >= n2)
                    {
                        throw new Exception("In one \"a..b\" range b is not larger than a.");
                    }
                    yield return new Section(SectionType.FromTo, n1, n2);
                }
                else
                {
                    long n1 = 0;
                    if (!Int64.TryParse(s, out n1))
                    {
                        throw new Exception("Error in range specification.");
                    }
                    yield return new Section(SectionType.Value, n1, 0);
                }
            }
        }

        public bool IsValueWithinRange(long value)
        {
            foreach (Section c in m_conditions)
            {
                if (c.Contains(value)) return true;
            }
            return false;
        }

        public IEnumerable<Section> ListConditions()
        {
            return m_conditions;
            //foreach (Section c in m_conditions)
            //{
            //    yield return c;
            //}
        }

        public override string ToString()
        {
            return String.Join(", ", m_conditions.Select(s => s.ToString()));
        }
    }
}
