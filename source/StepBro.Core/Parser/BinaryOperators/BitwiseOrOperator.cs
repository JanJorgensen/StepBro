using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class BitwiseOrOperator : BinaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            if (first.IsConstant && last.IsConstant)
            {
                #region const
                object f = first.Value;
                object s = last.Value;

                if (f is long && s is long)
                {
                    return new SBExpressionData((long)f | (long)s);
                }
                else
                {
                    throw new NotImplementedException("Only integers are allowed when using the bitwise OR operator.");
                }
                #endregion
            }
            else
            {
                #region variable
                if (first.IsValueType && last.IsValueType)
                {
                    if (first.IsInt && last.IsInt)
                    {
                        return new SBExpressionData(Expression.Or(first.ExpressionCode, last.ExpressionCode));
                    }
                    else
                    {
                        throw new NotImplementedException("Only integers are allowed when using the bitwise OR operator.");
                    }
                }
                else
                {
                    throw new NotImplementedException("Only integers are allowed when using the bitwise OR operator.");
                }
                #endregion
            }
        }
    }
}
