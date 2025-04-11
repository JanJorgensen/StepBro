using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Range = StepBro.Core.Data.Range;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private static MethodInfo s_SaveExpectValueText = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.SaveExpectValueText));
        private static MethodInfo s_AwaitAsyncVoid = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncVoid));
        private static MethodInfo s_AwaitAsyncTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncTyped));
        private static MethodInfo s_AwaitAsyncToTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitAsyncToTyped));
        private static MethodInfo s_AwaitObjectToTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.AwaitObjectToTyped));
        private static MethodInfo s_ProcedureReferenceIs = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ProcedureReferenceIs));
        private static MethodInfo s_ProcedureReferenceAs = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ProcedureReferenceAs));
        private static MethodInfo s_ObjectIsType = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ObjectIsType));
        private static MethodInfo s_DynamicObjectGetProperty = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicObjectGetProperty));
        private static MethodInfo s_DynamicObjectSetProperty = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicObjectSetProperty));


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
            var topLevel = m_expressionData.PopStackLevel();
            return topLevel.Stack.Pop();
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
            System.Diagnostics.Debug.Assert(expressionScope.Stack.Count == 1, "Parameter error in: " + context.GetText());    // Until anything else has been seen...
            var data = expressionScope.Stack.Pop();
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
                m_errors.InternalError(context.Start.Line, context.Start.Column, "Error parsing 'await' operation.");
                //m_expressionData.Push(new SBExpressionData(
                //    SBExpressionType.OperationError, "Error parsing 'await' operation.", null, context.Start));
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
                        SBExpressionType.Expression,
                        TypeReference.TypeBool,
                        call));
            }
        }

        public override void ExitExpSelect([NotNull] SBP.ExpSelectContext context)
        {
            var val2 = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            var val1 = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            var exp = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), targetType: TypeReference.TypeBool, reportIfUnresolved: true).NarrowGetValueType();
            if (CheckExpressionsForErrors(context, exp, val1, val2))
            {
                // TODO: Add some try-catch around this to catch exceptions and report errors.

                m_expressionData.Push(
                    new SBExpressionData(Expression.Condition(
                        exp.ExpressionCode,
                        val1.ExpressionCode,
                        val2.ExpressionCode)));
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
            if (input.IsError()) return input;
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

            if (!output.IsError() && !output.IsUnresolvedIdentifier)
            {
                if (output.IsDynamicObjectMember)
                {
                    var getter = s_DynamicObjectGetProperty.MakeGenericMethod((targetType != null) ? targetType.Type : typeof(Object));

                    var call = Expression.Call(
                        getter,
                        m_currentProcedure?.ContextReferenceInternal,
                        Expression.Convert(output.InstanceCode, typeof(IDynamicStepBroObject)),
                        Expression.Constant((string)output.Value));

                    output = new SBExpressionData(call);
                }
                else if (output.DataType.Type.IsContainer())
                {
                    Expression expression = output.ExpressionCode;
                    var datatype = (TypeReference)output.DataType.Type.GenericTypeArguments[0];
                    if (output.Value != null && output.Value is FileVariable)
                    {
                        datatype = ((FileVariable)output.Value).DataType;    // Get the declared type of the variable.
                    }
                    var getValue = Expression.Call(
                        expression,
                        expression.Type.GetMethod("GetTypedValue"),
                        Expression.Constant(null, typeof(StepBro.Core.Logging.ILogger)));
                    output = new SBExpressionData(
                        SBExpressionType.Expression,
                        datatype,
                        getValue);//,
                                  //result.Value /* E.g. the variable reference */ );
                }
                else if (output.DataType.IsInt() && targetType != null && targetType.Equals(TypeReference.TypeDouble))
                {
                    // Do automatic conversion from int to double.
                    output = new SBExpressionData(Expression.Convert(output.ExpressionCode, typeof(double)));
                }
                else if (output.IsOverrideElement)
                {
                    var overrider = (FileElementOverride)(output.Value);
                    var rootElement = IdentifierToExpressionData(overrider.GetRootBaseElement(), output.Token);
                    if (overrider.HasTypeOverride)
                    {
                        rootElement.Value = output.Value;
                    }

                    var getValue = Expression.Call(
                        rootElement.ExpressionCode,        // Code for getting the variable reference
                        rootElement.DataType.Type.GetMethod("GetTypedValue"),
                        Expression.Constant(null, typeof(StepBro.Core.Logging.ILogger)));
                    output = new SBExpressionData(
                        SBExpressionType.Expression,
                        overrider.DataType,
                        getValue,
                        output.Value);
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
            if (result == null)
            {
                throw new NotImplementedException("Must be handled somewhere else!!");
            }
            return ResolveForGetOperation(result, targetType);
        }

        public override void ExitExpBinary([NotNull] SBP.ExpBinaryContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            if (CheckExpressionsForErrors(context, first, last))
            {
                try
                {
                    var op = BinaryOperators.BinaryOperatorBase.GetOperator(context.op.Type);

                    try
                    {
                        if (m_isSimpleExpectWithValue)
                        {
                            if (first.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                            {
                                var valueSaverFirst = s_SaveExpectValueText.MakeGenericMethod(first.DataType.Type);
                                first = new SBExpressionData(Expression.Call(valueSaverFirst, m_currentProcedure.ContextReferenceInternal, first.ExpressionCode, Expression.Constant("Left"), Expression.Constant(true)));
                            }

                            if (last.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                            {
                                var valueSaverLast = s_SaveExpectValueText.MakeGenericMethod(last.DataType.Type);
                                last = new SBExpressionData(Expression.Call(valueSaverLast, m_currentProcedure.ContextReferenceInternal, last.ExpressionCode, Expression.Constant("Right"), Expression.Constant(false)));
                            }
                        }
                        var result = op.Resolve(this, first, last);
                        System.Diagnostics.Debug.Assert(result != null);
                        m_expressionData.Push(result);
                    }
                    catch (NotImplementedException)
                    {
                        var description = $"Operation '{context.GetChild(1).GetText()}' not implemented for the specified types.";
                        m_errors.SymanticError(context.op.Line, context.op.Column, false, description);
                        m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError, description, context.GetText(), new TokenOrSection(context.Start, context.Stop, context.GetText())));
                    }
                    catch (ParsingErrorException ex)
                    {
                        var description = (String.IsNullOrEmpty(ex.Message)) ? $"Operation '{context.GetChild(1).GetText()}' not supported for the specified types." : ex.Message;
                        m_errors.SymanticError(context.op.Line, context.op.Column, false, description);
                        m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError, description, context.GetText(), new TokenOrSection(context.Start, context.Stop, context.GetText())));
                    }
                    catch (Exception)
                    {
                        var description = $"Unhandled exception in operation '{context.GetChild(1).GetText()}'.";
                        m_errors.InternalError(context.op.Line, context.op.Column, "");
                        m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError, description, context.GetText(), new TokenOrSection(context.Start, context.Stop, context.GetText())));
                    }
                }
                catch (NotImplementedException)
                {
                    var description = $"Operation '{context.GetChild(1).GetText()}' is not implemented.";
                    m_errors.InternalError(context.op.Line, context.op.Column, description);
                }
                catch (Exception)
                {
                    var description = $"Unhandled exception in operation '{context.GetChild(1).GetText()}'.";
                    m_errors.InternalError(context.op.Line, context.op.Column, description);
                }
            }
        }

        public override void ExitExpUnaryRight([NotNull] SBP.ExpUnaryRightContext context)
        {
            this.ExitExpUnary(context, context.op.Type, false);
        }

        public override void ExitExpUnaryLeft([NotNull] SBP.ExpUnaryLeftContext context)
        {
            this.ExitExpUnary(context, context.op.Type, true);
        }

        private void ExitExpUnary(ParserRuleContext context, int type, bool opOnLeft)
        {
            var input = this.ResolveIfIdentifier(m_expressionData.Peek().Stack.Pop(), true);
            if (CheckExpressionsForErrors(context, input))
            {
                var op = UnaryOperators.UnaryOperatorBase.GetOperator(type);
                var result = op.Resolve(this, input, opOnLeft);
                m_expressionData.Push(result);
            }
        }

        public override void ExitExpAssignment([NotNull] SBP.ExpAssignmentContext context)
        {
            var last = m_expressionData.Peek().Stack.Pop();
            var first = this.ResolveIfIdentifier(m_expressionData.Peek().Stack.Pop(), true);
            last = this.ResolveForGetOperation(last, reportIfUnresolved: true, targetType: first.DataType);
            if (CheckExpressionsForErrors(context, first, last))
            {
                last.NarrowGetValueType();
                var op = AssignmentOperators.AssignmentOperatorBase.GetOperator(context.op.Type);
                if (op != null)
                {
                    // TODO: Check if operator is returned
                    last = this.CastProcedureAssignmentArgumentIfNeeded(first.DataType, last);
                    if (CheckExpressionsForErrors(context, first, last))
                    {
                        try
                        {
                            var result = op.Resolve(this, first, last);
                            m_expressionData.Push(result);
                        }
                        catch (ArgumentException ae)
                        {
                            // ArgumentExceptions are have shown to be semantic errors, not internal ones.
                            // If we find an ArgumentException that is an internal error, update this.
                            m_errors.SymanticError(context.Start.Line, context.Start.Column, false, $"Assignment failed: {ae.Message}");
                            // Adding an error expression to the expression data stack makes it possible
                            // to parse the rest of the script to catch further issues
                            m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError));
                        }
                        catch (Exception e)
                        {
                            m_errors.InternalError(context.Start.Line, context.Start.Column, $"Assignment failed: {e.Message}");
                            // Adding an error expression to the expression data stack makes it possible
                            // to parse the rest of the script to catch further issues
                            m_expressionData.Push(new SBExpressionData(SBExpressionType.ExpressionError));
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override void ExitExpBetween([NotNull] SBP.ExpBetweenContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop()).NarrowGetValueType();
            var middle = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop()).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop()).NarrowGetValueType();
            if (CheckExpressionsForErrors(context, first, middle, last))
            {
                if (m_isSimpleExpectWithValue)
                {
                    if (first.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                    {
                        var valueSaverFirst = s_SaveExpectValueText.MakeGenericMethod(first.DataType.Type);
                        first = new SBExpressionData(Expression.Call(valueSaverFirst, m_currentProcedure.ContextReferenceInternal, first.ExpressionCode, Expression.Constant("Left"), Expression.Constant(true)));
                    }

                    if (middle.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                    {
                        var valueSaverMiddle = s_SaveExpectValueText.MakeGenericMethod(middle.DataType.Type);
                        middle = new SBExpressionData(Expression.Call(valueSaverMiddle, m_currentProcedure.ContextReferenceInternal, middle.ExpressionCode, Expression.Constant("Middle"), Expression.Constant(false)));
                    }

                    if (last.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                    {
                        var valueSaverLast = s_SaveExpectValueText.MakeGenericMethod(last.DataType.Type);
                        last = new SBExpressionData(Expression.Call(valueSaverLast, m_currentProcedure.ContextReferenceInternal, last.ExpressionCode, Expression.Constant("Right"), Expression.Constant(false)));
                    }
                }

                var op1 = context.op1.Type;
                var op2 = context.op2.Type;
                // TODO: Check if operator is returned
                var result = SpecialOperators.BetweenOperator.Resolve(this, first, op1, middle, op2, last);
                m_expressionData.Push(result);
            }
        }

        public override void ExitExpEqualsWithTolerance([NotNull] SBP.ExpEqualsWithToleranceContext context)
        {
            var tolerance = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            var expected = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            var value = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop(), reportIfUnresolved: true).NarrowGetValueType();
            if (CheckExpressionsForErrors(context, value, expected, tolerance))
            {
                var op = context.op.Type;
                // TODO: Check if operator is returned

                if (m_isSimpleExpectWithValue)
                {
                    if (value.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                    {
                        var valueSaverValue = s_SaveExpectValueText.MakeGenericMethod(value.DataType.Type);
                        value = new SBExpressionData(Expression.Call(valueSaverValue, m_currentProcedure.ContextReferenceInternal, value.ExpressionCode, Expression.Constant("Left"), Expression.Constant(true)));
                    }

                    if (expected.ExpressionCode.ToString() != "null") // This is only true when the expression is null. The string "null" turns into "\"null\""
                    {
                        var valueSaverExpected = s_SaveExpectValueText.MakeGenericMethod(expected.DataType.Type);
                        expected = new SBExpressionData(Expression.Call(valueSaverExpected, m_currentProcedure.ContextReferenceInternal, expected.ExpressionCode, Expression.Constant("Right"), Expression.Constant(false)));
                    }
                }

                var result = SpecialOperators.EqualsWithToleranceOperator.Resolve(this, value, op, expected, tolerance);
                m_expressionData.Push(result);
            }
        }

        public override void ExitExpCoalescing([NotNull] SBP.ExpCoalescingContext context)
        {
            var last = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop()).NarrowGetValueType();
            var first = this.ResolveForGetOperation(m_expressionData.Peek().Stack.Pop()).NarrowGetValueType();
            if (CheckExpressionsForErrors(context, first, last))
            {
                var result = SpecialOperators.CoalescingOperator.Resolve(this, first, last);
                m_expressionData.Push(result);
            }
        }

        #region Literals

        public override void ExitLiteralInteger([NotNull] SBP.LiteralIntegerContext context)
        {
            var str = context.GetText();

            long value = 0L;
            if (str.TryParseInt64(out value))
            {
                m_expressionData.Push(new SBExpressionData(value, context.Start));
            }
            else
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "Format error in: " + str);
                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.ExpressionError,
                    "Expression error.",
                    context.GetText(),
                    new TokenOrSection(context.Start, context.Stop, context.GetText())));
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
            double v;
            if (str.TryParseFloat(out v))
            {
                m_expressionData.Push(new SBExpressionData(v, context.Start));
            }
            else
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "Format error in: " + str);
                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.ExpressionError,
                    "Expression error.",
                    context.GetText(),
                    new TokenOrSection(context.Start, context.Stop, context.GetText())));
            }
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
            var t = type?.Type;
            if (t != null &&
                !expression.IsError() &&
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

        private bool CheckExpressionsForErrors(ParserRuleContext context, params SBExpressionData[] expressions)
        {
            if (expressions.All(e => e.IsResolved && !e.IsError()))
            {
                return true;
            }
            else
            {
                foreach (var e in expressions.Where(exp => exp.IsResolved == false && !exp.IsError()))
                {
                    if (e.ReferencedType != SBExpressionType.ExpressionError)
                    {
                        if (e.ReferencedType == SBExpressionType.UnknownIdentifier)
                        {
                            m_errors.SymanticError(e.Token.Line, e.Token.Column, false, "Unknown identifier: " + (e.Value as string));
                        }
                        else
                        {
                            throw new NotImplementedException("Type: " + e.ReferencedType.ToString());
                            //m_errors.SymanticError(e.Token.Line, e.Token.Column, false, "");
                        }
                    }
                }

                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.ExpressionError,
                    "Expression error.",
                    context.GetText(),
                    new TokenOrSection(context.Start, context.Stop, context.GetText())));
                return false;
            }
        }

        private bool CheckArgumentExpressionsForErrors(ParserRuleContext context, params SBExpressionData[] expressions)
        {
            if (expressions.All(e => e.IsResolved && !e.IsError()))
            {
                return true;
            }
            else
            {
                foreach (var e in expressions.Where(exp => exp.IsResolved == false && exp.ReferencedType > SBExpressionType.ExpressionError))
                {
                    if (e.ReferencedType == SBExpressionType.UnknownIdentifier)
                    {
                        m_errors.SymanticError(e.Token.Line, e.Token.Column, false, "Unknown identifier: " + (e.Value as string));
                    }
                    else
                    {
                        throw new NotImplementedException("Type: " + e.ReferencedType.ToString());
                        //m_errors.SymanticError(e.Token.Line, e.Token.Column, false, "");
                    }
                }

                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.ExpressionError,
                    "Expression error.",
                    context.GetText(),
                    new TokenOrSection(context.Start, context.Stop, context.GetText())));
                return false;
            }
        }

        private Expression CheckAndConvertValueForAssignment(Expression expression, Type targetType)
        {
            if (targetType.IsAssignableFrom(expression.Type))
            {
                return expression;
            }
            else
            {
                MethodInfo convert_mi = expression.Type.TryGetConvertOperator(targetType);
                if (convert_mi != null)
                {
                    var pars = convert_mi.GetParameters();
                    if (pars.Length > 1)
                    {
                        if (pars[0].ParameterType == typeof(IScriptFile))
                        {
                            if (m_file != null)
                            {
                                return Expression.Call(
                                    convert_mi,
                                    Expression.Call(s_GetFileReference, Expression.Constant(m_file.UniqueID)),
                                    expression);
                            }
                        }
                        else
                        {
                            throw new ParsingErrorException("Unhandled parameters on Create function.");
                        }
                    }
                    else if (pars.Length == 1)
                    {
                        return Expression.Call(convert_mi, expression);
                    }
                }
            }
            return null;    // No luck :-(
        }

        #endregion
    }
}
