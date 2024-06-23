using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;
using StepBro.Core.Execution;
using System.Reflection;
using StepBro.Core.ScriptData;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal class IncreaseOperator : UnaryOperatorBase
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
                    if (input.IsValueType)
                    {
                        // Since ValueContainers are always made with exactly 1 generic type argument, and that
                        // generic type argument always is the type of the underlying value, we can use that here.
                        var unaryOperatorGlobalVariableTyped = s_UnaryOperatorGlobalVariable.MakeGenericMethod(input.DataType.Type.GenericTypeArguments[0]);
                        IValueContainer valueContainer = (input.Value as FileVariable).VariableOwnerAccess.Container;
                        return new SBExpressionData(Expression.Call(
                                        unaryOperatorGlobalVariableTyped,
                                        listener.m_currentProcedure?.ContextReferenceInternal,
                                        Expression.Constant(valueContainer.UniqueID),
                                        Expression.Constant(SBP.OP_INC),
                                        Expression.Constant(opOnLeft)));
                    }
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
                                return new SBExpressionData(Expression.PreIncrementAssign(input.ExpressionCode));
                            }
                            else
                            {
                                return new SBExpressionData(Expression.PostIncrementAssign(input.ExpressionCode));
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
