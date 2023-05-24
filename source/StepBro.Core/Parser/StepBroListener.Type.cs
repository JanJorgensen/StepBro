using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private Stack<TypeReference> m_typeStack = new Stack<TypeReference>();
        protected Stack<Tuple<TypeReference, List<TypeReference>>> m_typedefTypeStack = null;     // In Tuple: Generic type + list of generic parameters.

        public TypeReference LastParsedType { get { return m_typeStack.Peek(); } }

        /// <summary>
        /// Type used for indicating that a variable is specified with 'var' instead of an explicit type.
        /// </summary>
        internal sealed class VarSpecifiedType { private VarSpecifiedType() { } }

        /// <summary>
        /// Type used for indicating that a variable is specified with 'dynamic' instead of an explicit type.
        /// </summary>
        internal sealed class DynamicSpecifiedType { private DynamicSpecifiedType() { } }

        public override void ExitTypeVoid([NotNull] SBP.TypeVoidContext context)
        {
            m_typeStack.Push((TypeReference)typeof(void));
        }

        public override void ExitTypeProcedure([NotNull] SBP.TypeProcedureContext context)
        {
            m_typeStack.Push(TypeReference.TypeProcedure);
        }

        public override void ExitTypeFunction([NotNull] SBP.TypeFunctionContext context)
        {
            m_typeStack.Push(TypeReference.TypeFunction);
        }

        public override void ExitTypeTestList([NotNull] SBP.TypeTestListContext context)
        {
            m_typeStack.Push(TypeReference.TypeTestList);
        }

        public override void ExitTypePrimitive([NotNull] SBP.TypePrimitiveContext context)
        {
            Type type = null;
            var arrRank = (context.ChildCount > 1) ? context.children[1].GetText().Where(c => c == '[').Count() : 0;
            if (arrRank == 0)
            {
                switch (context.Start.Type)
                {
                    case SBP.BOOL: type = typeof(bool); break;
                    case SBP.INT_: type = typeof(long); break;
                    case SBP.INTEGER: type = typeof(long); break;
                    case SBP.DECIMAL: type = typeof(double); break;
                    case SBP.DOUBLE: type = typeof(double); break;
                    case SBP.VERDICT: type = typeof(Verdict); break;
                    case SBP.DATETIME: type = typeof(DateTime); break;
                    case SBP.TIMESPAN: type = typeof(TimeSpan); break;
                    case SBP.STRING: type = typeof(string); break;
                    case SBP.OBJECT: type = typeof(object); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (arrRank == 1)
            {
                switch (context.Start.Type)
                {
                    case SBP.BOOL: type = typeof(List<bool>); break;
                    case SBP.INT_: type = typeof(List<long>); break;
                    case SBP.INTEGER: type = typeof(List<long>); break;
                    case SBP.DECIMAL: type = typeof(List<double>); break;
                    case SBP.DOUBLE: type = typeof(List<double>); break;
                    case SBP.VERDICT: type = typeof(List<Verdict>); break;
                    case SBP.DATETIME: type = typeof(List<DateTime>); break;
                    case SBP.TIMESPAN: type = typeof(List<TimeSpan>); break;
                    case SBP.STRING: type = typeof(List<string>); break;
                    case SBP.OBJECT: type = typeof(List<object>); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (arrRank == 2)
            {
                switch (context.Start.Type)
                {
                    case SBP.BOOL: type = typeof(List<List<bool>>); break;
                    case SBP.INT_: type = typeof(List<List<long>>); break;
                    case SBP.INTEGER: type = typeof(List<List<long>>); break;
                    case SBP.DECIMAL: type = typeof(List<List<double>>); break;
                    case SBP.DOUBLE: type = typeof(List<List<double>>); break;
                    case SBP.VERDICT: type = typeof(List<List<Verdict>>); break;
                    case SBP.DATETIME: type = typeof(List<List<DateTime>>); break;
                    case SBP.TIMESPAN: type = typeof(List<List<TimeSpan>>); break;
                    case SBP.STRING: type = typeof(List<List<string>>); break;
                    case SBP.OBJECT: type = typeof(List<List<object>>); break;
                    default:
                        throw new NotImplementedException();
                }
            }
            m_typeStack.Push((TypeReference)type);
        }

        public override void ExitTypeClassOrInterface([NotNull] SBP.TypeClassOrInterfaceContext context)
        {
            var exp = m_expressionData.Pop();
            var type = this.ParseTypeString(exp.Value as string, false, true, context.Start);
            m_typeStack.Push(type);     // Push, even if null.
        }

        public override void ExitVariableVarType([NotNull] SBP.VariableVarTypeContext context)
        {
            m_typeStack.Push((TypeReference)typeof(VarSpecifiedType));
        }

        public override void ExitVariableDynamicType([NotNull] SBP.VariableDynamicTypeContext context)
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

        public override void ExitTypeReference([NotNull] SBP.TypeReferenceContext context)
        {
            var typename = context.GetChild(2).GetText();
            var type = ParseTypeString(typename, false, true, (context.GetChild(2).Payload as Antlr4.Runtime.CommonToken));
            m_expressionData.Push(SBExpressionData.Constant(TypeReference.TypeType, type));
        }

        public override void EnterTypeGeneric([NotNull] SBP.TypeGenericContext context)
        {
            m_typedefTypeStack = new Stack<Tuple<TypeReference, List<TypeReference>>>();
            m_typedefTypeStack.Push(new Tuple<TypeReference, List<TypeReference>>(null, new List<TypeReference>()));
        }

        public override void ExitTypeGeneric([NotNull] SBP.TypeGenericContext context)
        {
            var type = m_typedefTypeStack.Peek().Item1;
            var parameters = m_typedefTypeStack.Peek().Item2;
        }

        public override void EnterTypeParameter([NotNull] SBP.TypeParameterContext context)
        {
            m_expressionData.PushStackLevel("TypeParameter");
        }

        public override void ExitTypeParameter([NotNull] SBP.TypeParameterContext context)
        {
            m_expressionData.PopStackLevel();
            var type = m_typeStack.Pop();

            m_typedefTypeStack.Peek().Item2.Add(type);

            //var type = PopType("typeparameter");
            //var par = m_typedefStack.Pop();
            //par.SetTypeName(type.Item1, type.Item2);
            //m_typedefStack.Peek().AddParameter(par);
        }

    }
}