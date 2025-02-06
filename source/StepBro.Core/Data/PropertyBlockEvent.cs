using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlockEvent : PropertyBlockEntry
    {
        private Verdict m_verdict;

        public PropertyBlockEvent(int line, string name, Verdict verdict) : base(line, PropertyBlockEntryType.Event, name)
        {
            m_verdict = verdict;
        }

        protected override uint GetValueHashCode()
        {
            return (uint)m_verdict.GetHashCode();
        }

        public Verdict Verdict { get { return m_verdict; } }

        public override void GetTestString(StringBuilder text)
        {
            text.Append("on " + this.Name + " : " + m_verdict.ToString());
        }

        public override PropertyBlockEntry Clone(bool skipUsedOrApproved = false)
        {
            return new PropertyBlockEvent(this.Line, null, m_verdict).CloneBase(this);
        }

        public override SerializablePropertyBlockEntry CloneForSerialization()
        {
            return new SerializablePropertyBlockEvent() 
            { 
                Name = this.Name,
                Line = this.Line,
                SpecifiedType = this.SpecifiedTypeName, 
                Verdict = m_verdict 
            };
        }
    }
}
