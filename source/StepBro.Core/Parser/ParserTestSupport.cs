using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser
{
    internal class ParserTestSupport
    {
        public class DumpParsing : TSharpBaseListener
        {
            StringBuilder m_collected = new StringBuilder();
            public override void ExitExpSelect([NotNull] TSP.ExpSelectContext context)
            {
                m_collected.Append(context.expression(0).GetText());
            }
        }
    }
}
