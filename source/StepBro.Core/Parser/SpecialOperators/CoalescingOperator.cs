using System;
using System.Linq.Expressions;

namespace StepBro.Core.Parser.SpecialOperators
{
    internal class CoalescingOperator
    {
        public static SBExpressionData Resolve(StepBroListener listener, SBExpressionData first, SBExpressionData last)
        {
            if (first.IsConstant && last.IsConstant)
            {
                #region Const
                return new SBExpressionData(first.Value ?? last.Value);
                #endregion
            }
            else
            {
                #region Variable
                if (first.IsNullable)
                {
                    if (last.IsValueType)
                    {
                        var nullableTypeArg = Nullable.GetUnderlyingType(first.DataType.Type);
                        if (last.DataType.Equals(nullableTypeArg) || last.DataType.Type.IsSubclassOf(nullableTypeArg))
                        {
                            return new SBExpressionData(Expression.Coalesce(first.ExpressionCode, last.ExpressionCode));
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
                else if (!first.DataType.Type.IsValueType)
                {
                    if (first.DataType.Type == last.DataType.Type)
                    {
                        return new SBExpressionData(Expression.Coalesce(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (first.DataType.Type.IsSubclassOf(last.DataType.Type))
                    {
                        return new SBExpressionData(Expression.Coalesce(first.ExpressionCode, last.ExpressionCode));
                    }
                    else if (last.DataType.Type.IsSubclassOf(first.DataType.Type))
                    {
                        return new SBExpressionData(Expression.Coalesce(first.ExpressionCode, last.ExpressionCode));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                #endregion
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
