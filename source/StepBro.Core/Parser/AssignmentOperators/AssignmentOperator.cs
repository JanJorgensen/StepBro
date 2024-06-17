using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;
using StepBro.Core.Execution;
using System.Reflection;
using StepBro.Core.Api;
using System.ComponentModel;
using StepBro.Core.ScriptData;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.Core.Parser.AssignmentOperators
{
    internal class AssignmentOperator : AssignmentOperatorBase
    {
        private static MethodInfo s_DynamicObjectSetProperty = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicObjectSetProperty));
        private static MethodInfo s_SetGlobalVariable = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.SetGlobalVariable));

        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            switch (first.ReferencedType)
            {
                case SBExpressionType.GlobalVariableReference:
                    // Since ValueContainers are always made with exactly 1 generic type argument, and that
                    // generic type argument always is the type of the underlying value, we can use that here.
                    var setGlobalVariableTyped = s_SetGlobalVariable.MakeGenericMethod(first.DataType.Type.GenericTypeArguments[0]);
                    IValueContainer valueContainer = (first.Value as FileVariable).VariableOwnerAccess.Container; // first;
                    return new SBExpressionData(Expression.Call(
                                    setGlobalVariableTyped,
                                    listener.m_currentProcedure?.ContextReferenceInternal,
                                    Expression.Constant(valueContainer.UniqueID),
                                    last.ExpressionCode));
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.Indexing:
                case SBExpressionType.PropertyReference:
                    return new SBExpressionData(Expression.Assign(first.ExpressionCode, last.ExpressionCode));
                case SBExpressionType.DatatableReference:
                    break;
                case SBExpressionType.DynamicObjectMember:
                    {
                        var call = Expression.Call(
                            s_DynamicObjectSetProperty,
                            listener.m_currentProcedure?.ContextReferenceInternal,
                            Expression.Convert(first.InstanceCode, typeof(IDynamicStepBroObject)),
                            Expression.Constant((string)first.Value),
                            Expression.Convert(last.ExpressionCode, typeof(object)));

                        return new SBExpressionData(call);
                    }
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
    }
}
