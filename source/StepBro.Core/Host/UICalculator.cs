using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    public class UICalculator : ServiceBase<UICalculator, UICalculator>
    {
        private object m_lastResult = 9;

        internal UICalculator(out IService serviceAccess) : base("UICalculator", out serviceAccess)
        {

        }

        public object Evaluate(string expression)
        {
            return m_lastResult;
        }

        public object LastResult { get { return m_lastResult; } }
    }
}
