using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal abstract class UnaryOperatorBase
    {
        protected static MethodInfo s_UnaryOperatorGlobalVariable = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.UnaryOperatorGlobalVariable));

        private static IncreaseOperator g_IncOp = new IncreaseOperator();
        private static DecreaseOperator g_DecOp = new DecreaseOperator();
        private static NotOperator g_NotOp = new NotOperator();
        private static NegateOperator g_NegateOp = new NegateOperator();
        private static ComplementOperator g_ComplementOp = new ComplementOperator();

        public abstract SBExpressionData Resolve(StepBroListener listener, SBExpressionData input, bool opOnLeft);

        public static UnaryOperatorBase GetOperator(int op)
        {
            if (op == SBP.OP_INC) return g_IncOp;
            else if (op == SBP.OP_DEC) return g_DecOp;
            else if (op == SBP.NOT || op == SBP.BANG) return g_NotOp;
            else if (op == SBP.MINUS) return g_NegateOp;
            else if (op == SBP.TILDE) return g_ComplementOp;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
