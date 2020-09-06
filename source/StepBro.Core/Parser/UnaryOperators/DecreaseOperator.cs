using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal class DecreaseOperator : UnaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData input, bool opOnLeft)
        {
            switch (input.ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                    break;
                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.GlobalVariableReference:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.Indexing:
                case SBExpressionType.PropertyReference:
                    if (input.IsValueType)
                    {
                        if (input.IsInt)
                        {
                            if (opOnLeft)
                            {
                                return new SBExpressionData(Expression.PreDecrementAssign(input.ExpressionCode));
                            }
                            else
                            {
                                return new SBExpressionData(Expression.PostDecrementAssign(input.ExpressionCode));
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    break;
                case SBExpressionType.TypeReference:
                    break;
                case SBExpressionType.MethodReference:
                    break;
                case SBExpressionType.ProcedureReference:
                    break;
                case SBExpressionType.DatatableReference:
                    break;
                default:
                    break;
            }

            throw new NotImplementedException();
        }
    }
}
