using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class ModulusOperator : BinaryOperatorBase
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
                    return new SBExpressionData((long)f % (long)s);
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData((double)f % (double)s);
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData((double)(long)f % (double)s);
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData((double)f % (double)(long)s);
                }
                else if (f is TimeSpan && s is long)
                {
                    return new SBExpressionData(TimeSpan.FromTicks(((TimeSpan)f).Ticks % (long)s));
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
                        return new SBExpressionData(Expression.Modulo(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Modulo(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Modulo(first.ConvertToDouble().ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(Expression.Modulo(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsInt)
                    {
                        return new SBExpressionData(
                            Expression.Call(
                                null,
                                typeof(TimeSpan).GetMethod("FromTicks", new Type[] { typeof(long) }),
                                Expression.Modulo(
                                    Expression.Property(first.ExpressionCode, "Ticks"),
                                    last.ExpressionCode)));
                    }
                    //else if (first.IsTimespan && last.IsDecimal)
                    //{
                    //    return new TSExpressionData(
                    //        Expression.Call(
                    //            null,
                    //            typeof(TimeSpan).GetMethod("FromTicks", new Type[] { typeof(long) }),
                    //            Expression.Call(
                    //                typeof(System.Convert).GetMethod("ToInt64", new Type[] { typeof(double) }),
                    //                Expression.Modulo(
                    //                    Expression.Property(first.ExpressionCode, "Ticks"),
                    //                    last.ExpressionCode))));
                    //}
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
