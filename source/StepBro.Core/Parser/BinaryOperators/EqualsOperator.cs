using System;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class EqualsOperator : BinaryOperatorBase
    {
        public bool InvertResult { get; private set; }

        public EqualsOperator(bool invert)
        {
            this.InvertResult = invert;
        }
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            if (first.IsConstant && last.IsConstant)
            {
                #region Const
                object f = first.Value;
                object s = last.Value;

                if (f is long && s is long)
                {
                    return new SBExpressionData(((long)f == (long)s) ^ InvertResult);
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData(((double)f == (double)s) ^ InvertResult);
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData(((double)f == (long)s) ^ InvertResult);
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData(((long)f == (double)s) ^ InvertResult);
                }
                else if (f is TimeSpan && s is TimeSpan)
                {
                    return new SBExpressionData(((TimeSpan)f == (TimeSpan)s) ^ InvertResult);
                }
                else if (f is DateTime && s is DateTime)
                {
                    return new SBExpressionData(((DateTime)f == (DateTime)s) ^ InvertResult);
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
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ExpressionCode, last.ExpressionCode)));
                    }
                    else if (first.IsDecimal && last.IsDecimal)
                    {
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ExpressionCode, last.ExpressionCode)));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ConvertToDouble().ExpressionCode, last.ExpressionCode)));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ExpressionCode, last.ConvertToDouble().ExpressionCode)));
                    }
                    else if (first.IsTimespan && last.IsTimespan)
                    {
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ExpressionCode, last.ConvertToDouble().ExpressionCode)));
                    }
                    else if (first.IsDateTime && last.IsDateTime)
                    {
                        return new SBExpressionData(InvertIfNeeded(Expression.Equal(first.ExpressionCode, last.ConvertToDouble().ExpressionCode)));
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

        private Expression InvertIfNeeded(Expression e)
        {
            if (InvertResult) return Expression.Not(e);
            else return e;
            // TODO: Maybe replace Expression in Resolve() with Expression.NotEqual when inverse.
        }
    }
}
