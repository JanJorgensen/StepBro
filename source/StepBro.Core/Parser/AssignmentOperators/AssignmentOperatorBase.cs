using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser.AssignmentOperators
{
    internal abstract class AssignmentOperatorBase
    {
        private static AssignmentOperator g_AssignOp = new AssignmentOperator();
        private static AddAssignmentOperator g_AddAssignOp = new AddAssignmentOperator();
        private static SubAssignmentOperator g_SubAssignOp = new SubAssignmentOperator();

        public abstract SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last);

        public static AssignmentOperatorBase GetOperator(int op)
        {
            if (op == SBP.ASSIGNMENT) return g_AssignOp;
            else if (op == SBP.OP_ADD_ASSIGNMENT) return g_AddAssignOp;
            else if (op == SBP.OP_SUB_ASSIGNMENT) return g_SubAssignOp;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
