using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal abstract class BinaryOperatorBase
    {
        private static MultiplyOperator g_MultiplyOp = new MultiplyOperator();
        private static DivideOperator g_DivideOp = new DivideOperator();
        private static PlusOperator g_PlusOp = new PlusOperator();
        private static MinusOperator g_MinusOp = new MinusOperator();
        private static ModulusOperator g_ModulusOp = new ModulusOperator();
        private static EqualsOperator g_EqualOp = new EqualsOperator(false);
        private static EqualsOperator g_NotEqualOp = new EqualsOperator(true);
        private static EqualsApproxOperator g_EqualApproxOp = new EqualsApproxOperator();
        private static GreaterThanOperator g_GreaterThanOp = new GreaterThanOperator();
        private static LessThanOperator g_LessThanOp = new LessThanOperator();
        private static GreaterThanOrEqualOperator g_GreaterThanOrEqualOp = new GreaterThanOrEqualOperator();
        private static LessThanOrEqualOperator g_LessThanOrEqualOp = new LessThanOrEqualOperator();
        private static GreaterThanOrEqualOperator g_GreaterThanOrApproxOp = new GreaterThanOrEqualOperator();
        private static LessThanOrApproxOperator g_LessThanOrApproxOp = new LessThanOrApproxOperator();
        private static LogicalAndOperator g_LogicalAndOp = new LogicalAndOperator();
        private static LogicalOrOperator g_LogicalOrOp = new LogicalOrOperator();

        public abstract SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last);

        public static BinaryOperatorBase GetOperator(int op)
        {
            if (op == TSP.STAR) return g_MultiplyOp;
            if (op == TSP.DIV) return g_DivideOp;
            if (op == TSP.PLUS) return g_PlusOp;
            if (op == TSP.MINUS) return g_MinusOp;
            if (op == TSP.PERCENT) return g_ModulusOp;
            if (op == TSP.OP_EQ) return g_EqualOp;
            if (op == TSP.OP_NE) return g_NotEqualOp;
            if (op == TSP.OP_EQ_APPROX) return g_EqualApproxOp;
            if (op == TSP.GT) return g_GreaterThanOp;
            if (op == TSP.LT) return g_LessThanOp;
            if (op == TSP.OP_GE) return g_GreaterThanOrEqualOp;
            if (op == TSP.OP_LE) return g_LessThanOrEqualOp;
            if (op == TSP.OP_GT_APPROX) return g_GreaterThanOrApproxOp;
            if (op == TSP.OP_LT_APPROX) return g_LessThanOrApproxOp;
            if (op == TSP.OP_AND) return g_LogicalAndOp;
            if (op == TSP.OP_OR) return g_LogicalOrOp;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
