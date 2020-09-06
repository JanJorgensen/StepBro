using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

using StepBro.Core.Data;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class MultiplyOperator : BinaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            if (first.IsConstant && last.IsConstant)
            {
                #region Const
                object f = first.Value;
                object s = last.Value;

                if (f is long && s is long)
                {
                    return new SBExpressionData((long)f * (long)s);
                }
                else if (f is string && s is long)
                {
                    var fs = (string)f;
                    var n = (long)s;
                    var result = StepBro.Core.Execution.ExecutionHelperMethods.StringMultiply(fs, n);
                    return new SBExpressionData(result);
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData((double)f * (double)s);
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData((double)f * (long)s);
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData((long)f * (double)s);
                }
                else if (f is TimeSpan && s is long)
                {
                    return new SBExpressionData(((TimeSpan)f).Multiply((long)s));
                }
                else if (f is long && s is TimeSpan)
                {
                    return new SBExpressionData(((TimeSpan)s).Multiply((long)f));
                }
                else if (f is TimeSpan && s is double)
                {
                    return new SBExpressionData(((TimeSpan)f).Multiply((double)s));
                }
                else if (f is double && s is TimeSpan)
                {
                    return new SBExpressionData(((TimeSpan)s).Multiply((double)f));
                }
                else
                {
                    throw new NotImplementedException();
                }
                #endregion
            }
            else
            {
                #region Variable
                if (first.IsValueType && last.IsValueType)
                {
                    if (first.IsInt && last.IsInt)
                    {
                        return new SBExpressionData(Expression.Multiply(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsString && last.IsInt)
                    {
                        return new SBExpressionData(
                            Expression.Call(
                                typeof(StepBro.Core.Execution.ExecutionHelperMethods).GetMethod("StringMultiply", new Type[] { typeof(string), typeof(long) }),
                                first.ExpressionCode,
                                last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Multiply(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Multiply(first.ConvertToDouble().ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(Expression.Multiply(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsInt)
                    {
                        var method = typeof(StepBro.Core.Data.TimeUtils).GetMethod("Multiply", new Type[] { typeof(TimeSpan), typeof(long) });
                        return new SBExpressionData(Expression.Call(
                            method,
                            first.ExpressionCode,
                            last.ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsDecimal)
                    {
                        var method = typeof(StepBro.Core.Data.TimeUtils).GetMethod("Multiply", new Type[] { typeof(TimeSpan), typeof(double) });
                        return new SBExpressionData(Expression.Call(
                            method,
                            first.ExpressionCode,
                            last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsTimespan)
                    {
                        var method = typeof(StepBro.Core.Data.TimeUtils).GetMethod("Multiply", new Type[] { typeof(TimeSpan), typeof(long) });
                        return new SBExpressionData(Expression.Call(
                            method,
                            last.ExpressionCode,
                            first.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsTimespan)
                    {
                        var method = typeof(StepBro.Core.Data.TimeUtils).GetMethod("Multiply", new Type[] { typeof(TimeSpan), typeof(double) });
                        return new SBExpressionData(Expression.Call(
                            method,
                            last.ExpressionCode,
                            first.ExpressionCode));
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
                #endregion
            }

        }
    }
}
