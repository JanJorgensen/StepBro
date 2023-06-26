using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Range = StepBro.Core.Data.Range;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private static MethodInfo s_AwaitAsyncVoid = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncVoid));
        private static MethodInfo s_AwaitAsyncTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncTyped));
        private static MethodInfo s_AwaitAsyncToTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncToTyped));
        private static MethodInfo s_AwaitObjectToTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitObjectToTyped));
        private static MethodInfo s_ProcedureReferenceIs = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ProcedureReferenceIs));
        private static MethodInfo s_ProcedureReferenceAs = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ProcedureReferenceAs));
        private static MethodInfo s_ObjectIsType = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ObjectIsType));

        private ExpressionStack m_expressionData = new ExpressionStack();

        public void PrepareForExpressionParsing(string name)
        {
            m_expressionData.Clear();
            m_expressionData.PushStackLevel(name);
        }

        public SBExpressionData GetExpressionResult()
        {
            return this.ResolveIfIdentifier(this.GetExpressionResultUnresolved(), true);
        }

        public SBExpressionData GetExpressionResultUnresolved()
        {
            var topStack = m_expressionData.PopStackLevel();
            return topStack.Pop();
        }

        //public Expression PopStackLevelAsExpression()
        //{
        //    var exp = this.TryPopStackLevelAsExpression();
        //    //if (exp == null) throw new NullReferenceException();
        //    return exp;
        //}

        //public Expression TryPopStackLevelAsExpression()
        //{
        //    Expression value = null;
        //    var stack = m_expressionData.PopStackLevel();
        //    if (stack.Count == 0) return null;
        //    var exp = stack.Pop();
        //    exp = this.ResolveIfIdentifier(exp, m_inFunctionScope);
        //    value = exp.ExpressionCode;
        //    //if (value == null)
        //    //{
        //    //    switch (exp.ReferencedType)
        //    //    {
        //    //        case SBExpressionType.Constant:
        //    //            value = Expression.Constant(exp.Value);
        //    //            break;
        //    //        //case TSExpressionType.GlobalVariableReference:
        //    //        //    break;
        //    //        //case TSExpressionType.LocalVariableReference:
        //    //        //    break;
        //    //        //case TSExpressionType.PropertyReference:
        //    //        //    break;
        //    //        //case TSExpressionType.MethodReference:
        //    //        //    break;
        //    //        case SBExpressionType.Identifier:
        //    //            value = this.ResolveQualifiedIdentifier((string)exp.Value, true)?.ExpressionCode;
        //    //            break;
        //    //        default:
        //    //            throw new NotImplementedException();
        //    //    }
        //    //}
        //    return value;
        //}

        public override void EnterParExpression([NotNull] SBP.ParExpressionContext context)
        {
            m_expressionData.PushStackLevel("EnterParExpression");
        }

        public override void ExitParExpression([NotNull] SBP.ParExpressionContext context)
        {
            var expressionScope = m_expressionData.PopStackLevel();
            System.Diagnostics.Debug.Assert(expressionScope.Count == 1);    // Until anything else has been seen...
            var data = expressionScope.Pop();
            m_expressionData.Push(data);
        }

        public override void ExitPrimary([NotNull] SBP.PrimaryContext context)
        {
            if (context.Start.Type == Grammar.StepBro.IDENTIFIER)
            {
                m_expressionData.Push(SBExpressionData.CreateIdentifier(context.GetText(), token: context.Start));
            }
            else if (context.Start.Type == Grammar.StepBro.THIS)
            {
                var thisProperty = Expression.Property(
                    m_currentProcedure.ContextReferenceInternal,
                    typeof(IScriptCallContext).GetProperty(nameof(IScriptCallContext.This)));
                m_expressionData.Push(new SBExpressionData(
                    HomeType.Immediate,
                    SBExpressionType.PropertyReference,
                    new TypeReference(typeof(IProcedureThis)),
                    thisProperty,
                    token: context.Start));
            }
        }

        public override void EnterExpPrimary([NotNull] SBP.ExpPrimaryContext context)
        {
        }

        public override void ExitExpPrimary([NotNull] SBP.ExpPrimaryContext context)
        {
        }

        public override void EnterExpCast([NotNull] SBP.ExpCastContext context)
        {
            //base.EnterExpCast(context);
        }

        public override void ExitExpCast([NotNull] SBP.ExpCastContext context)
        {
            var type = m_typeStack.Pop();
            var value = this.ResolveForGetOperation(m_expressionData.Pop());
            m_expressionData.Push(new SBExpressionData(Expression.Convert(value.ExpressionCode, type.Type)));
        }

        public override void ExitExpAwait([NotNull] SBP.ExpAwaitContext context)
        {
            try
            {
                var exp = this.ResolveForGetOperation(m_expressionData.Pop());
                m_expressionData.Push(SBExpressionData.CreateAwaitExpression(exp.ExpressionCode, context.Start));
            }
            catch
            {
                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.OperationError, "Error parsing 'await' operation.", null, context.Start));
            }

            //var awaiter = this.MakeAwaitOperation(exp.ExpressionCode, context, true);
            //if (awaiter != null)
            //{
            //    m_expressionData.Push(
            //        new SBExpressionData(
            //            HomeType.Immediate,
            //            SBExpressionType.Expression,
            //            new TypeReference(awaiter.Type),
            //            awaiter));
            //}
            //else
            //{
            //    m_expressionData.Push(exp);
            //}
        }

        public override void ExitExpCastAs([NotNull] SBP.ExpCastAsContext context)
        {
            var exp = this.ResolveForGetOperation(m_expressionData.Pop());
            var type = m_typeStack.Pop();
            if (type.HasProcedureReference)
            {
                var caster = s_ProcedureReferenceAs.MakeGenericMethod(type.Type);

                var call = Expression.Call(
                    caster,
                    m_currentProcedure?.ContextReferenceInternal,
                    Expression.Convert(exp.ExpressionCode, typeof(IProcedureReference)),
                    Expression.Constant((type.DynamicType as IFileProcedure).ParentFile.UniqueID),
                    Expression.Constant((type.DynamicType as IFileProcedure).UniqueID));
                m_expressionData.Push(
                    new SBExpressionData(
                        HomeType.Immediate,
                        SBExpressionType.Expression,
                        type,
                        call));
            }
            else if (type.DynamicType != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                var call = Expression.Call(
                    s_ObjectIsType,
                    Expression.Constant(type.Type, typeof(Type)),
                    Expression.Convert(exp.ExpressionCode, typeof(object)),
                    Expression.Constant(context.ChildCount == 4));
                m_expressionData.Push(
                    new SBExpressionData(
                        HomeType.Immediate,
                        SBExpressionType.Expression,
                        TypeReference.TypeBool,
                        call));
            }
        }

        public override void ExitExpIsType([NotNull] SBP.ExpIsTypeContext context)
        {
            var exp = this.ResolveForGetOperation(m_expressionData.Pop());
            var type = m_typeStack.Pop();
            if (type.HasProcedureReference)
            {
                var call = Expression.Call(
                    s_ProcedureReferenceIs,
                    this.GetContextAccess(),
                    Expression.Convert(exp.ExpressionCode, typeof(IProcedureReference)),
                    Expression.Constant((type.DynamicType as IFileProcedure).ParentFile.UniqueID),
                    Expression.Constant((type.DynamicType as IFileProcedure).UniqueID),
                    Expression.Constant(context.ChildCount == 4));
                m_expressionData.Push(
                    new SBExpressionData(
                        HomeType.Immediate,
                        SBExpressionType.Expression,
                        TypeReference.TypeBool,
                        call));
            }
            else if (type.DynamicType != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                var call = Expression.Call(
                    s_ObjectIsType,
                    Expression.Constant(type.Type, typeof(Type)),
                    Expression.Convert(exp.ExpressionCode, typeof(object)),
                    Expression.Constant(context.ChildCount == 4));
                m_expressionData.Push(
                    new SBExpressionData(
                        HomeType.Immediate,
                        SBExpressionType.Expression,
                        TypeReference.TypeBool,
                        call));
            }
        }

        internal Expression MakeAwaitOperation(Expression input, Antlr4.Runtime.ParserRuleContext context, bool expectAwaitable, Type assignmentType)
        {
            var t = input.Type;
            if (t == typeof(System.IAsyncResult))
            {
                if (assignmentType != null) throw new ArgumentException("It can't be an assignment when expression is this type.");
                return Expression.Call(s_AwaitAsyncVoid, m_currentProcedure.ContextReferenceInternal, input);
            }
            else if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(StepBro.Core.Tasks.IAsyncResult<>))
            {
                var T = t.GenericTypeArguments[0];
                if (T == typeof(object) && assignmentType != null && assignmentType != typeof(object))
                {
                    var awaiter = s_AwaitAsyncToTyped.MakeGenericMethod(assignmentType);
                    return Expression.Call(awaiter, m_currentProcedure.ContextReferenceInternal, input);
                }
                else
                {
                    var awaiter = s_AwaitAsyncTyped.MakeGenericMethod(T);
                    return Expression.Call(awaiter, m_currentProcedure.ContextReferenceInternal, input);
                }
            }
            else if (t == typeof(object))
            {
                if (assignmentType != null && assignmentType != typeof(object))
                {
                    var awaiter = s_AwaitObjectToTyped.MakeGenericMethod(assignmentType);
                    return Expression.Call(awaiter, m_currentProcedure.ContextReferenceInternal, input);
                }
                else
                {
                    var awaiter = s_AwaitObjectToTyped.MakeGenericMethod(typeof(object));
                    return Expression.Call(awaiter, m_currentProcedure.ContextReferenceInternal, input);
                }
            }
            else
            {
                if (expectAwaitable)
                {
                    m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Expression is not an asynchronuos type that can be used with 'await'.");
                    return null;
                }
                else
                {
                    return input;
                }
            }
        }

        private SBExpressionData ResolveForGetOperation(SBExpressionData input, TypeReference targetType = null, bool reportIfUnresolved = false)
        {
            var output = input;
            if (input.IsUnresolvedIdentifier)
            {
                var resolved = this.ResolveIdentifierForGetOperation((string)input.Value, m_inFunctionScope, targetType);
                if (resolved != null)
                {
                    output = resolved;
                    output.Token = input.Token;
                    output.ParameterName = input.ParameterName;
                    output.Argument = input.Argument;
                    if (reportIfUnresolved && resolved.IsUnknownIdentifier)
                    {
                        m_errors.UnresolvedIdentifier(output.Token, (string)output.Value);
                    }
                }
            }
            return output;
        }

        private SBExpressionData ResolveIdentifierForGetOperation(string input, bool inFunctionScope, TypeReference targetType = null)
        {
            SBExpressionData result = this.ResolveQualifiedIdentifier(input, inFunctionScope);
            if (result.IsError())
            {
                return result;
            }
            if (result != null)
            {
                Expression expression = result.ExpressionCode;
                var isContainer = typeof(IValueContainer).IsAssignableFrom(expression.Type);
                if (isContainer && (targetType == null || !targetType.Type.IsAssignableFrom(typeof(IValueContainer))))
                {
                    var datatype = (TypeReference)expression.Type.GenericTypeArguments[0];
                    if (result.Value != null && result.Value is FileVariable)
                    {
                        datatype = ((FileVariable)result.Value).DataType;    // Get the declared type of the variable.
                    }
                    var getValue = Expression.Call(
                        expression,
                        expression.Type.GetMethod("GetTypedValue"),
                        Expression.Constant(null, typeof(StepBro.Core.Logging.ILogger)));
                    result = new SBExpressionData(
                        HomeType.Immediate,
                        SBExpressionType.Expression,
                        datatype,
                        getValue);//,
                        //result.Value /* E.g. the variable reference */ );
                }
                return result;
            }
            else
            {
                throw new NotImplementedException("Must be handled somewhere else!!");
            }
        }

        public override void ExitExpBinary([NotNull] SBP.ExpBinaryContext context)
        {
            var ctxt = context.GetText();
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            if (first.IsResolved && last.IsResolved)
            {
                var op = BinaryOperators.BinaryOperatorBase.GetOperator(context.op.Type);
                // TODO: Check if operator is returned
                var result = op.Resolve(this, first, last);
                m_expressionData.Push(result);
            }
            else
            {
                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.OperationError, 
                    "Error parsing binary operation.", 
                    context.GetText(), 
                    new TokenOrSection(context.Start, context.Stop, context.GetText())));
            }
        }

        public override void ExitExpUnaryRight([NotNull] SBP.ExpUnaryRightContext context)
        {
            this.ExitExpUnary(context.op.Type, false);
        }

        public override void ExitExpUnaryLeft([NotNull] SBP.ExpUnaryLeftContext context)
        {
            this.ExitExpUnary(context.op.Type, true);
        }

        private void ExitExpUnary(int type, bool opOnLeft)
        {
            var input = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            if (input.IsError())
            {
                m_expressionData.Push(input);
            }
            else
            {
                var op = UnaryOperators.UnaryOperatorBase.GetOperator(type);
                var result = op.Resolve(this, input, opOnLeft);
                m_expressionData.Push(result);
            }
        }

        public override void ExitExpAssignment([NotNull] SBP.ExpAssignmentContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Pop(), reportIfUnresolved: true);
            var first = this.ResolveIfIdentifier(m_expressionData.Peek().Pop(), true);
            if (last.IsError())
            {
                m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError));
            }
            else
            {
                if (first.IsError())
                {
                    m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError));
                }
                else
                {
                    last.NarrowGetValueType();
                    var op = AssignmentOperators.AssignmentOperatorBase.GetOperator(context.op.Type);
                    if (op != null)
                    {
                        // TODO: Check if operator is returned
                        last = this.CastProcedureAssignmentArgumentIfNeeded(first.DataType, last);

                        if (first.IsError()) m_expressionData.Push(first);
                        else if (last.IsError()) m_expressionData.Push(last);
                        else
                        {
                            var result = op.Resolve(this, first, last);
                            m_expressionData.Push(result);
                        }
                    }
                }
            }
        }

        public override void ExitExpBetween([NotNull] SBP.ExpBetweenContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var middle = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var op1 = context.op1.Type;
            var op2 = context.op2.Type;
            // TODO: Check if operator is returned
            var result = SpecialOperators.BetweenOperator.Resolve(this, first, op1, middle, op2, last);
            m_expressionData.Push(result);
        }

        public override void ExitExpEqualsWithTolerance([NotNull] SBP.ExpEqualsWithToleranceContext context)
        {
            var tolerance = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var expected = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var value = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var op = context.op.Type;
            // TODO: Check if operator is returned
            var result = SpecialOperators.EqualsWithToleranceOperator.Resolve(this, value, op, expected, tolerance);
            m_expressionData.Push(result);
        }

        public override void ExitExpCoalescing([NotNull] SBP.ExpCoalescingContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Pop()).NarrowGetValueType();
            var result = SpecialOperators.CoalescingOperator.Resolve(this, first, last);
            m_expressionData.Push(result);
        }

        #region Literals

        public override void ExitLiteralInteger([NotNull] SBP.LiteralIntegerContext context)
        {
            var str = context.GetText();
            char last = str[str.Length - 1];
            if (Char.IsLetter(last))
            {
                int dotIndex = str.IndexOf('.');
                long value;
                long valueFromDecimals = 0;
                if (dotIndex > 0)
                {
                    value = Int64.Parse(str.Substring(0, dotIndex));
                    string decimals = str.Substring(dotIndex + 1, str.Length - (dotIndex + 2));
                    valueFromDecimals = Int64.Parse(decimals);
                    switch (decimals.Length)
                    {
                        case 1:
                            valueFromDecimals *= 100L;
                            break;
                        case 2:
                            valueFromDecimals *= 10L;
                            break;
                        default:
                            break;
                    }
                    value = value * 1000L + valueFromDecimals;
                }
                else
                {
                    value = Int64.Parse(str.Substring(0, str.Length - 1)) * 1000L;
                }

                switch (last)
                {
                    case 'K': break;
                    case 'M': value *= 1000L; break;
                    case 'G': value *= 1000000L; break;
                    case 'T': value *= 1000000000L; break;
                    case 'P': value *= 1000000000000L; break;
                    default:
                        throw new NotImplementedException("Postfix not implemented: " + last.ToString());
                }
                m_expressionData.Push(new SBExpressionData(value));
            }
            else
            {
                m_expressionData.Push(new SBExpressionData(Int64.Parse(context.GetText()), context.Start));
            }
        }

        public override void ExitLiteralHex([NotNull] SBP.LiteralHexContext context)
        {
            var str = context.GetText();
            long value = 0L;
            if (str.StartsWith("0x"))
            {
                value = Int64.Parse(str.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                value = Int64.Parse(str.Substring(0, str.Length - 1), System.Globalization.NumberStyles.HexNumber);
            }
            m_expressionData.Push(new SBExpressionData(value, context.Start));
        }

        public override void ExitLiteralFloat([NotNull] SBP.LiteralFloatContext context)
        {
            var str = context.GetText();
            var strVal = str;
            char last = str[str.Length - 1];
            var factor = 1.0;
            if (Char.IsLetter(last))
            {
                switch (last)
                {
                    case 'P': factor = 1000000000000000.0; break;
                    case 'T': factor = 1000000000000.0; break;
                    case 'G': factor = 1000000000.0; break;
                    case 'M': factor = 1000000.0; break;
                    case 'k': factor = 1000.0; break;
                    case 'm': factor = 0.001; break;
                    case 'u': factor = 0.000001; break;
                    case 'n': factor = 0.000000001; break;
                    case 'p': factor = 0.000000000001; break;
                    default:
                        break;
                }
                strVal = str.Substring(0, str.Length - 1);
            }
            double v = Double.Parse(strVal, System.Globalization.CultureInfo.InvariantCulture);

            m_expressionData.Push(new SBExpressionData(v * factor, context.Start));
        }

        public override void ExitLiteralBool([NotNull] SBP.LiteralBoolContext context)
        {
            m_expressionData.Push(new SBExpressionData(context.GetText() == "true", context.Start));
        }

        public override void ExitLiteralString([NotNull] SBP.LiteralStringContext context)
        {
            m_expressionData.Push(SBExpressionData.Constant(TypeReference.TypeString, ParseStringLiteral(context.GetText(), context), context.Start));
        }

        internal static string ParseStringLiteral(string text, Antlr4.Runtime.ParserRuleContext context)
        {
            var str = text;
            if (str[0] == '@')
            {
                int i = str.IndexOf('\"') + 1;
                str = str.Substring(i, str.Length - (i + 1));
                str = str.Replace("\"\"", "\"");
                return str;
            }
            else
            {
                try
                {
                    str = str.Substring(1, str.Length - 2);
                    return str.DecodeLiteral();
                }
                catch (FormatException)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override void ExitLiteralIdentifier([NotNull] SBP.LiteralIdentifierContext context)
        {
            var str = context.GetText();
            str = str.Substring(1, str.Length - 2);
            m_expressionData.Push(new SBExpressionData(new Identifier(str), context.Start));
        }

        public override void ExitVerdictLiteral([NotNull] SBP.VerdictLiteralContext context)
        {
            var str = context.GetText();
            Data.Verdict verdict = (Data.Verdict)Enum.Parse(typeof(Data.Verdict), str, true);
            m_expressionData.Push(new SBExpressionData(verdict));
        }

        public override void ExitLiteralTimespan([NotNull] SBP.LiteralTimespanContext context)
        {
            var str = context.GetText();
            var ts = TimeUtils.ParseTimeSpan(str);
            m_expressionData.Push(new SBExpressionData(ts, context.Start));
        }

        public override void ExitLiteralDateTime([NotNull] SBP.LiteralDateTimeContext context)
        {
            var str = context.GetText();
            var ts = TimeUtils.ParseDateTime(str, 1);   // 1, because @ should be skipped.
            m_expressionData.Push(new SBExpressionData(ts, context.Start));
        }

        public override void ExitLiteralNull([NotNull] SBP.LiteralNullContext context)
        {
            m_expressionData.Push(SBExpressionData.Constant((TypeReference)typeof(object), null, context.Start));
        }

        public override void ExitLiteralRange([NotNull] SBP.LiteralRangeContext context)
        {
            var str = context.GetText();
            string error = null;
            var start = str.IndexOf('[', 1) + 1;
            var range = Range.Create(str.Substring(start, str.Length - (start + 1)), ref error);
            if (String.IsNullOrEmpty(error))
            {
                m_expressionData.Push(new SBExpressionData(range, context.Start));
            }
            else
            {
                throw new NotImplementedException();    // TODO
            }
        }

        public override void ExitLiteralBinaryBlock([NotNull] SBP.LiteralBinaryBlockContext context)
        {
            var str = context.GetText();
            var l = str.Length - 1;
            var bytes = new byte[l / 2];
            var b = 0;
            var index = 1;
            while (index < l)
            {
                byte value = Byte.Parse(str.Substring(index, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[b++] = value;
                index += 2;
            }
            m_expressionData.Push(new SBExpressionData(bytes, context.Start));
        }

        #endregion

        #region Helper Methods

        public SBExpressionData CastProcedureAssignmentArgumentIfNeeded(TypeReference type, SBExpressionData expression)
        {
            var t = type.Type;
            if (!expression.IsError() &&
                expression.DataType.Type != t &&
                expression.DataType.Type == typeof(IProcedureReference) &&
                t.IsConstructedGenericType &&
                t.GenericTypeArguments.Length == 1 &&
                t.GetGenericTypeDefinition() == typeof(IProcedureReference<>))
            {
                var getCastMethodTyped = s_CastProcedureReference.MakeGenericMethod(t.GenericTypeArguments[0]);

                var casted = Expression.Call(
                    getCastMethodTyped,
                    m_currentProcedure.ContextReferenceInternal,
                    expression.ExpressionCode);

                return new SBExpressionData(casted);
            }
            else
            {
                return expression;
            }
        }

        #endregion
    }
}
