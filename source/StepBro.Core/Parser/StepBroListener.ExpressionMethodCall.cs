using Antlr4.Runtime.Misc;
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
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private static MethodInfo s_ExecuteDynamicObjectMethod = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ExecuteDynamicObjectMethod));
        private static MethodInfo s_ExecuteDynamicAsyncObjectMethod = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ExecuteDynamicAsyncObjectMethod));

        #region Arguments

        private int m_argumentIndex = 0;   // TODO: add this to the stack somehow.
        private string m_argumentName = null;  // TODO: add this to the stack somehow.
        private SBExpressionData m_leftOfMethodCallExpression = null;   // TODO: add this to the stack somehow.

        public override void EnterArgument([NotNull] SBP.ArgumentContext context)
        {
            m_argumentIndex = m_expressionData.Peek().Count;
            m_argumentName = null;
        }

        public override void ExitArgument([NotNull] SBP.ArgumentContext context)
        {
            m_expressionData.Peek().Peek().ArgumentIndex = m_argumentIndex;
            m_expressionData.Peek().Peek().ParameterName = m_argumentName;
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
            m_argumentName = context.children[0].GetText();
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
            m_arguments.Push(m_expressionData.PopStackLevel());
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
            DynamicObjectMethodCall
        }

        public void HandleParensExpression(
            Antlr4.Runtime.ParserRuleContext context,
            bool isCallStatement,
            SBExpressionData left,
            Stack<SBExpressionData> argumentStack,
            SBExpressionData assignmentTarget,
            PropertyBlock propertyBlock)
        {
            if (left.IsError())
            {
                if (!isCallStatement) 
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

            var matchingMethods = new List<Tuple<MethodInfo, int>>();
            var suggestedAssignmentsForMatchingMethods = new List<List<SBExpressionData>>();
            Expression instance = null;
            string instanceName = left.Instance;
            var callType = ParansExpressionType.MethodCall;
            var firstParIsThis = false; // Whether the first parameter of procedure (or method?) is a 'this' reference.
            TypeReference returnType = null;

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
                    m_errors.SymanticError(context.Start.Line, -1, false, $"\"{left.ToString()}\" should access a ctor or Create-function, but is not implemented.");
                    return;

                case SBExpressionType.Identifier:
                    m_errors.SymanticError(left.Token.Line, left.Token.Column, false, $"\"{left.ToString()}\" is unresolved.");
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
                        var procedure = (left.Value as IProcedureReference).ProcedureData;
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
                    }
                    break;

                //case SBExpressionType.DynamicObjectProperty:
                //case SBExpressionType.DynamicObjectPropertyReadonly:
                //    m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Left side is a dynamic property, not a dynamic procedure.");
                //    return;

                //case SBExpressionType.DynamicObjectProcedure:
                //    {
                //        callType = ParansExpressionType.DynamicObjectMethodCall;
                //    }
                //    break;

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
                                        HomeType.Immediate,
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

                case SBExpressionType.OperationError:
                case SBExpressionType.UnsupportedOperation:
                case SBExpressionType.UnknownIdentifier:
                    return;

                default:
                    m_errors.InternalError(context.Start.Line, -1, $"Unhandled expression type: \"{left.ReferencedType}\".");
                    break;
            }

            #endregion

            MethodInfo selectedMethod = null;
            List<SBExpressionData> methodArguments = null;

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

            if (methods == null)
            {
                throw new Exception("No methods identified; should be handled and reported earlier.");
            }

            #region Find matching method and setup arguments

            foreach (MethodInfo m in methods)
            {
                var constructedMethod = m;
                var extensionInstance = m.IsExtension() ? instance : null;
                List<SBExpressionData> suggestedAssignments = new List<SBExpressionData>();
                int score = this.CheckMethodArguments(
                    ref constructedMethod,
                    callType == ParansExpressionType.ProcedureCall || callType == ParansExpressionType.FunctionCall,
                    firstParIsThis,
                    instance, 
                    instanceName,
                    m_currentProcedure?.ContextReferenceInternal,
                    extensionInstance,
                    arguments,
                    suggestedAssignments);
                if (score > 0)
                {
                    matchingMethods.Add(new Tuple<MethodInfo, int>(constructedMethod, score));
                    suggestedAssignmentsForMatchingMethods.Add(suggestedAssignments);
                }
            }

            int bestMatch = -1;
            if (matchingMethods.Count == 1)
            {
                bestMatch = 0;
            }
            else if (matchingMethods.Count > 1)
            {
                int max = matchingMethods[0].Item2;
                int maxAt = 0;
                int numAtMax = 1;
                for (var i = 1; i < matchingMethods.Count; i++)
                {
                    if (matchingMethods[i].Item2 > max)
                    {
                        max = matchingMethods[i].Item2;
                        maxAt = i;
                        numAtMax = 1;
                    }
                    else if (matchingMethods[i].Item2 == max)
                    {
                        numAtMax++;
                    }
                }
                if (numAtMax == 1)
                {
                    bestMatch = maxAt;
                }
            }

            if (bestMatch >= 0)
            {
                selectedMethod = matchingMethods[bestMatch].Item1;
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
                // Handle none or more than one alternative
                throw new NotImplementedException();
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
                            var procedure = procRef.ProcedureData as FileProcedure;
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
                                typeof(Exception),
                                Expression.Rethrow())
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
                            HomeType.Immediate,
                            SBExpressionType.Expression,
                            returnType,
                            Expression.Call(
                                theDelegate,
                                delegateType.GetMethod("Invoke"),
                                methodArguments.Select(a => a.ExpressionCode).ToArray()),
                            null));
                }
            }
            else    // Not a procedure or function; a method.
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
                                    HomeType.Immediate,
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
                                        HomeType.Immediate,
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

            #endregion
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
            ArgumentList,
            Params,
            NoArguments
        }

        internal int CheckMethodArguments(
            ref MethodInfo method,
            bool isProcedure, bool firstParIsThis,
            Expression instance, string instanceName,
            Expression contextReference,
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

            var parameters = method.GetParameters();

            // Run through all the parameters; all must be assigned.
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (state == ArgumentInsertState.ExtensionInstance)
                {
                    suggestedAssignmentsOut.Add(new SBExpressionData(extensionInstance));
                    state = ArgumentInsertState.Initial;
                    continue;
                }
                else if (state == ArgumentInsertState.ProcedureContext)
                {
                    suggestedAssignmentsOut.Add(new SBExpressionData(contextReference));
                    state = ArgumentInsertState.Initial;
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
                                string prefix = method.Name;
                                if (!String.IsNullOrEmpty(instanceName))
                                {
                                    prefix = instanceName + "." + prefix;
                                }
                                var contextCreator = Expression.Call(
                                    s_CreateMethodCallContext,
                                    contextReference,
                                    Expression.Constant(prefix));
                                suggestedAssignmentsOut.Add(new SBExpressionData(contextCreator));

                                //suggestedAssignmentsOut.Add(new SBExpressionData(m_currentProcedure.ContextReference));
                            }
                            continue;
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

                if (state == ArgumentInsertState.Mandatory)
                {
                    if (p.GetType() == typeof(ArgumentList))
                    {
                        state = ArgumentInsertState.ArgumentList;
                    }
                    else
                    {
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
                        else if (IsParameterAssignableFromArgument(p, argPicker.Current))
                        {
                            var a = argPicker.Pick();
                            if (p.ParameterType == typeof(object))
                            {
                                a = a.NewExpressionCode(Expression.Convert(a.ExpressionCode, typeof(object)));
                                matchScore -= 10;    // Matching an object parameter is not as good as matching the exact same type.
                            }
                            suggestedAssignmentsOut.Add(a);
                            continue;   // next parameter
                        }
                        else if (argPicker.Current.DataType.Type == typeof(Int64) && p.ParameterType.IsPrimitiveNarrowableIntType())
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Convert(argPicker.Pick().ExpressionCode, p.ParameterType)));
                            continue;   // next parameter
                        }
                        else if (argPicker.Current.DataType.Type == typeof(Double) && p.ParameterType == typeof(Single))
                        {
                            suggestedAssignmentsOut.Add(new SBExpressionData(Expression.Convert(argPicker.Pick().ExpressionCode, p.ParameterType)));
                            matchScore -= 5;    // Matching a 'single' is not as good as matching the exact same type.
                            continue;   // next parameter
                        }
                        else
                        {
                            state = ArgumentInsertState.Named;  // Try finding the argument by name, then.
                        }
                    }
                }

                if (state == ArgumentInsertState.ThisReference)
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

                if (state == ArgumentInsertState.Named)
                {
                    // Note: Just because the call uses named arguments does not mean that the parameters have default values.

                    if (argPicker.FindUnpicked(a => a.ParameterName == p.Name))
                    {
                        suggestedAssignmentsOut.Add(argPicker.Pick());
                        continue;   // next parameter
                    }
                    else
                    {
                        if (p.HasDefaultValue)
                        {
                            suggestedAssignmentsOut.Add(
                                new SBExpressionData(
                                    Expression.Constant(p.DefaultValue, p.ParameterType)));
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

        private static bool IsParameterAssignableFromArgument(ParameterInfo parameter, SBExpressionData argument)
        {
            if (parameter.ParameterType.IsByRef)
            {
                if (argument.ReferencedType != SBExpressionType.LocalVariableReference) return false;   // TODO: report reason for rejection.
                return (parameter.ParameterType == argument.DataType.Type);  // Whtn ByRef type must be exactly the same (I think).
            }
            return parameter.ParameterType.IsAssignableFrom(argument.DataType.Type);
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
