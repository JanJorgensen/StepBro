﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class LessThanOrApproxOperator : BinaryOperatorBase
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
                    return new SBExpressionData((long)f <= (long)s);
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData(ExecutionHelperMethods.LessThanOrApprox((double)f, (double)s));
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData(ExecutionHelperMethods.LessThanOrApprox((double)f, (long)s));
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData(ExecutionHelperMethods.LessThanOrApprox((long)f, (double)s));
                }
                else if (f is TimeSpan && s is TimeSpan)
                {
                    return new SBExpressionData((TimeSpan)f < (TimeSpan)s);
                }
                else if (f is DateTime && s is DateTime)
                {
                    return new SBExpressionData((DateTime)f < (DateTime)s);
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
                        return new SBExpressionData(Expression.LessThanOrEqual(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsDecimal)
                    {
                        return new SBExpressionData(
                            Expression.Call(
                                typeof(ExecutionHelperMethods).GetMethod("LessThanApprox", new Type[] { typeof(double), typeof(double) }),
                                first.ExpressionCode,
                                last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(
                            Expression.Call(
                                typeof(ExecutionHelperMethods).GetMethod("LessThanApprox", new Type[] { typeof(long), typeof(double) }),
                                first.ExpressionCode,
                                last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(
                            Expression.Call(
                                typeof(ExecutionHelperMethods).GetMethod("LessThanApprox", new Type[] { typeof(double), typeof(long) }),
                                first.ExpressionCode,
                                last.ExpressionCode));
                    }
                    else if (first.IsTimespan && last.IsTimespan)
                    {
                        return new SBExpressionData(Expression.LessThan(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    }
                    else if (first.IsDateTime && last.IsDateTime)
                    {
                        return new SBExpressionData(Expression.LessThan(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
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
