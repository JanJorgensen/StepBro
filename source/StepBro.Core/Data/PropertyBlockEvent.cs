using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlockEvent : PropertyBlockEntry
    {
        private Verdict? m_verdict;

        public PropertyBlockEvent(string name, Verdict verdict) : base(PropertyBlockEntryType.Event, name)
        {
            m_verdict = verdict;
        }

        public bool IsVerdict { get { return m_verdict.HasValue; } }
        public Verdict Verdict { get { return m_verdict.Value; } }

        public override void GetTestString(StringBuilder text)
        {
            if (m_verdict.HasValue)
            {
                text.Append("on " + this.Name + " : " + m_verdict.ToString());
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
