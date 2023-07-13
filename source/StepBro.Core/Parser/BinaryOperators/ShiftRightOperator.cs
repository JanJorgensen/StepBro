using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser.BinaryOperators
{
    internal class ShiftRightOperator : BinaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            if (first.IsConstant && last.IsConstant)
            {
                #region const
                object f = first.Value;
                object s = last.Value;

                if (f is long && s is int)
                {
                    return new SBExpressionData((long)f >> (int)s);
                }
                else if (f is int && s is int)
                {
                    return new SBExpressionData((int)f >> (int)s);
                }
                else if (f is long && s is long)
                {
                    return new SBExpressionData((long)f >> Convert.ToInt32((long)s)); // Force int on right side because second value needs to be int
                }
                else if (f is int && s is long)
                {
                    return new SBExpressionData((int)f >> Convert.ToInt32((long)s)); // Force int on right side because second value needs to be int
                }
                else
                {
                    throw new NotImplementedException("Only integers are allowed when using the right shift operator.");
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
                        return new SBExpressionData(Expression.RightShift(first.ExpressionCode, last.ExpressionCode));
                    }
                    else
                    {
                        throw new NotImplementedException("Only integers are allowed when using the right shift operator.");
                    }
                }
                else
                {
                    throw new NotImplementedException("Only integers are allowed when using the right shift operator.");
                }
                #endregion
            }
        }
    }
}
