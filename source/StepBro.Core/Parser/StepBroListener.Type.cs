using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private Stack<TypeReference> m_typeStack = new Stack<TypeReference>();
        public TypeReference LastParsedType { get { return m_typeStack.Peek(); } }

        /// <summary>
        /// Type used for indicating that a variable is specified with 'var' instead of an explicit type.
        /// </summary>
        internal sealed class VarSpecifiedType { private VarSpecifiedType() { } }

        /// <summary>
        /// Type used for indicating that a variable is specified with 'dynamic' instead of an explicit type.
        /// </summary>
        internal sealed class DynamicSpecifiedType { private DynamicSpecifiedType() { } }

        public override void ExitTypeVoid([NotNull] TSP.TypeVoidContext context)
        {
            m_typeStack.Push((TypeReference)typeof(void));
        }

        public override void ExitTypeProcedure([NotNull] TSP.TypeProcedureContext context)
        {
            m_typeStack.Push(TypeReference.TypeProcedure);
        }

        public override void ExitTypeFunction([NotNull] TSP.TypeFunctionContext context)
        {
            m_typeStack.Push(TypeReference.TypeFunction);
        }

        public override void ExitTypeTestList([NotNull] TSP.TypeTestListContext context)
        {
            m_typeStack.Push(TypeReference.TypeTestList);
        }

        public override void ExitTypePrimitive([NotNull] TSP.TypePrimitiveContext context)
        {
            Type type = null;
            var arrRank = (context.ChildCount > 1) ? context.children[1].GetText().Where(c => c == '[').Count() : 0;
            if (arrRank == 0)
            {
                switch (context.Start.Type)
                {
                    case TSP.BOOL: type = typeof(bool); break;
                    case TSP.INT_: type = typeof(long); break;
                    case TSP.INTEGER: type = typeof(long); break;
                    case TSP.DECIMAL: type = typeof(double); break;
                    case TSP.DOUBLE: type = typeof(double); break;
                    case TSP.VERDICT: type = typeof(Verdict); break;
                    case TSP.DATETIME: type = typeof(DateTime); break;
                    case TSP.TIMESPAN: type = typeof(TimeSpan); break;
                    case TSP.STRING: type = typeof(string); break;
                    case TSP.OBJECT: type = typeof(object); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (arrRank == 1)
            {
                switch (context.Start.Type)
                {
                    case TSP.BOOL: type = typeof(List<bool>); break;
                    case TSP.INT_: type = typeof(List<long>); break;
                    case TSP.INTEGER: type = typeof(List<long>); break;
                    case TSP.DECIMAL: type = typeof(List<double>); break;
                    case TSP.DOUBLE: type = typeof(List<double>); break;
                    case TSP.VERDICT: type = typeof(List<Verdict>); break;
                    case TSP.DATETIME: type = typeof(List<DateTime>); break;
                    case TSP.TIMESPAN: type = typeof(List<TimeSpan>); break;
                    case TSP.STRING: type = typeof(List<string>); break;
                    case TSP.OBJECT: type = typeof(List<object>); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (arrRank == 2)
            {
                switch (context.Start.Type)
                {
                    case TSP.BOOL: type = typeof(List<List<bool>>); break;
                    case TSP.INT_: type = typeof(List<List<long>>); break;
                    case TSP.INTEGER: type = typeof(List<List<long>>); break;
                    case TSP.DECIMAL: type = typeof(List<List<double>>); break;
                    case TSP.DOUBLE: type = typeof(List<List<double>>); break;
                    case TSP.VERDICT: type = typeof(List<List<Verdict>>); break;
                    case TSP.DATETIME: type = typeof(List<List<DateTime>>); break;
                    case TSP.TIMESPAN: type = typeof(List<List<TimeSpan>>); break;
                    case TSP.STRING: type = typeof(List<List<string>>); break;
                    case TSP.OBJECT: type = typeof(List<List<object>>); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            m_typeStack.Push((TypeReference)type);
        }

        public override void ExitTypeClassOrInterface([NotNull] TSP.TypeClassOrInterfaceContext context)
        {
            var exp = m_expressionData.Pop();
            var type = this.ParseTypeString(exp.Value as string, false, true, context.Start);
            m_typeStack.Push(type);     // Push, even if null.
        }

        public override void ExitVariableVarType([NotNull] TSP.VariableVarTypeContext context)
        {
            m_typeStack.Push((TypeReference)typeof(VarSpecifiedType));
        }

        public override void ExitVariableDynamicType([NotNull] TSP.VariableDynamicTypeContext context)
        {
            m_typeStack.Push((TypeReference)typeof(DynamicSpecifiedType));
        }

        public TypeReference ParseTypeString(
            string text, 
            bool skipPrimitives = false, 
            bool reportErrors = false, 
            Antlr4.Runtime.IToken token = null)
        {
            int iBrackets = text.IndexOf('[');
            var identifier = (iBrackets >= 0) ? text.Substring(0, iBrackets) : text;
            if (!skipPrimitives)
            {
                switch (identifier)
                {
                    case "void": return TypeReference.TypeVoid;
                    case "int": return TypeReference.TypeInt64;
                    case "integer": return TypeReference.TypeInt64;
                    case "string": return TypeReference.TypeString;
                    case "decimal": return TypeReference.TypeDouble;
                    case "double": return TypeReference.TypeDouble;
                    case "bool": return TypeReference.TypeBool;
                    case "verdict": return TypeReference.TypeVerdict;
                    case "timespan": return TypeReference.TypeTimeSpan;
                    case "datetime": return TypeReference.TypeDateTime;
                    case "object": return TypeReference.TypeObject;
                    case "procedure": return TypeReference.TypeProcedure;
                    case "function": return TypeReference.TypeFunction;
                    default:
                        break;
                }
            }

            return this.ResolveQualifiedType(identifier, reportErrors, token)?.DataType;
        }

        public override void ExitTypeReference([NotNull] TSP.TypeReferenceContext context)
        {
            var typename = context.GetChild(2).GetText();
            var type = ParseTypeString(typename, false, true, (context.GetChild(2).Payload as Antlr4.Runtime.CommonToken));
            m_expressionData.Push(SBExpressionData.Constant(TypeReference.TypeType, type));
        }
    }
}