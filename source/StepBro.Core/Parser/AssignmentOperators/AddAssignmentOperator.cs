using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.AssignmentOperators
{
    internal class AddAssignmentOperator : AssignmentOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            switch (first.ReferencedType)
            {
                case SBExpressionType.GlobalVariableReference:
                    break;
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.Indexing:
                case SBExpressionType.PropertyReference:
                    return new SBExpressionData(Expression.AddAssignChecked(first.ExpressionCode, last.ExpressionCode));
                case SBExpressionType.DatatableReference:
                    break;
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
