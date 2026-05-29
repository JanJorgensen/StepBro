using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        public override void EnterExpBracket([NotNull] SBP.ExpBracketContext context)
        {
            m_expressionData.PushStackLevel("ExpressionBracket");
        }

        public override void ExitExpBracket([NotNull] SBP.ExpBracketContext context)
        {
            var expressions = m_expressionData.PopStackLevel();
            var indexingExpressions = new List<SBExpressionData>();
            indexingExpressions.Add(expressions.Stack.Pop());
            var source = this.ResolveIfIdentifier(expressions.Stack.Pop(), true);
            var sourceType = source.DataType;

            if (sourceType.Type.IsGenericType && sourceType.Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                m_expressionData.Push(this.CreateListIndexerExpression(source, this.ResolveForGetOperation(indexingExpressions[0], reportIfUnresolved: true)));
            }
            else if (sourceType.Type.IsArray)
            {
                m_expressionData.Push(this.CreateArrayIndexerExpression(source, this.ResolveForGetOperation(indexingExpressions[0], reportIfUnresolved: true)));
            }
            else if (sourceType.Type == typeof(string))
            {
                m_expressionData.Push(this.CreateStringIndexerExpression(source, this.ResolveForGetOperation(indexingExpressions[0], reportIfUnresolved: true)));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public SBExpressionData CreateListIndexerExpression(SBExpressionData source, SBExpressionData indexer)
        {
            var indexerCode = indexer.ExpressionCode;

            // http://stackoverflow.com/questions/794198/how-do-i-check-if-a-given-value-is-a-generic-list

            if (source.DataType.Type.IsGenericType && source.DataType.Type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (indexer.DataType.Equals(typeof(long)))
                {
                    indexerCode = Expression.Convert(indexerCode, typeof(int));
                }
                else if (!indexer.DataType.Equals(typeof(int)))
                {
                    throw new NotImplementedException("An indexer type not supported yet.");
                }
                var propInfo = source.DataType.Type.GetProperty("Item", source.DataType.Type.GenericTypeArguments[0], new Type[] { typeof(int) });
                var propExpr = Expression.Property(source.ExpressionCode, propInfo, indexerCode);
                return new SBExpressionData(propExpr, SBExpressionType.Indexing);

                // http://stackoverflow.com/questions/6759416/accessing-indexer-from-expression-tree
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public SBExpressionData CreateStringIndexerExpression(SBExpressionData source, SBExpressionData indexer)
        {
            var indexerCode = indexer.ExpressionCode;

            if (indexer.DataType.Equals(typeof(long)))
            {
                indexerCode = Expression.Convert(indexerCode, typeof(int));
            }
            else if (!indexer.DataType.Equals(typeof(int)))
            {
                throw new NotImplementedException("An indexer type not supported yet.");
            }

            var substringExpression = Expression.Call(source.ExpressionCode, typeof(string).GetMethod(nameof(String.Substring), new Type[] { typeof(Int32), typeof(Int32)}), indexerCode, Expression.Constant(1));
            return new SBExpressionData(substringExpression, SBExpressionType.Expression);
        }

        public SBExpressionData CreateArrayIndexerExpression(SBExpressionData source, SBExpressionData indexer)
        {
            var indexerCode = indexer.ExpressionCode;

            if (indexer.DataType.Equals(typeof(long)))
            {
                indexerCode = Expression.Convert(indexerCode, typeof(int));
            }
            else if (!indexer.DataType.Equals(typeof(int)))
            {
                throw new NotImplementedException("An indexer type not supported yet.");
            }

            var arrayIndexingExpression = Expression.ArrayIndex(source.ExpressionCode, indexerCode);
            return new SBExpressionData(arrayIndexingExpression, SBExpressionType.Expression);
        }

        public override void EnterExpArray([NotNull] SBP.ExpArrayContext context)
        {
            m_expressionData.PushStackLevel("ExpressionArray");
        }

        public override void ExitExpArray([NotNull] SBP.ExpArrayContext context)
        {
            var valueParserExpressions = m_expressionData.PopStackLevel().Stack.ToList();
            for (int i = 0; i < valueParserExpressions.Count; i++)
            {
                valueParserExpressions[i] = this.ResolveForGetOperation(valueParserExpressions[i]);
            }

            bool first = true, same = true;
            Type firstElementType = null;
            foreach (var t in valueParserExpressions.Select(exp => exp.DataType.Type))
            {
                if (first) { firstElementType = t; first = false; }
                else
                {
                    if (t != firstElementType)
                    {
                        same = false;
                        break;
                    }
                }
            }
            if (same)
            {
                var arrayValueExpressions = Expression.NewArrayInit(firstElementType, valueParserExpressions.Select(exp => exp.ExpressionCode).Reverse().ToArray());

                if (firstElementType.IsArray || firstElementType.IsGenericType)
                {
                    if (firstElementType.IsGenericType)
                    {
                        if (firstElementType.IsArray)
                        {
                            throw new NotImplementedException("Element type is array.");
                        }
                        else
                        {
                            throw new NotImplementedException("");
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("");
                    }
                }
                else
                {
                    var typeGeneric = typeof(List<>);
                    Type[] typeArgs = { firstElementType };
                    var listType = typeGeneric.MakeGenericType(typeArgs);
                    var ctor = listType.GetConstructor(new Type[] { firstElementType.MakeArrayType() });
                    var listCtor = Expression.New(ctor, arrayValueExpressions);
                    m_expressionData.Push(new SBExpressionData(listCtor));

                    // http://stackoverflow.com/questions/1151464/how-to-dynamically-create-generic-c-sharp-object-using-reflection
                }
            }
            else
            {
                throw new NotImplementedException("Elements in array are not the same type.");
            }
        }


        // int[][] myArray [ [10,11,12,13], [20,21,22,23] ]     =>  myArray[1,3] = 23
    }
}
