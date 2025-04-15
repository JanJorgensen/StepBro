using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private static MethodInfo s_ExecuteDynamicObjectMethod = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ExecuteDynamicObjectMethod));
        private static MethodInfo s_ExecuteDynamicAsyncObjectMethod = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ExecuteDynamicAsyncObjectMethod));

        #region Arguments

        private SBExpressionData m_leftOfMethodCallExpression = null;   // TODO: add this to the stack somehow.

        public override void EnterArgument([NotNull] SBP.ArgumentContext context)
        {
            m_expressionData.Peek().ArgumentIndex = m_expressionData.Peek().Stack.Count;
            m_expressionData.Peek().ArgumentName = null;
        }

        public override void ExitArgument([NotNull] SBP.ArgumentContext context)
        {
            m_expressionData.Peek().Stack.Peek().ArgumentIndex = m_expressionData.Peek().ArgumentIndex;
            m_expressionData.Peek().Stack.Peek().ParameterName = m_expressionData.Peek().ArgumentName;
        }

        //public override void EnterArgumentWithoutName([NotNull] SBP.ArgumentWithoutNameContext context)
        //{
        //    m_argumentIndex = m_expressionData.Peek().Count;
        //}

        //public override void ExitArgumentWithoutName([NotNull] SBP.ArgumentWithoutNameContext context)
        //{
        //    m_expressionData.Peek().Peek().ArgumentIndex = m_argumentIndex;
        //}

        public override void ExitReferenceArgument([NotNull] SBP.ReferenceArgumentContext context)
        {
            string name = context.children[1].GetText();
            string refType = context.children[0].GetText();
            m_expressionData.Push(SBExpressionData.CreateIdentifier(name, refType));
        }

        public override void ExitArgumentName([NotNull] SBP.ArgumentNameContext context)
        {
            m_expressionData.Peek().ArgumentName = context.children[0].GetText();
        }

        //public override void ExitArgumentWithName([NotNull] SBP.ArgumentWithNameContext context)
        //{
        //    m_expressionData.Peek().Peek().ParameterName = context.children[0].GetText();
        //}

        public override void EnterArguments([NotNull] SBP.ArgumentsContext context)
        {
            m_expressionData.PushStackLevel("EnterArguments @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
        }

        public override void ExitArguments([NotNull] SBP.ArgumentsContext context)
        {
            // We need to turn the stack into a list so we can iterate through it and keep the right order
            // We need to get the data off the stack so we can use the "ResolveForGetOperation" method on it to make sure
            // the variables are resolved. This is specifically for global variables as when they are not resolved
            // they are VariableContainers which is not the same as the underlying type, and gives compile errors.

            List<SBExpressionData> stackData = m_expressionData.PopStackLevel().Stack.ToList();
            Stack<SBExpressionData> handledStackData = new Stack<SBExpressionData>();
            for (int i = stackData.Count() - 1; i >= 0; i--) // Need to go through backwards because Stack -> List -> Stack
            {
                handledStackData.Push(ResolveForGetOperation(stackData[i]));
            }

            m_arguments.Push(handledStackData);
        }

        public override void EnterMethodArguments([NotNull] SBP.MethodArgumentsContext context)
        {
            var left = m_expressionData.Pop();
            left = this.ResolveIfIdentifier(left, true);
            m_leftOfMethodCallExpression = left;

            m_expressionData.Push(left);
        }

        #endregion

        public override void EnterExpParens([NotNull] SBP.ExpParensContext context)
        {
            //var left = m_expressionData.Pop();
            //left = this.ResolveIfIdentifier(left);
            //m_expressionData.Push(left);
        }

        public override void ExitExpParens([NotNull] SBP.ExpParensContext context)
        {
            var argumentStack = m_arguments.Pop();
            var left = m_expressionData.Pop();
            this.HandleParensExpression(context, false, left, argumentStack, null, null);
        }

        private enum ParansExpressionType
        {
            MethodCall, ProcedureCall, FunctionCall,
            DynamicProcedureCall, DynamicFunctionCall,
            DynamicObjectMethodCall, Error
        }

        public void HandleParensExpression(
            Antlr4.Runtime.ParserRuleContext context,
            bool isCallStatement,
            SBExpressionData left,
            Stack<SBExpressionData> argumentStack,
            SBExpressionData assignmentTarget,
            PropertyBlock propertyBlock)
        {
            try
            {
                if (left.IsError())
                {
                    if (isCallStatement)
                    {
                        m_scopeStack.Peek().AddStatementCode(Expression.Empty());   // Add ampty statement, to make the rest of the error handling easier.
                    }
                    else
                    {
                        m_expressionData.Push(left);
                    }
                    return;
                }

                var leftType = left.DataType?.Type;
                List<SBExpressionData> arguments = new List<SBExpressionData>();
                if (argumentStack.Count > 0)
                {
                    while (argumentStack.Count > 0) arguments.Insert(0, this.ResolveIfIdentifier(argumentStack.Pop(), true)); // First pushed first in list.
                }

                var matchingMethods = new List<Tuple<MethodBase, int>>();
                int bestMatch = -1;
                var suggestedAssignmentsForMatchingMethods = new List<List<SBExpressionData>>();
                Expression instance = null;
                string instanceName = left.Instance;
                var callType = ParansExpressionType.MethodCall;
                var firstParIsThis = false; // Whether the first parameter of procedure (or method?) is a 'this' reference.
                TypeReference returnType = null;
                FileProcedure procedure = null;
                List<SBExpressionData> methodArguments = null;

                #region Identify call type and list methods with matching name

                IEnumerable<MethodInfo> methods = null;
                switch (left.ReferencedType)
                {
                    case SBExpressionType.Namespace:
                    case SBExpressionType.Constant:
                    case SBExpressionType.GlobalVariableReference:
                        m_errors.SymanticError(context.Start.Line, -1, false, $"\"{left.ToString()}\" is not a method, procedure or delegate.");
                        return;

                    case SBExpressionType.TypeReference:
                        {
                            foreach (var constructor in left.DataType.Type.GetConstructors())
                            {
                                List<SBExpressionData> suggestedAssignments = new List<SBExpressionData>();
                                var score = CheckConstructorArguments(constructor, arguments, suggestedAssignments);
                                if (score > 0)
                                {
                                    matchingMethods.Add(new Tuple<MethodBase, int>(constructor, score));
                                    suggestedAssignmentsForMatchingMethods.Add(suggestedAssignments);
                                }
                            }

                            ConstructorInfo selectedConstructor = null;

                            bestMatch = GetBestMatch(matchingMethods);
                            if (bestMatch >= 0)
                            {
                                selectedConstructor = matchingMethods[bestMatch].Item1 as ConstructorInfo;
                                var suggestedAssignments = suggestedAssignmentsForMatchingMethods[bestMatch];

                                methodArguments = new List<SBExpressionData>();
                                int i = 0;
                                foreach (var p in selectedConstructor.GetParameters())
                                {
                                    SBExpressionData argument = suggestedAssignments[i];
                                    if (argument == null || argument.ExpressionCode == null)
                                    {
                                        throw new NotImplementedException();    // Implicit argument must be found and added.
                                    }
                                    else
                                    {
                                        methodArguments.Add(argument);
                                    }
                                    i++;
                                }
                                List<Expression> argumentExpressions = new List<Expression>();
                                foreach (SBExpressionData cArg in suggestedAssignments)
                                {
                                    //argumentTypes.Add(ResolveForGetOperation(cArg).DataType.Type);
                                    argumentExpressions.Add(ResolveForGetOperation(cArg).ExpressionCode);
                                }
                                var expressionData = new SBExpressionData(Expression.New(selectedConstructor, argumentExpressions));

                                // left.DataType always contains the correct datatype.
                                // We could have a typedef which means the constructor will utilize the base of the typedef making the constructors type not always the correct
                                // one, so we override the datatype to ensure it is always correct.
                                expressionData.DataType = left.DataType;

                                m_expressionData.Push(expressionData);
                                return;
                            }
                            else
                            {

                                foreach (var method in left.DataType.Type.GetMethods().Where(m => m.Name == "Create"))
                                {
                                    List<SBExpressionData> suggestedAssignments = new List<SBExpressionData>();
                                    var score = CheckArguments(method, false, false, null, null, null, null, null, null, arguments, suggestedAssignments, null);
                                    if (score > 0)
                                    {
                                        matchingMethods.Add(new Tuple<MethodBase, int>(method, score));
                                        suggestedAssignmentsForMatchingMethods.Add(suggestedAssignments);
                                    }
                                }

                                bestMatch = GetBestMatch(matchingMethods);
                                if (bestMatch >= 0)
                                {

                                }
                                else
                                {
                                    // Handle none or more than one alternative
                                    if (matchingMethods.Count() > 0)
                                    {
                                        // Multiple methods can be used, all fit the call.
                                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"Ambiguity in method resolve for constructor: {left.Token.Text}.");
                                    }
                                    else if (methods?.Count() > 0 && matchingMethods.Count() == 0)
                                    {
                                        // The method exists, but no method fits with the given call
                                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"No constructor could be found with matching parameters.");
                                    }
                                    else
                                    {
                                        // The method does not exist
                                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"No public constructor could be found.");
                                    }

                                    // Set the call type to error so we do not get an exception
                                    callType = ParansExpressionType.Error;
                                }
                            }
                        }
                        break;

                    case SBExpressionType.Identifier:
                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"\"{(string)(left.Value)}\" is unresolved.");
                        return;

                    case SBExpressionType.Expression:
                    case SBExpressionType.PropertyReference:
                    case SBExpressionType.LocalVariableReference:
                        if (left.DataType.Type.IsDelegate())
                        {
                            methods = new MethodInfo[] { leftType.GetMethod("Invoke") };
                        }
                        else if (typeof(IProcedureReference).IsAssignableFrom(leftType))
                        {
                            if (leftType.IsGenericType && leftType.GetGenericTypeDefinition() == typeof(IProcedureReference<>))
                            {
                                var m = leftType.GetGenericArguments()[0].GetMethod("Invoke");
                                System.Diagnostics.Debug.Assert(m != null);
                                methods = new MethodInfo[] { m };
                                callType = isCallStatement ? ParansExpressionType.ProcedureCall : ParansExpressionType.FunctionCall;
                                if (left.DataType.HasProcedureReference)
                                {
                                    var proc = left.DataType.DynamicType as FileProcedure;
                                    firstParIsThis = proc.IsFirstParameterThisReference;
                                    instance = left.InstanceCode;
                                }
                            }
                            else
                            {
                                // Make rest of this method treat this as a method call rather than a procedure call.
                                instance = left.ExpressionCode;
                                callType = isCallStatement ? ParansExpressionType.DynamicProcedureCall : ParansExpressionType.DynamicFunctionCall;
                            }
                        }
                        else
                        {
                            m_errors.SymanticError(context.Start.Line, -1, false, $"\"{left.ToString()}\" is not a procedure reference or a delegate.");
                            return;
                        }
                        break;

                    case SBExpressionType.MethodReference:
                        instance = left.ExpressionCode;
                        methods = (IEnumerable<MethodInfo>)left.Value;
                        break;

                    case SBExpressionType.ProcedureReference:
                        {
                            callType = isCallStatement ? ParansExpressionType.ProcedureCall : ParansExpressionType.FunctionCall;
                            // Note: never anonymous procedure here.
                            procedure = (left.Value as IProcedureReference).ProcedureData as FileProcedure;
                            if (!isCallStatement && (procedure.Flags & ProcedureFlags.IsFunction) == ProcedureFlags.None)
                            {
                                m_errors.SymanticError(context.Start.Line, -1, false, "Only procedures marked as 'function' can be called from expressions.");
                            }
                            if (procedure.DataType != null && procedure.DataType.Type != typeof(UnresolvedProcedureType))
                            {
                                var m = procedure.DataType.Type.GetGenericArguments()[0].GetMethod("Invoke");
                                System.Diagnostics.Debug.Assert(m != null);
                                methods = new MethodInfo[] { m };
                            }
                            else
                            {
                                m_errors.SymanticError(context.Start.Line, -1, false, "Unresolved signature for procedure.");
                            }
                            instance = left.InstanceCode;
                            instanceName = left.Instance;
                        }
                        break;

                    case SBExpressionType.DynamicObjectMember:
                    case SBExpressionType.DynamicAsyncObjectMember:
                        {
                            instance = left.InstanceCode;
                            var name = left.Value as string;
                            SBExpressionData sequencialFirstArguments = null, namedArguments = null, sequencialLastArguments = null;
                            if (this.CreateArgumentsForDynamicCall(
                                arguments,
                                ref sequencialFirstArguments,
                                ref namedArguments,
                                ref sequencialLastArguments))
                            {
                                MethodInfo dynamicExecutor = (left.ReferencedType == SBExpressionType.DynamicObjectMember) ? s_ExecuteDynamicObjectMethod : s_ExecuteDynamicAsyncObjectMethod;
                                Expression dynamicCall = Expression.Call(
                                    dynamicExecutor,
                                    m_currentProcedure?.ContextReferenceInternal,
                                    Expression.Convert(instance, (left.ReferencedType == SBExpressionType.DynamicObjectMember) ? typeof(IDynamicStepBroObject) : typeof(IDynamicAsyncStepBroObject)),
                                    Expression.Constant(name),
                                    sequencialFirstArguments.ExpressionCode);

                                returnType = (left.ReferencedType == SBExpressionType.DynamicObjectMember) ? TypeReference.TypeObject : TypeReference.TypeAsyncObject;
                                if (m_callAssignmentAwait)
                                {
                                    dynamicCall = this.MakeAwaitOperation(dynamicCall, context, true, m_callAssignmentTarget?.DataType.Type);
                                    returnType = (TypeReference)dynamicCall.Type; //new TypeReference(dynamicCall.Type);
                                    m_callAssignmentAwait = false;
                                }

                                if (isCallStatement)
                                {
                                    m_scopeStack.Peek().AddStatementCode(dynamicCall);
                                }
                                else
                                {
                                    m_expressionData.Push(
                                        new SBExpressionData(
                                            SBExpressionType.Expression,
                                            returnType,
                                            dynamicCall,
                                            automaticTypeConvert: true));
                                }

                                var conditionalReturn = Expression.Condition(
                                    Expression.Call(s_PostExpressionStatement, m_currentProcedure.ContextReferenceInternal),
                                    Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type)),
                                    Expression.Empty());

                                m_scopeStack.Peek().AddStatementCode(conditionalReturn);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            return;     // All done for now.
                        }

                    case SBExpressionType.ExpressionError:
                    case SBExpressionType.UnsupportedOperation:
                    case SBExpressionType.UnknownIdentifier:
                        return;

                    default:
                        m_errors.InternalError(context.Start.Line, -1, $"Unhandled expression type: \"{left.ReferencedType}\".");
                        break;
                }

                #endregion

                MethodInfo selectedMethod = null;

                Expression earlyExitExpression = null;
                if (m_currentProcedure != null) earlyExitExpression = Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type));  // When told to exit the procedure now.

                #region Dynamic Procedure Call

                if (callType == ParansExpressionType.DynamicProcedureCall)
                {
                    selectedMethod = s_DynamicProcedureCall;

                    SBExpressionData sequencialFirstArguments = null, namedArguments = null, sequencialLastArguments = null;
                    if (this.CreateArgumentsForDynamicCall(
                        arguments,
                        ref sequencialFirstArguments,
                        ref namedArguments,
                        ref sequencialLastArguments))
                    {
                        Expression callProcedure = Expression.Call(
                                s_DynamicProcedureCall,
                                instance,
                                m_currentProcedure?.ContextReferenceInternal,
                                Expression.Constant(null, typeof(PropertyBlock)),
                                sequencialFirstArguments.ExpressionCode,
                                namedArguments.ExpressionCode,
                                sequencialLastArguments.ExpressionCode);

                        if (m_callAssignmentTarget != null)
                        {
                            callProcedure = Expression.Assign(m_callAssignmentTarget.ExpressionCode, callProcedure);
                        }

                        m_scopeStack.Peek().AddStatementCode(
                            Expression.TryCatch(
                                Expression.Block(typeof(void), callProcedure),
                                Expression.Catch(
                                    typeof(RequestEarlyExitException),
                                    earlyExitExpression),
                                Expression.Catch(
                                    typeof(Exception),
                                    Expression.Rethrow()))
                            );
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    return;     // All done.
                }
                else if (callType == ParansExpressionType.DynamicFunctionCall)
                {
                    selectedMethod = s_DynamicFunctionCall;
                    throw new NotImplementedException("DYNAMIC FUNCTION CALL");
                }

                #endregion

                // Fall through: Not a dynamic call

                if (methods == null || methods.Any() == false)
                {
                    throw new Exception("No methods identified; should be handled and reported earlier.");
                }

                #region Find matching method and setup arguments

                foreach (MethodInfo m in methods)
                {
                    var constructedMethod = m;
                    var extensionInstance = (m.IsExtension() || (callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall)) ? instance : null;
                    List<SBExpressionData> suggestedAssignments = new List<SBExpressionData>();
                    int score = this.CheckMethodArguments(
                        ref constructedMethod,
                        callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall,
                        firstParIsThis,
                        procedure?.GetFormalParameters(),
                        instance,
                        instanceName,
                        m_currentProcedure?.ContextReferenceInternal,
                        null, // TODO: IScriptFile reference
                        extensionInstance,
                        arguments,
                        suggestedAssignments);
                    if (score > 0)
                    {
                        matchingMethods.Add(new Tuple<MethodBase, int>(constructedMethod, score));
                        suggestedAssignmentsForMatchingMethods.Add(suggestedAssignments);
                    }
                }

                bestMatch = GetBestMatch(matchingMethods);
                if (bestMatch >= 0)
                {
                    selectedMethod = matchingMethods[bestMatch].Item1 as MethodInfo;
                    var suggestedAssignments = suggestedAssignmentsForMatchingMethods[bestMatch];

                    returnType = new TypeReference(selectedMethod.ReturnType);
                    if (selectedMethod.IsExtension())
                    {
                        instance = null;
                    }
                    methodArguments = new List<SBExpressionData>();
                    int i = 0;
                    foreach (var p in selectedMethod.GetParameters())
                    {
                        SBExpressionData argument = suggestedAssignments[i];
                        if (argument == null || argument.ExpressionCode == null)
                        {
                            throw new NotImplementedException();    // Implicit argument must be found and added.
                        }
                        else
                        {
                            methodArguments.Add(argument);
                        }
                        i++;
                    }
                }
                else
                {
                    var callTargetDescriptor = methods.First().Name;
                    if (left.Value != null)
                    {
                        if (left.Value is IProcedureReference reference)
                        {
                            callTargetDescriptor = reference.Name;
                        }
                        else if (left.Value is List<MethodInfo> methodList)
                        {
                            callTargetDescriptor = methodList[0].Name;
                        }
                        else if (left.ReferencedType == SBExpressionType.LocalVariableReference)
                        {
                            if (left.Value is IdentifierInfo info)
                            {
                                callTargetDescriptor = info.Name;
                            }
                            else if (left.Value is IProcedureReference proc)
                            {
                                callTargetDescriptor = proc.Name;
                            }
                            else if (left.Value is FileVariable variable)
                            {
                                callTargetDescriptor = $"target referenced by '{variable.Name}'";     // TODO: Improve on this .
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Value type: " + left.Value.GetType().FullName);
                                //throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Value type: " + left.Value.GetType().FullName);
                            //throw new NotImplementedException();
                        }
                    }



                    // Handle none or more than one alternative
                    if (matchingMethods.Count() > 0)
                    {
                        // Multiple methods can be used, all fit the call.
                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"Ambiguity in method or procedure resolved for {callTargetDescriptor}.");
                    }
                    else if (methods.Count() > 0 && matchingMethods.Count() == 0)
                    {
                        // The method exists, but no method fits with the given call
                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"All arguments are not matching for {callTargetDescriptor}\".");


                    }
                    else
                    {
                        // The method does not exist
                        m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"Unknown method or identifier {callTargetDescriptor}.");
                    }



#if DEBUG

                    foreach (MethodInfo m in methods)
                    {
                        var constructedMethod = m;
                        var extensionInstance = (m.IsExtension() || (callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall)) ? instance : null;
                        List<SBExpressionData> suggestedAssignments = new List<SBExpressionData>();
                        int score = this.CheckMethodArguments(
                            ref constructedMethod,
                            callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall,
                            firstParIsThis,
                            procedure?.GetFormalParameters(),
                            instance,
                            instanceName,
                            m_currentProcedure?.ContextReferenceInternal,
                            null, // TODO: IScriptFile reference
                            extensionInstance,
                            arguments,
                            suggestedAssignments);
                    }
#endif

                    // Set the call type to error so we do not get an exception
                    callType = ParansExpressionType.Error;
                }

                #endregion

                #region Call the target procedure or method

                if (callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall)
                {
                    Expression procedureReferenceExp = null;
                    Type delegateType = null;
                    Type procedureReferenceType = null;
                    Expression theDelegate = null;

                    switch (left.ReferencedType)
                    {
                        case SBExpressionType.Expression:
                        case SBExpressionType.PropertyReference:
                        case SBExpressionType.LocalVariableReference:
                            {
                                procedureReferenceExp = left.ExpressionCode;

                                if (leftType.IsGenericType && leftType.GetGenericTypeDefinition() == typeof(IProcedureReference<>))
                                {
                                    delegateType = leftType.GetGenericArguments()[0];
                                    procedureReferenceType = leftType; //typeof(IProcedureReference<>).MakeGenericType(delegateType);
                                }
                                else
                                {
                                    if (leftType == typeof(IProcedureReference))
                                    {
                                        // Anonymous procedure type; do a dynamic call !!!
                                        throw new NotImplementedException("Dynamic call needs to be implemented.");
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Not a procedure reference");
                                    }
                                }
                            }
                            break;

                        case SBExpressionType.ProcedureReference:
                            {
                                var procRef = left.Value as IProcedureReference;
                                procedure = procRef.ProcedureData as FileProcedure;
                                returnType = procedure.ReturnType;
                                delegateType = procedure.DelegateType;
                                procedureReferenceType = procedure.ProcedureReferenceType;

                                if (delegateType != null)
                                {
                                    var getStaticProcedureReferenceTyped = s_GetProcedureTyped.MakeGenericMethod(delegateType);
                                    int fileID = Object.ReferenceEquals(procedure.ParentFile, m_file) ? -1 : ((ScriptFile)procedure.ParentFile).UniqueID;

                                    procedureReferenceExp = Expression.Call(
                                        getStaticProcedureReferenceTyped,
                                        m_currentProcedure.ContextReferenceInternal,
                                        Expression.Constant(fileID),
                                        Expression.Constant(procedure.UniqueID));
                                }
                                else
                                {
                                    int fileID = Object.ReferenceEquals(procedure.ParentFile, m_file) ? -1 : ((ScriptFile)procedure.ParentFile).UniqueID;

                                    procedureReferenceExp = Expression.Call(
                                        s_GetProcedure,
                                        m_currentProcedure.ContextReferenceInternal,
                                        Expression.Constant(fileID),
                                        Expression.Constant(procedure.UniqueID));
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    theDelegate = Expression.Property(procedureReferenceExp, "RuntimeProcedure");

                    if (isCallStatement)
                    {
                        // C# version of the generated code:
                        // var callcontext = context.EnterNewScriptContext(MySecond, ContextLogOption.Normal).Disposer())
                        // try { MySecondProcedure(callcontext.Value, v1, v2); }
                        // finally { callcontext.Dispose(); }

                        var procRefVar = Expression.Variable(procedureReferenceType, "procRef");
                        var callArgsArray = Expression.Variable(typeof(object[]), "__callArgs" + context.Start.Line + "_" + context.Start.Column);
                        var subCallContextContainer = Expression.Variable(typeof(InternalDisposer<IScriptCallContext>), "subCallContextContainer");
                        var subCallContext = Expression.Property(subCallContextContainer, "Value");
                        // Replace the callContext argument with new context reference.
                        methodArguments[0] = new SBExpressionData(subCallContext);
                        // TODO: Generate list of arguments in callArgsArray (inputs only)
                        // TODO: Replace input arguments (not out's and ref's) of methodArguments with __callArgs[n].
                        var assignProcRefVar = Expression.Assign(procRefVar, procedureReferenceExp);

                        var createSubContext = Expression.New(
                                        typeof(InternalDisposer<IScriptCallContext>).GetConstructor(new Type[] { typeof(IScriptCallContext) }),
                                        Expression.Call(
                                            m_currentProcedure.ContextReferenceInternal,
                                            typeof(IScriptCallContext).GetMethod(nameof(IScriptCallContext.EnterNewScriptContext), new Type[] { typeof(IProcedureReference), typeof(ContextLogOption), typeof(bool), typeof(object[]) }),
                                            procRefVar,
                                            Expression.Constant(ContextLogOption.Normal),
                                            Expression.Constant(false),
                                            callArgsArray));

                        var invokeMethod = delegateType.GetMethod("Invoke");

                        Expression callProcedure = Expression.Call(
                                    Expression.Property(
                                        procRefVar,
                                        "RuntimeProcedure"),
                                    invokeMethod,
                                    methodArguments.Select(a => a.ExpressionCode).ToArray());
                        if (m_callAssignmentAwait)
                        {
                            callProcedure = this.MakeAwaitOperation(callProcedure, context, true, m_callAssignmentTarget?.DataType.Type);
                            returnType = new TypeReference(callProcedure.Type);
                        }
                        if (m_callAssignmentTarget != null)
                        {
                            callProcedure = Expression.Assign(m_callAssignmentTarget.ExpressionCode, callProcedure);
                        }

                        var disposeSubContext = Expression.Call(
                            subCallContextContainer,
                            typeof(InternalDisposer<IScriptCallContext>).GetMethod("Dispose"));

                        ParameterExpression catchParam = Expression.Parameter(typeof(Exception));

                        var reportUnhandledException = Expression.Call(
                            Expression.Convert(subCallContext, typeof(ScriptCallContext)),
                            typeof(ScriptCallContext).GetMethod(nameof(ScriptCallContext.ReportError), new Type[] { typeof(string), typeof(ErrorID), typeof(Exception) }),
                            Expression.Constant("Unhandled Exception"),
                            Expression.Constant(null, typeof(ErrorID)),
                            catchParam);


                        var completeProcedureCall = Expression.Block(
                            new ParameterExpression[] { procRefVar, callArgsArray, subCallContextContainer },
                            Expression.TryCatchFinally(
                                Expression.Block(
                                    assignProcRefVar,
                                    Expression.Assign(callArgsArray, Expression.Convert(Expression.Constant(null), typeof(object[]))),
                                    Expression.Assign(subCallContextContainer, createSubContext),
                                    callProcedure,
                                    Expression.Condition(
                                        Expression.Call(s_PostProcedureCallResultHandling, m_currentProcedure.ContextReferenceInternal, subCallContext),
                                        earlyExitExpression,
                                        Expression.Empty())
                                    ),
                                disposeSubContext,
                                Expression.Catch(
                                    typeof(UnhandledExceptionInScriptException),
                                    Expression.Rethrow()),
                                Expression.Catch(
                                    catchParam,
                                    Expression.Block(
                                        reportUnhandledException,
                                        Expression.Throw(
                                            Expression.New(
                                                typeof(UnhandledExceptionInScriptException).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0],
                                                catchParam,
                                                Expression.Convert(subCallContext, typeof(ScriptCallContext))))))
                                ));

                        m_scopeStack.Peek().AddStatementCode(completeProcedureCall);
                    }
                    else
                    {
                        var subCallContext = Expression.New(
                            typeof(FunctionCallContextWrapper).GetConstructor(
                                new Type[] { typeof(IScriptCallContext),
                                            typeof(int),
                                            typeof(int) }),
                            m_currentProcedure.ContextReferenceInternal,
                            Expression.Constant(context.Start.Line),
                            Expression.Constant(-1) /* TODO: get line and column from the 'left' parameter */);

                        methodArguments[0] = new SBExpressionData(subCallContext);

                        m_expressionData.Push(
                            new SBExpressionData(
                                SBExpressionType.Expression,
                                returnType,
                                Expression.Call(
                                    theDelegate,
                                    delegateType.GetMethod("Invoke"),
                                    methodArguments.Select(a => a.ExpressionCode).ToArray()),
                                null));
                    }
                }
                else if (callType != ParansExpressionType.Error)    // Not a procedure or function; a method.
                {
                    switch (left.ReferencedType)
                    {
                        case SBExpressionType.Expression:
                        case SBExpressionType.PropertyReference:
                        case SBExpressionType.LocalVariableReference:
                            if (leftType.IsDelegate())
                            {
                                m_expressionData.Push(
                                    new SBExpressionData(
                                        SBExpressionType.Expression,
                                    (TypeReference)(leftType.GetMethod("Invoke").ReturnType),
                                    Expression.Invoke(left.ExpressionCode, methodArguments.Select(a => a.ExpressionCode).ToArray()),
                                    null));
                            }
                            else
                            {
                                throw new NotImplementedException("Can we end up here?");
                            }
                            break;

                        case SBExpressionType.MethodReference:
                            {
                                Expression callMethod = Expression.Call(
                                    instance,
                                    selectedMethod,
                                    methodArguments.Select(a => a.ExpressionCode).ToArray());
                                if (isCallStatement)
                                {
                                    if (m_callAssignmentAwait)
                                    {
                                        callMethod = this.MakeAwaitOperation(callMethod, context, true, m_callAssignmentTarget?.DataType.Type);
                                        returnType = new TypeReference(callMethod.Type);
                                    }
                                    if (m_callAssignmentTarget != null)
                                    {
                                        callMethod = SBExpressionData.NarrowGetValueTypeByConverting(callMethod, callMethod.Type);
                                        callMethod = Expression.Assign(m_callAssignmentTarget.ExpressionCode, callMethod);
                                    }

                                    var conditionalReturn = Expression.Condition(
                                        Expression.Call(s_PostExpressionStatement, m_currentProcedure.ContextReferenceInternal),
                                        Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type)),
                                        Expression.Empty());

                                    m_scopeStack.Peek().AddStatementCode(Expression.Block(callMethod, conditionalReturn));
                                }
                                else
                                {
                                    m_expressionData.Push(
                                        new SBExpressionData(
                                            SBExpressionType.Expression,
                                            returnType,
                                            callMethod,
                                            null));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    m_expressionData.Push(
                        new SBExpressionData(
                            SBExpressionType.ExpressionError));
                }
                #endregion
            }
            catch (Exception e)
            {
                m_errors.InternalError(left.Token.Line, left.Token.Column, e.Message);
            }
        }

        private static int GetBestMatch(List<Tuple<MethodBase, int>> matches)
        {
            int bestMatch = -1;
            if (matches.Count == 1)
            {
                bestMatch = 0;
            }
            else if (matches.Count > 1)
            {
                int max = matches[0].Item2;
                int maxAt = 0;
                int numAtMax = 1;
                for (var i = 1; i < matches.Count; i++)
                {
                    if (matches[i].Item2 > max)
                    {
                        max = matches[i].Item2;
                        maxAt = i;
                        numAtMax = 1;
                    }
                    else if (matches[i].Item2 == max)
                    {
                        numAtMax++;
                    }
                }
                if (numAtMax == 1)
                {
                    bestMatch = maxAt;
                }
            }
            return bestMatch;
        }

        private enum ArgumentInsertState
        {
            Initial,
            ExtensionInstance,
            //ImplicitArguments,
            ProcedureContext,
            ThisReference,
            Mandatory,
            Named,
            //ArgumentList,
            Params,
            NoArguments
        }

        internal int CheckConstructorArguments(
            ConstructorInfo constructor,
            List<SBExpressionData> arguments,
            List<SBExpressionData> suggestedAssignmentsOut,
            PropertyBlock propertyBlock = null)
        {
            return this.CheckArguments(
                constructor, false, false,
                null, null, null,
                null, null, null,
                arguments, suggestedAssignmentsOut, propertyBlock);
        }

        internal int CheckMethodArguments(
            ref MethodInfo method,
            bool isProcedure, bool firstParIsThis,
            List<ParameterData> procedureParameters,
            Expression instance, string instanceName,
            Expression contextReference,
            Expression homeFileReference,
            Expression extensionInstance,
            List<SBExpressionData> arguments,
            List<SBExpressionData> suggestedAssignmentsOut,
            PropertyBlock propertyBlock = null)
        {
            // Construct method if generic and extension method.
            if (method.IsGenericMethodDefinition && extensionInstance != null)
            {
                // NOTE: This has to be improved with checks to avoid mistakes.
                var t = extensionInstance.Type;
                if (extensionInstance.Type.IsConstructedGenericType)
                {
                    t = t.GetGenericArguments()[0];
                }
                method = method.MakeGenericMethod(t);
            }

            return this.CheckArguments(
                method, isProcedure, firstParIsThis,
                procedureParameters, instance, instanceName,
                contextReference, homeFileReference, extensionInstance, arguments, suggestedAssignmentsOut, propertyBlock);
        }

        internal int CheckArguments(
            MethodBase method,
            bool isProcedure, bool firstParIsThis,
            List<ParameterData> procedureParameters,
            Expression instance, string instanceName,
            Expression contextReference,
            Expression homeFileReference,
            Expression extensionInstance,
            List<SBExpressionData> arguments,
            List<SBExpressionData> suggestedAssignmentsOut,
            PropertyBlock propertyBlock = null)
        {
            ListElementPicker<SBExpressionData> argPicker = new ListElementPicker<SBExpressionData>(arguments);
            ArgumentInsertState state = ArgumentInsertState.Initial;
            bool thisParameterHandled = false;
            int matchScore = 1000;

            if (isProcedure) state = ArgumentInsertState.ProcedureContext;
            else if (extensionInstance != null) state = ArgumentInsertState.ExtensionInstance;

            var parameters = method.GetParameters();

            // Run through all the parameters; all must be assigned.
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (state == ArgumentInsertState.ExtensionInstance)
                {
                    SBExpressionData extensionInstanceData = new SBExpressionData(extensionInstance);
                    suggestedAssignmentsOut.Add(extensionInstanceData);
                    thisParameterHandled = true;
                    state = ArgumentInsertState.Initial;
                    var matching = CheckParameterArgumentTypeMatch(p, extensionInstanceData);
                    switch (matching)
                    {
                        case TypeMatch.NoTypeMatch:
                        case TypeMatch.UnsupportedSource:
                            matchScore = 0; // If it is incorrect, we should not use it.
                            break;
                        case TypeMatch.MatchLowest:
                        case TypeMatch.MatchLevel3:
                        case TypeMatch.MatchLevel2:
                        case TypeMatch.MatchLevel1:
                        case TypeMatch.ExactMatch:
                            matchScore += 10 - (((int)(TypeMatch.ExactMatch - matching)) * 5); // If we have the correct extension instance then it is more correct than a method that does not use the extension instance (This is a choice we have made).
                            break;
                    }
                    continue;
                }
                else if (state == ArgumentInsertState.ProcedureContext)
                {
                    suggestedAssignmentsOut.Add(new SBExpressionData(contextReference));
                    if (extensionInstance != null) state = ArgumentInsertState.ExtensionInstance;
                    else state = ArgumentInsertState.Initial;
                    continue;
                }
                else if (state == ArgumentInsertState.Initial)
                {
                    if (ImplicitAttribute.IsImplicit(p))
                    {
                        if (p.ParameterType.IsAssignableFrom(typeof(ICallContext)))
                        {
                            if (isProcedure)
                            {
                                suggestedAssignmentsOut.Add(new SBExpressionData(m_currentProcedure.ContextReferenceInternal));
                            }
                            else
                            {
                                string prefix = null;
                                if (!String.IsNullOrEmpty(instanceName))
                                {
                                    prefix = instanceName;
                                }
                                else if (method.DeclaringType != typeof(ScriptUtils))
                                {
                                    prefix = method.DeclaringType.Name;
                                }
                                var objectRef = (Expression)Expression.Constant(null, typeof(object));
                                if (instance != null) objectRef = instance;
                                var contextCreator = Expression.Call(
                                    s_CreateMethodCallContext,
                                    contextReference,
                                    Expression.Constant(prefix, typeof(string)),
                                    objectRef,
                                    Expression.Constant(method.Name));
                                suggestedAssignmentsOut.Add(new SBExpressionData(contextCreator));
                            }
                            continue;
                        }
                        else if (p.ParameterType.IsAssignableFrom(typeof(IScriptFile)))
                        {
                            if (m_isInVariableInitializer)
                            {
                                suggestedAssignmentsOut.Add(new SBExpressionData(m_variableInitializerParameterScriptFile));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            // TODO: Parsing error.
                            throw new NotImplementedException();
                        }
                    }
                    else if (!thisParameterHandled && firstParIsThis)
                    {
                        state = ArgumentInsertState.ThisReference;
                    }
                    else
                    {
                        state = ArgumentInsertState.Mandatory;
                    }
                }

                if (state != ArgumentInsertState.ThisReference)
                {
                    if (argPicker.UnpickedCount == 0 && state != ArgumentInsertState.ThisReference)
                    {
                        state = ArgumentInsertState.NoArguments;
                    }
                    else if (p.IsDefined(typeof(ParamArrayAttribute)))
                    {
                        state = ArgumentInsertState.Params;
                    }
                }

                if (state == ArgumentInsertState.ThisReference)
                {
                    if (instance != null)
                    {
                        thisParameterHandled = true;
                        if (p.ParameterType.IsAssignableFrom(instance.Type))
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(instance));
                            state = ArgumentInsertState.Mandatory;  // Not sure about this...
                        }
                        else
                        {
                            return 0;   // Wrong type of 'this' reference
                        }
                    }
                    else
                    {
                        state = ArgumentInsertState.Mandatory;  // The parameter must be set from a normal argument, then.
                    }
                }

                if (state == ArgumentInsertState.Mandatory)
                {
                    if (p.ParameterType == typeof(ArgumentList))
                    {
                        if (i < (parameters.Length - 1)) return 0;  // There are more parameters after the ArgumentList; that's not okay.
                        var unnamed = new List<SBExpressionData>();
                        var named = new List<SBExpressionData>();
                        bool namedFound = false;
                        while (argPicker.UnpickedCount > 0)
                        {
                            if (argPicker.Current.IsNamed) namedFound = true;
                            if (!namedFound) unnamed.Add(argPicker.Pick());
                            else
                            {
                                if (!argPicker.Current.IsNamed) return 0; // An unnamed after a named is not okay.
                                named.Add(argPicker.Pick());
                            }
                        }

                        Expression unnamedArray = Expression.Constant(null, typeof(object[]));
                        Expression namedArray = Expression.Constant(null, typeof(NamedArgument[]));

                        if (unnamed.Count > 0)
                        {
                            unnamedArray = Expression.NewArrayInit(typeof(object), unnamed.Select(ex => ex.ExpressionCode).ToArray());
                        }
                        if (named.Count > 0)
                        {
                            var ctor = typeof(NamedArgument).GetConstructor(new Type[] { typeof(string), typeof(object) });
                            namedArray = Expression.NewArrayInit(
                                typeof(NamedArgument),
                                named.Select(ex => Expression.New(ctor, Expression.Constant(ex.ParameterName), Expression.Convert(ex.ExpressionCode, typeof(object)))).ToArray());
                        }

                        Expression listCreator = Expression.New(
                            typeof(ArgumentList).GetConstructor(new Type[] { typeof(object[]), typeof(NamedArgument[]) }),
                            unnamedArray,
                            namedArray);
                        suggestedAssignmentsOut.Add(new SBExpressionData(listCreator));

                        return matchScore - (50 * 1);
                    }
                    else
                    {
                        TypeMatch matching = TypeMatch.NoTypeMatch;
                        SBExpressionData convertedAssignment = null;
                        if (argPicker.Current.IsNamed)
                        {
                            if (argPicker.AllBeforeCurrentArePickedAndOthersUnpicked())
                            {
                                state = ArgumentInsertState.Named;
                            }
                            else
                            {
                                // TODO: report error; unexpected that args before current are not picked.
                                return 0;
                            }
                        }
                        else if (argPicker.Current.IsUnknownIdentifier)
                        {
                            if (p.ParameterType.IsEnum)
                            {
                                if (Enum.TryParse(p.ParameterType, (string)argPicker.Current.Value, out object value))
                                {
                                    suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Constant(value, p.ParameterType)));
                                }
                                else
                                {
                                    throw new NotImplementedException("Handling of unknown enum literal or enum short-form not implemented.");
                                }
                            }
                            else if (p.ParameterType == typeof(Identifier))
                            {
                                var ctor = typeof(Identifier).GetConstructor(new Type[] { typeof(string) });
                                suggestedAssignmentsOut.Add(new SBExpressionData(Expression.New(ctor, Expression.Constant((string)(argPicker.Pick().Value)))));
                            }
                        }
                        else if ((matching = CheckParameterArgumentTypeMatch(p, argPicker.Current)) >= TypeMatch.MatchLowest)
                        {
                            var argExpression = ResolveForGetOperation(argPicker.Pick(), (TypeReference)p.ParameterType);
                            matchScore -= (((int)(TypeMatch.ExactMatch - matching)) * 5);
                            switch (matching)
                            {
                                case TypeMatch.MatchLowest:
                                    argExpression = argExpression.NewExpressionCode(Expression.Convert(argExpression.ExpressionCode, typeof(object)));
                                    break;
                                case TypeMatch.MatchLevel3:
                                case TypeMatch.MatchLevel2:
                                case TypeMatch.MatchLevel1:
                                case TypeMatch.ExactMatch:
                                    break;
                                default:
                                    throw new InvalidOperationException("Should not be here");
                            }
                            suggestedAssignmentsOut.Add(argExpression);
                            continue;   // next parameter
                        }
                        else if (!Object.ReferenceEquals(convertedAssignment = AssignmentExpressionCreateCastOrConvertIfNeeded(null, argPicker.Current, (TypeReference)p.ParameterType), argPicker.Current) && convertedAssignment != null)
                        {
                            argPicker.Pick();
                            matchScore -= 30;   // The argument was converted; an exact match would be "better".
                            suggestedAssignmentsOut.Add(convertedAssignment);
                            //{
                            //    var casted = AssignmentExpressionCreateCastOrConvertIfNeeded(null, extensionInstanceData, (TypeReference)p.ParameterType);
                            //    if (!Object.ReferenceEquals(casted, extensionInstanceData))
                            //    {

                            //    }
                            //}
                            //break;

                        }
                        else if (argPicker.Current.DataType.Type == typeof(Int64) && p.ParameterType.IsPrimitiveIntType())
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Convert(argPicker.Pick().ExpressionCode, p.ParameterType)));
                            matchScore -= 5;
                            continue;   // next parameter
                        }
                        else if (argPicker.Current.DataType.Type == typeof(Double) && p.ParameterType == typeof(Single))
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Convert(argPicker.Pick().ExpressionCode, p.ParameterType)));
                            matchScore -= 20;    // Matching a 'single' is not as good as matching the exact same type.
                            continue;   // next parameter
                        }
                        else if (argPicker.Current.DataType.IsInt() && (p.ParameterType == typeof(Double) || p.ParameterType == typeof(Single)))
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Convert(argPicker.Pick().ExpressionCode, p.ParameterType)));
                            matchScore -= 40;    // Matching an integer is not as good as matching the exact same type.
                            continue;   // next parameter
                        }
                        else if (argPicker.Current.DataType.Type == typeof(string) && p.ParameterType == typeof(Identifier))
                        {
                            suggestedAssignmentsOut.Add(
                                new SBExpressionData(Expression.New(typeof(Identifier).GetConstructor(new Type[] { typeof(string) }), argPicker.Pick().ExpressionCode)));
                            matchScore -= 40;    // Matching an integer is not as good as matching the exact same type.
                            continue;   // next parameter
                        }
                        else
                        {
                            state = ArgumentInsertState.Named;  // Try finding the argument by name, then.
                        }
                    }
                }

                if (state == ArgumentInsertState.Named)
                {
                    // Note: Just because the call uses named arguments does not mean that the parameters have default values.

                    if (argPicker.FindUnpicked(a => a.ParameterName == p.Name))
                    {
                        var assignment = argPicker.Pick();
                        assignment = AssignmentExpressionCreateCastOrConvertIfNeeded(null, assignment, (TypeReference)p.ParameterType);
                        suggestedAssignmentsOut.Add(assignment);
                        continue;   // next parameter
                    }
                    else
                    {
                        if (p.HasDefaultValue)
                        {
                            object defaultValue = p.DefaultValue;
                            if (defaultValue == null && p.ParameterType.IsValueType)
                            {
                                // Structs just have null as default value in ParameterInfo, so create a usable default value.
                                defaultValue = Activator.CreateInstance(p.ParameterType);
                            }
                            suggestedAssignmentsOut.Add(
                                new SBExpressionData(
                                    Expression.Constant(defaultValue, p.ParameterType)));
                            continue;
                        }
                        else if (procedureParameters != null)
                        {
                            var fp = procedureParameters.Find(a => a.Name == p.Name);
                            if (fp != null)
                            {
                                object c = fp.DefaultValue;
                                if (c == null && p.ParameterType.IsValueType)
                                {
                                    c = Activator.CreateInstance(p.ParameterType);
                                }
                                suggestedAssignmentsOut.Add(
                                    new SBExpressionData(
                                        Expression.Constant(c, p.ParameterType)));
                                continue;
                            }
                        }
                        else if (argPicker.Current.Argument == "out")
                        {
                            suggestedAssignmentsOut.Add(argPicker.Pick());
                            continue;
                        }
                        else
                        {
                            // TODO: Report argument not found, maybe
                            return 0;
                        }
                    }
                }

                if (state == ArgumentInsertState.Params)
                {
                    if (!p.ParameterType.IsArray) throw new Exception("Unexpected type of 'params' parameter; not an array.");
                    var t = p.ParameterType.GetElementType();
                    matchScore -= 5;    // Lower score, just to make methods having direct parameters instead of params array score higher.

                    if (!argPicker.AllBeforeCurrentArePickedAndOthersUnpicked())
                    {
                        // TODO: report error, maybe
                        return 0;
                    }
                    if (argPicker.UnpickedCount == 1 && p.ParameterType.IsAssignableFrom(argPicker.Current.DataType.Type))
                    {
                        suggestedAssignmentsOut.Add(argPicker.Pick());
                        continue;
                    }
                    else
                    {
                        var paramsArgs = new List<Expression>();
                        while (argPicker.UnpickedCount > 0)
                        {
                            if (t == argPicker.Current.DataType.Type)
                            {
                                paramsArgs.Add(argPicker.Pick().ExpressionCode);
                            }
                            else if (t.IsAssignableFrom(argPicker.Current.DataType.Type))
                            {
                                paramsArgs.Add(Expression.Convert(argPicker.Pick().ExpressionCode, t));
                                matchScore -= 5;    // Lower score when not an exact match;
                            }
                            else
                            {
                                // TODO: report error, maybe
                                return 0;
                            }
                        }
                        suggestedAssignmentsOut.Add(new SBExpressionData(Expression.NewArrayInit(t, paramsArgs)));
                    }
                    continue;
                }

                if (state == ArgumentInsertState.NoArguments)
                {
                    if (p.IsDefined(typeof(ParamArrayAttribute)))
                    {
                        if (!p.ParameterType.IsArray) throw new Exception("Unexpected type of 'params' parameter; not an array.");
                        var t = p.ParameterType.GetElementType();
                        suggestedAssignmentsOut.Add(new SBExpressionData(Expression.NewArrayInit(t)));
                        matchScore -= 5;    // In case there's another method with no more parameters, that would be a better choise.
                    }
                    else
                    {
                        if (p.HasDefaultValue)
                        {
                            object c = p.DefaultValue;
                            if (c == null && p.ParameterType.IsValueType)
                            {
                                c = Activator.CreateInstance(p.ParameterType);
                            }
                            suggestedAssignmentsOut.Add(
                                new SBExpressionData(
                                    Expression.Constant(c, p.ParameterType)));
                            continue;
                        }
                        else if (procedureParameters != null)
                        {
                            var fp = procedureParameters.Find(a => a.Name == p.Name);
                            if (fp != null)
                            {
                                object c = fp.DefaultValue;
                                if (c == null && p.ParameterType.IsValueType)
                                {
                                    c = Activator.CreateInstance(p.ParameterType);
                                }
                                suggestedAssignmentsOut.Add(
                                    new SBExpressionData(
                                        Expression.Constant(c, p.ParameterType)));
                                continue;
                            }
                        }
                        else
                        {
                            // TODO: Report argument not found, maybe
                            return 0;
                        }
                    }
                }
            }

            if (suggestedAssignmentsOut.Count >= parameters.Length)
            {
                if (argPicker.PickedCount < arguments.Count)
                {
                    //throw new Exception("Not all passed arguments are used!");   
                    matchScore = 0;     // TODO: report in a better way.
                }
                return matchScore;
            }
            return 0;
        }

        internal bool CreateArgumentsForDynamicCall(
            List<SBExpressionData> arguments,
            ref SBExpressionData sequencialFirstArguments,
            ref SBExpressionData namedArguments,
            ref SBExpressionData sequencialLastArguments)
        {
            // 12, "Jan", false
            // a: 12, b: "Jan", c: false
            // a: 12, b: "Jan", c: false, 3, 5, 7, 9

            List<Expression> first = new List<Expression>();
            List<SBExpressionData> named = new List<SBExpressionData>();
            List<Expression> last = new List<Expression>();
            bool inFirst = true, inLast = false;

            foreach (var arg in arguments)
            {
                if (String.IsNullOrEmpty(arg.ParameterName))
                {
                    if (inFirst)
                    {
                        first.Add(arg.ExpressionCode);
                    }
                    else
                    {
                        inLast = true;
                        last.Add(arg.ExpressionCode);
                    }
                }
                else   // Named argument
                {
                    if (inFirst)
                    {
                        inFirst = false;
                    }
                    else if (inLast)
                    {
                        return false;
                    }
                    named.Add(arg);
                }
            }

            //Additional information: Et udtryk af typen 'System.Object' kan ikke 
            //bruges til initialisering af en matrix af typen 'System.Object[]'
            var firstArgs = first.Select(a => Expression.Convert(a, typeof(object)));
            var lastArgs = last.Select(a => Expression.Convert(a, typeof(object)));
            if (first.Count > 0)
            {
                sequencialFirstArguments = new SBExpressionData(
                    Expression.NewArrayInit(typeof(object), firstArgs));
            }
            else
            {
                sequencialFirstArguments = new SBExpressionData(Expression.Constant(null, typeof(object[])));
            }
            if (last.Count > 0)
            {
                sequencialLastArguments = new SBExpressionData(
                    Expression.NewArrayInit(typeof(object), lastArgs));
            }
            else
            {
                sequencialLastArguments = new SBExpressionData(Expression.Constant(null, typeof(object[])));
            }

            if (named.Count > 0)
            {
                var namedElementCtor = typeof(NamedData<object>).GetConstructor(new Type[] { typeof(string), typeof(object) });
                var argumentlistCtor = typeof(ArgumentList).GetConstructor(new Type[] { typeof(NamedData<object>[]) });
                namedArguments = new SBExpressionData(
                    Expression.New(
                        argumentlistCtor,
                        named.Select(
                            a => Expression.New(
                                namedElementCtor,
                                Expression.Constant(a.ParameterName),
                                a.ExpressionCode)).ToArray()));
            }
            else
            {
                namedArguments = new SBExpressionData(Expression.Constant(null, typeof(ArgumentList)));
            }
            return true;
        }

        #region Keyword Procedure Call

        public override void EnterKeywordProcedureCall([NotNull] SBP.KeywordProcedureCallContext context)
        {
            m_lastElementPropertyBlock = null;
        }

        public override void ExitKeywordProcedureCall([NotNull] SBP.KeywordProcedureCallContext context)
        {
            //var identifier = m_identifier;
            //var keywordArguments = m_keywordArguments;
            //var argumentStack = m_arguments.Pop();
            //var propertyBlock = m_lastElementPropertyBlock;

            //var left = TSExpressionData.CreateIdentifier(String.Join(".", identifier));
            //left = this.ResolveIfIdentifier(left);        // Now done in EnterMethodArguments() above.

            //this.HandleParensExpression(context, true, left, argumentStack, null, propertyBlock);
        }

        public override void EnterProcedureAndArgumentCombinedPhrase([NotNull] SBP.ProcedureAndArgumentCombinedPhraseContext context)
        {
            m_expressionData.PushStackLevel("ProcedureAndArgumentCombinedPhrase");
        }

        public override void ExitProcedureAndArgumentCombinedPhrase([NotNull] SBP.ProcedureAndArgumentCombinedPhraseContext context)
        {
            //m_keywordArguments = m_expressionData.PopStackLevel();
            //int n = context.ChildCount - 1;     // Minus the procedureAndArgumentCombinedPhraseTail 
            //m_identifier.Clear();
            //for (int i = 0; i < n; i += 2)  // Skip every second child (the ".")
            //{
            //    m_identifier.Add(context.GetChild(i).GetText());
            //}
            throw new NotImplementedException();
        }

        public override void ExitProcedureAndArgumentCombinedPhraseTail([NotNull] SBP.ProcedureAndArgumentCombinedPhraseTailContext context)
        {
            base.ExitProcedureAndArgumentCombinedPhraseTail(context);
        }

        #endregion

        private static TypeMatch CheckParameterArgumentTypeMatch(ParameterInfo parameter, SBExpressionData argument)
        {
            if (parameter.ParameterType.IsByRef)
            {
                if (argument.ReferencedType != SBExpressionType.LocalVariableReference) return TypeMatch.UnsupportedSource;
                return (parameter.ParameterType == argument.DataType.Type) ? TypeMatch.ExactMatch : TypeMatch.NoTypeMatch;  // When it's a ByRef, the type must be exactly the same (I think).
            }
            if (argument.DataType.Type == parameter.ParameterType)
            {
                return TypeMatch.ExactMatch;
            }
            else if (argument.IsUnknownIdentifier)
            {
                return TypeMatch.NoTypeMatch;
            }
            else if (typeof(IValueContainer).IsAssignableFrom(argument.DataType.Type) && !typeof(IValueContainer).IsAssignableFrom(parameter.ParameterType))
            {
                if (argument.DataType.Type.IsGenericType &&
                    argument.DataType.Type.GetGenericTypeDefinition() == typeof(IValueContainer<>) &&
                    parameter.ParameterType.IsAssignableFrom(argument.DataType.Type.GenericTypeArguments[0]))
                {
                    return TypeMatch.MatchLevel1;
                }
                else
                {
                    return TypeMatch.NoTypeMatch;
                }
            }
            else if (parameter.ParameterType == typeof(object)) // Not exac match, and parameter is the lowest possible matching level (object).
            {
                return TypeMatch.MatchLowest;
            }
            return parameter.ParameterType.IsAssignableFrom(argument.DataType.Type) ? TypeMatch.MatchLevel1 : TypeMatch.NoTypeMatch;
        }

        private static int GetIndexOfNamedArgument(List<SBExpressionData> arguments, string name)
        {
            int i = 0;
            foreach (var exp in arguments)
            {
                if (exp.ParameterName == name) return i;
                i++;
            }
            return -1;
        }
    }
}
