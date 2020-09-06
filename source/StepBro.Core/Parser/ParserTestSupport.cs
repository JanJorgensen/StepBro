using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal class ParserTestSupport
    {
        public class DumpParsing : StepBro.Core.Parser.Grammar.StepBroBaseListener
        {
            StringBuilder m_collected = new StringBuilder();
            public override void ExitExpSelect([NotNull] SBP.ExpSelectContext context)
            {
                m_collected.Append(context.expression(0).GetText());
            }
        }
    }
}
