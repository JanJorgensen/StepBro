using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.Execution;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser.SpecialOperators
{
    internal class EqualsWithToleranceOperator
    {
        public static SBExpressionData Resolve(StepBroListener listener, SBExpressionData value, int op, SBExpressionData expected, SBExpressionData tolerance)
        {
            bool isApprox = (op == SBP.OP_EQ_APPROX);

            if (value.IsValueType && expected.IsValueType && tolerance.IsValueType)
            {
                var v = value.ExpressionCode;
                var e = expected.ExpressionCode;
                var t = tolerance.ExpressionCode;

                if (value.IsInt)
                {
                    if (!expected.IsInt || !tolerance.IsInt)
                    {
                        throw new NotSupportedException("Expected value and tolerance must be integer values too.");
                    }
                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.EqualsWithToleranceIntegerExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            v,
                            e,
                            t));

                }
                else if (value.IsDecimal)
                {
                    if (expected.IsInt)
                    {
                        e = Expression.Convert(e, typeof(double));
                    }
                    else if (!expected.IsDecimal)
                    {
                        throw new ArgumentException("Error");
                    }
                    if (tolerance.IsInt)
                    {
                        t = Expression.Convert(t, typeof(double));
                    }
                    else if (!tolerance.IsDecimal)
                    {
                        throw new ArgumentException("Error");
                    }

                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.EqualsWithToleranceDecimalExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            Expression.Constant(isApprox),
                            v,
                            e,
                            t));
                }
                else if (value.IsTimespan)
                {
                    if (expected.IsDecimal)
                    {
                        e = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            e);
                    }
                    else if (expected.IsInt)
                    {
                        e = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            Expression.Convert(e, typeof(double)));
                    }
                    else if (!expected.IsTimespan)
                    {
                        throw new ArgumentException("Error");
                    }
                    if (tolerance.IsDecimal)
                    {
                        t = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            t);
                    }
                    else if (tolerance.IsInt)
                    {
                        t = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            Expression.Convert(t, typeof(double)));
                    }
                    else if (!tolerance.IsTimespan)
                    {
                        throw new ArgumentException("Error");
                    }

                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.EqualsWithToleranceTimespanExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            Expression.Constant(isApprox),
                            v,
                            e,
                            t));
                }
                else if (value.IsDateTime)
                {
                    if (!expected.IsDateTime)
                    {
                        throw new ArgumentException("Error");
                    }
                    if (tolerance.IsDecimal)
                    {
                        t = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            t);
                    }
                    else if (tolerance.IsInt)
                    {
                        t = Expression.Call(
                            typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                            Expression.Convert(t, typeof(double)));
                    }
                    else if (!tolerance.IsTimespan)
                    {
                        throw new ArgumentException("Error");
                    }

                    var helper = typeof(ExecutionHelperMethods).GetMethod(
                        nameof(ExecutionHelperMethods.EqualsWithToleranceDateTimeExpression));

                    return new SBExpressionData(
                        Expression.Call(
                            helper,
                            Expression.Convert(Expression.Constant(null), typeof(IScriptCallContext)),
                            Expression.Constant(isApprox),
                            v,
                            e,
                            t));
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
            if (op == SBP.OP_LE) return NumericLimitType.Include;
            else if (op == SBP.OP_LT_APPROX) return NumericLimitType.Approx;
            else if (op == SBP.LT) return NumericLimitType.Exclude;
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
