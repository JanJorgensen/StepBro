using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal abstract class UnaryOperatorBase
    {
        private static IncreaseOperator g_IncOp = new IncreaseOperator();
        private static DecreaseOperator g_DecOp = new DecreaseOperator();
        private static NotOperator g_NotOp = new NotOperator();
        private static NegateOperator g_NegateOp = new NegateOperator();
        private static ComplementOperator g_ComplementOp = new ComplementOperator();

        public abstract SBExpressionData Resolve(StepBroListener listener, SBExpressionData input, bool opOnLeft);

        public static UnaryOperatorBase GetOperator(int op)
        {
            if (op == TSP.OP_INC) return g_IncOp;
            else if (op == TSP.OP_DEC) return g_DecOp;
            else if (op == TSP.NOT || op == TSP.BANG) return g_NotOp;
            else if (op == TSP.MINUS) return g_NegateOp;
            else if (op == TSP.TILDE) return g_ComplementOp;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
