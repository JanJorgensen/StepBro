using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class PlusOperator : BinaryOperatorBase
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
                    return new SBExpressionData((long)f + (long)s);
                }
                else if (f is string && s is string)
                {
                    return new SBExpressionData((string)f + (string)s);
                }
                else if (f is string && s is long)
                {
                    return new SBExpressionData((string)f + ((long)s).ToString());
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData((double)f + (double)s);
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData((double)f + (long)s);
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData((long)f + (double)s);
                }
                else if (f is TimeSpan && s is TimeSpan)
                {
                    return new SBExpressionData((TimeSpan)f + (TimeSpan)s);
                }
                else if (f is TimeSpan && s is double)
                {
                    return new SBExpressionData((TimeSpan)f + TimeSpan.FromSeconds((double)s));
                }
                else if (f is DateTime && s is TimeSpan)
                {
                    return new SBExpressionData((DateTime)f + (TimeSpan)s);
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
                        return new SBExpressionData(Expression.Add(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsString && last.IsString)
                    {
                        return new SBExpressionData(
                            Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }),
                                first.ExpressionCode,
                                last.ExpressionCode));
                    }
                    else if (first.IsString)
                    {
                        Expression lastAsText;
                        if (last.IsString)
                        {
                            lastAsText = last.ExpressionCode;
                        }
                        else if (last.DataType.Type != typeof(object) && typeof(IEnumerable).IsAssignableFrom(last.DataType.Type))
                        {
                            var m = typeof(StringUtils).GetMethod(nameof(StringUtils.ListToString), new Type[] { typeof(IEnumerable) });
                            lastAsText = Expression.Call(m, Expression.Convert(last.ExpressionCode, typeof(IEnumerable)));
                        }
                        else
                        {
                            lastAsText = Expression.Call(
                                typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ObjectToString), new Type[] { typeof(object) }),
                                Expression.Convert(last.ExpressionCode, typeof(object)));
                        }
                        return new SBExpressionData(
                            Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) }),
                                first.ExpressionCode,
                                lastAsText));
                    }
                    else if (first.IsDecimal && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Add(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Add(first.ConvertToDouble().ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(Expression.Add(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsTimespan)
                    {
                        return new SBExpressionData(Expression.Call(
                            first.ExpressionCode,
                            typeof(System.TimeSpan).GetMethod("Add", new Type[] { typeof(TimeSpan) }),
                            last.ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Call(
                            first.ExpressionCode,
                            typeof(System.TimeSpan).GetMethod("Add", new Type[] { typeof(TimeSpan) }),
                            Expression.Call(
                                typeof(System.TimeSpan).GetMethod("FromSeconds", new Type[] { typeof(double) }),
                                last.ExpressionCode)));
                    }
                    else if (first.IsDateTime && last.IsTimespan)
                    {
                        return new SBExpressionData(Expression.Call(
                            first.ExpressionCode,
                            typeof(System.DateTime).GetMethod("Add", new Type[] { typeof(TimeSpan) }),
                            last.ExpressionCode));
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
