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

namespace StepBro.Core.Parser.AssignmentOperators
{
    internal class AssignmentOperator : AssignmentOperatorBase
    {
        private static MethodInfo s_DynamicObjectSetProperty = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicObjectSetProperty));

        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            switch (first.ReferencedType)
            {
                case SBExpressionType.GlobalVariableReference:
                    break;
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
