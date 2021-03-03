using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    internal delegate bool ExpectStatementEvaluationDelegate(IScriptCallContext context, out string actualValue);
}
