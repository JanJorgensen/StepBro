using System;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class EqualsOperator : BinaryOperatorBase
    {
        private bool m_invert;

        public EqualsOperator(bool invert)
        {
            m_invert = invert;
        }
        private BinaryExpression EqualityExpression(Expression left, Expression right)
        {
            if (m_invert) return Expression.Equal(left, right);
            else return Expression.Equal(left, right);
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
                    return new SBExpressionData(((long)f == (long)s) ^ m_invert);
                }
                else if (f is double && s is double)
                {
                    return new SBExpressionData(((double)f == (double)s) ^ m_invert);
                }
                else if (f is double && s is long)
                {
                    return new SBExpressionData(((double)f == (long)s) ^ m_invert);
                }
                else if (f is long && s is double)
                {
                    return new SBExpressionData(((long)f == (double)s) ^ m_invert);
                }
                else if (f is TimeSpan && s is TimeSpan)
                {
                    return new SBExpressionData(((TimeSpan)f == (TimeSpan)s) ^ m_invert);
                }
                else if (f is DateTime && s is DateTime)
                {
                    return new SBExpressionData(((DateTime)f == (DateTime)s) ^ m_invert);
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
                    if (first.DataType.Equals(last.DataType))
                    {
                        return new SBExpressionData(this.EqualityExpression(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsInt && last.IsDecimal)
                    {
                        return new SBExpressionData(this.EqualityExpression(first.ConvertToDouble().ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.IsDecimal && last.IsInt)
                    {
                        return new SBExpressionData(this.EqualityExpression(first.ExpressionCode, last.ConvertToDouble().ExpressionCode));
                    }
                    else if (first.IsObject && last.IsObject)
                    {
                        return new SBExpressionData(this.EqualityExpression(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if ((first.IsString && last.IsObject && last.IsConstant && last.Value == null && last.DataType.Type == typeof(object)) ||
                        (last.IsString && first.IsObject && first.IsConstant && first.Value == null && first.DataType.Type == typeof(object)))
                    {
                        return new SBExpressionData(this.EqualityExpression(first.ExpressionCode, last.ExpressionCode));
                    }
                    else
                    {
                        if (first.DataType.Equals(last.DataType)) throw new NotImplementedException();
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
