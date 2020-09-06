using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.Execution;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser.SpecialOperators
{
    internal class BetweenOperator
    {
        public static SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, int op1, SBExpressionData middle, int op2, SBExpressionData last)
        {
            if (first.IsValueType && middle.IsValueType && last.IsValueType)
            {
                var f = first.ExpressionCode;
                var l = last.ExpressionCode;
                var m = middle.ExpressionCode;

                if (first.IsInt && middle.IsInt && last.IsInt)
                {
                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.IsBetweenIntegerExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            first.ExpressionCode,
                            Expression.Constant(OpToLimitType(op1)),
                            middle.ExpressionCode,
                            Expression.Constant(OpToLimitType(op2)),
                            last.ExpressionCode));

                }
                else if (middle.IsDecimal || middle.IsInt)
                {
                    if (first.IsInt)
                    {
                        f = Expression.Convert(f, typeof(double));
                    }
                    else if (!first.IsDecimal)
                    {
                        throw new NotImplementedException("Error");
                    }
                    if (last.IsInt)
                    {
                        l = Expression.Convert(l, typeof(double));
                    }
                    else if (!last.IsDecimal)
                    {
                        throw new NotImplementedException("Error");
                    }

                    if (middle.IsInt)
                    {
                        m = Expression.Convert(m, typeof(double));
                    }

                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.IsBetweenDecimalExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            f,
                            Expression.Constant(OpToLimitType(op1)),
                            m,
                            Expression.Constant(OpToLimitType(op2)),
                            l));
                }
                else if (middle.IsTimespan)
                {
                    if (first.IsDecimal)
                    {
                        f = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            f);
                    }
                    else if (first.IsInt)
                    {
                        f = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            Expression.Convert(f, typeof(double)));
                    }
                    else if (!first.IsTimespan)
                    {
                        throw new NotImplementedException("Error");
                    }
                    if (last.IsDecimal)
                    {
                        l = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            l);
                    }
                    else if (last.IsInt)
                    {
                        l = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            Expression.Convert(l, typeof(double)));
                    }
                    else if (!last.IsTimespan)
                    {
                        throw new NotImplementedException("Error");
                    }

                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.IsBetweenTimespanExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            f,
                            Expression.Constant(OpToLimitType(op1)),
                            m,
                            Expression.Constant(OpToLimitType(op2)),
                            l));
                }
                else if (first.IsDateTime && middle.IsDateTime && last.IsDateTime)
                {
                    //return new TSExpressionData(Expression.LessThan(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static NumericLimitType OpToLimitType(int op)
        {
            if (op == TSP.OP_LE) return NumericLimitType.Include;
            else if (op == TSP.OP_LT_APPROX) return NumericLimitType.Approx;
            else if (op == TSP.LT) return NumericLimitType.Exclude;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
