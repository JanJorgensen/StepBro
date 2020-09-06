using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class DefaultVerdictAttribute : Attribute
    {
        Verdict m_verdict;
        public DefaultVerdictAttribute(Verdict verdict)
        {
            m_verdict = verdict;
        }

        public Verdict Value
        {
            get { return m_verdict; }
        }
    }
}
