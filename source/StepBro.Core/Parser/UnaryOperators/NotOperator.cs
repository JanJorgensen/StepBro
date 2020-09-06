using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.UnaryOperators
{
    internal class NotOperator : UnaryOperatorBase
    {
        public override SBExpressionData Resolve(StepBroListener listener, SBExpressionData input, bool opOnLeft)
        {
            if (input.IsConstant)
            {
                object i = input.Value;

                if (i is bool)
                {
                    return new SBExpressionData(!(bool)i);
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
                    if (input.IsBool)
                    {
                        return new SBExpressionData(Expression.Not(input.ExpressionCode));
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
