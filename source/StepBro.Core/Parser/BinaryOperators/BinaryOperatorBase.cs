using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

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
        private static GreaterThanOrApproxOperator g_GreaterThanOrApproxOp = new GreaterThanOrApproxOperator();
        private static LessThanOrApproxOperator g_LessThanOrApproxOp = new LessThanOrApproxOperator();
        private static LogicalAndOperator g_LogicalAndOp = new LogicalAndOperator();
        private static LogicalOrOperator g_LogicalOrOp = new LogicalOrOperator();
        private static BitwiseAndOperator g_BitwiseAndOp = new BitwiseAndOperator();
        private static BitwiseOrOperator g_BitwiseOrOp = new BitwiseOrOperator();
        private static BitwiseXorOperator g_BitwiseXorOp = new BitwiseXorOperator();
        private static ShiftRightOperator g_ShiftRightOp = new ShiftRightOperator();
        private static ShiftLeftOperator g_ShiftLeftOp = new ShiftLeftOperator();

        public abstract SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last);

        public static BinaryOperatorBase GetOperator(int op)
        {
            if (op == SBP.STAR) return g_MultiplyOp;
            if (op == SBP.DIV) return g_DivideOp;
            if (op == SBP.PLUS) return g_PlusOp;
            if (op == SBP.MINUS) return g_MinusOp;
            if (op == SBP.PERCENT) return g_ModulusOp;
            if (op == SBP.OP_EQ) return g_EqualOp;
            if (op == SBP.OP_NE) return g_NotEqualOp;
            if (op == SBP.OP_EQ_APPROX) return g_EqualApproxOp;
            if (op == SBP.GT) return g_GreaterThanOp;
            if (op == SBP.LT) return g_LessThanOp;
            if (op == SBP.OP_GE) return g_GreaterThanOrEqualOp;
            if (op == SBP.OP_LE) return g_LessThanOrEqualOp;
            if (op == SBP.OP_GT_APPROX) return g_GreaterThanOrApproxOp;
            if (op == SBP.OP_LT_APPROX) return g_LessThanOrApproxOp;
            if (op == SBP.OP_AND) return g_LogicalAndOp;
            if (op == SBP.OP_OR) return g_LogicalOrOp;
            if (op == SBP.AMP) return g_BitwiseAndOp;
            if (op == SBP.BITWISE_OR) return g_BitwiseOrOp;
            if (op == SBP.CARET) return g_BitwiseXorOp;
            if (op == SBP.OP_RIGHT_SHIFT) return g_ShiftRightOp;
            if (op == SBP.OP_LEFT_SHIFT) return g_ShiftLeftOp;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
