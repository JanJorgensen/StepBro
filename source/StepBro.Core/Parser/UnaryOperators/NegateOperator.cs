using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal class NegateOperator : UnaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData input, bool opOnLeft)
        {
            if (input.IsConstant)
            {
                object i = input.Value;

                if (i is long)
                {
                    return new SBExpressionData(-(long)i);
                }
                else if (i is double)
                {
                    return new SBExpressionData(-(double)i);
                }
                else if (i is TimeSpan)
                {
                    return new SBExpressionData(((TimeSpan)i).Negate());
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (input.IsValueType)
                {
                    if (input.IsInt)
                    {
                        return new SBExpressionData(Expression.Negate(input.ExpressionCode));
                    }
                    else if (input.IsDecimal)
                    {
                        return new SBExpressionData(Expression.Negate(input.ExpressionCode));
                    }
                    else if (input.IsTimespan)
                    {
                        return new SBExpressionData(Expression.Call(input.ExpressionCode, "Negate", new Type[] { }));
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
        }
    }
}
