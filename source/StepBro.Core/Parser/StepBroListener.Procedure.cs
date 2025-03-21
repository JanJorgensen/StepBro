﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static StepBro.Core.Parser.Grammar.StepBro;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private static MethodInfo s_ExpectStatement = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ExpectStatement));
        private static MethodInfo s_CreateMethodCallContext = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.CreateMethodCallContext));
        private static MethodInfo s_PostExpressionStatement = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.PostExpressionStatement));

        private TypeReference m_procedureReturnType = null;
        private bool m_procedureIsFunction = false;
        private List<ParameterData> m_parameters = null;
        private Stack<FileProcedure> m_procedureStack = new Stack<FileProcedure>();
        internal FileProcedure m_currentProcedure = null;   // The procedure currently being parsed.
        private bool m_inFunctionScope = false;
        private FileProcedure m_lastProcedure = null;       // The last procedure the parser ended parsing.
        private ProcedureParsingScope m_procedureBaseScope = null;
        private Stack<ProcedureParsingScope> m_scopeStack = new Stack<ProcedureParsingScope>();
        private bool m_enteredLoopStatement = false;
        private string m_modifiers = null;
        private bool m_isSimpleExpectWithValue = false;

        public IFileProcedure LastParsedProcedure { get { return m_lastProcedure; } }

        private SBExpressionData m_callAssignmentTarget = null;
        private bool m_callAssignmentAwait = false;
        private Stack<Stack<SBExpressionData>> m_arguments = new Stack<Stack<SBExpressionData>>();
        private Stack<Stack<SBExpressionData>> m_statementExpressions = new Stack<Stack<SBExpressionData>>();
        private Stack<List<Expression>> m_forInitVariables = new Stack<List<Expression>>();
        private Stack<SBExpressionData> m_forCondition = new Stack<SBExpressionData>();
        //private Stack<TSExpressionData> m_keywordArguments = null;

        public Stack<SBExpressionData> GetArguments()
        {
            return m_arguments.Pop();
        }

        private void EnterProcedureParsing(string name)
        {
            if (m_file != null && m_file.TypeScanIncluded)
            {
                var proc = m_file.ListElements().FirstOrDefault(e => e.ElementType == FileElementType.ProcedureDeclaration && e.Name == name);
                if (proc != null)
                {
                    m_currentProcedure = proc as FileProcedure;
                    m_currentFileElement = proc as FileElement;
                    if (((m_currentProcedure.Flags & ProcedureFlags.IsFunction) == ProcedureFlags.IsFunction) != m_procedureIsFunction) throw new Exception("Procedure from type scanning is different type (procedure/function) than in the current parsing.");
                }
                else
                {
                    throw new Exception("");    // TODO: convert to parsing error (internal error).
                }
            }
            else
            {
                m_currentProcedure = new FileProcedure(m_file, m_fileElementModifier, m_elementStart.Line, null, m_currentNamespace, name);
            }
            m_currentProcedure.ReturnType = m_procedureReturnType;
            m_currentProcedure.Flags = (m_currentProcedure.Flags & ~ProcedureFlags.IsFunction) | (m_procedureIsFunction ? ProcedureFlags.IsFunction : ProcedureFlags.None);

            m_currentFileElement = m_currentProcedure;
            m_lastProcedure = m_currentProcedure;
            m_procedureStack.Push(m_currentProcedure);
        }

        private void ExitProcedureParsing()
        {
            m_currentFileElement = m_currentProcedure;
            m_lastProcedure = m_currentProcedure;
            if (m_file != null)
            {
                if (!m_file.TypeScanIncluded)
                {
                    m_file.AddElement(m_currentProcedure);
                }
                m_currentProcedure.Compile();   // TODO: Maybe not called here ( and runtime code needs to be added).
            }
            m_procedureStack.Pop(); // Pop current. New current is the at the top.
            m_currentProcedure = (m_procedureStack.Count > 0) ? m_procedureStack.Peek() : null;
        }

        private string GetNodeText(IParseTree node)
        {
            if (node.ChildCount == 0) { return node.GetText(); }
            if (node.ChildCount == 1) { return GetNodeText(node.GetChild(0)); }
            else
            {
                var left = node.GetChild(0);
                StringBuilder sb = new StringBuilder();
                sb.Append(GetNodeText(left));
                for (int i = 1; i < node.ChildCount; i++)
                {
                    var n = node.GetChild(i);
                    if (left.SourceInterval.a < n.SourceInterval.b)
                    {
                        sb.Append(new string(' ', n.SourceInterval.b - left.SourceInterval.a - 1));
                    }
                    sb.Append(GetNodeText(n));
                    left = n;
                }
                return sb.ToString();
            }
        }


        public override void EnterProcedureDeclaration([NotNull] SBP.ProcedureDeclarationContext context)
        {
            //this.EnterProcedureParsing();
        }

        public override void ExitProcedureDeclaration([NotNull] SBP.ProcedureDeclarationContext context)
        {
            this.ExitProcedureParsing();
        }

        public override void EnterFileElementProcedure([NotNull] SBP.FileElementProcedureContext context)
        {
            m_procedureIsFunction = false;  // In case no type scanning hsa beed performed.
        }

        public override void EnterFileElementFunction([NotNull] SBP.FileElementFunctionContext context)
        {
            //if (m_file.TypeScanIncluded && m_currentProcedure.IsFunction == true) throw new Exception("Procedure from type scanning is set to be a \"function\" type.");
            m_procedureIsFunction = true;  // In case no type scanning hsa beed performed.
        }

        public override void ExitFileElementProcedure([NotNull] SBP.FileElementProcedureContext context)
        {
            this.ExitProcedureParsing();
        }

        public override void ExitFileElementFunction([NotNull] SBP.FileElementFunctionContext context)
        {
            this.ExitProcedureParsing();
        }

        public override void EnterProcedureReturnType([NotNull] SBP.ProcedureReturnTypeContext context)
        {
            m_expressionData.PushStackLevel("ReturnType");
        }

        public override void ExitProcedureReturnType([NotNull] SBP.ProcedureReturnTypeContext context)
        {
            m_procedureReturnType = m_typeStack.Pop();  // Must be past the return type. Save here to make it available to the parsing of the body.
            m_expressionData.PopStackLevel();
        }

        public override void ExitProcedureName([NotNull] SBP.ProcedureNameContext context)
        {
            m_elementStart = context.Start;
            this.EnterProcedureParsing(context.GetText());
        }

        public override void EnterFormalParameters([NotNull] SBP.FormalParametersContext context)
        {
            m_parameters = new List<ParameterData>();     // If used from the formalParameters rule (could be empty).
        }
        public override void EnterFormalParameterDecls([NotNull] SBP.FormalParameterDeclsContext context)
        {
            m_parameters = new List<ParameterData>();     // If not used from the formalParameters rule.
        }
        public override void EnterFormalParameterDecl([NotNull] SBP.FormalParameterDeclContext context)
        {
            m_expressionData.PushStackLevel("Parameter");
            //m_parameters = new List<Tuple<Type, string>>();     // If not used from the formalParameters rule.
        }
        public override void ExitFormalParameterModifiers([NotNull] SBP.FormalParameterModifiersContext context)
        {
            m_modifiers = context.GetText();
        }
        public override void ExitFormalParameterDecl([NotNull] SBP.FormalParameterDeclContext context)
        {
        }

        public override void ExitFormalParameterDeclStart([NotNull] SBP.FormalParameterDeclStartContext context)
        {
            string name = context.GetChild(context.ChildCount - 1).GetText();
            TypeReference type = m_typeStack.Pop();
            var modifiers = (m_modifiers != null) ? m_modifiers.Split(' ') : new string[] { };
            System.Diagnostics.Debug.Assert(type != null);
            var typeToken = ((ParserRuleContext)context.children[context.ChildCount - 2]).Start;
            m_parameters.Add(new ParameterData(modifiers, name, type.Type.Name, type, typeToken));
            m_expressionData.PopStackLevel();
        }

        public override void EnterFormalParameterAssignment([NotNull] SBP.FormalParameterAssignmentContext context)
        {
            m_expressionData.PushStackLevel("FormalParameterAssignment @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
        }

        public override void ExitFormalParameterAssignment([NotNull] SBP.FormalParameterAssignmentContext context)
        {
            var levelData = m_expressionData.PopStackLevel();
            var parInitializer = this.ResolveForGetOperation(levelData.Stack.Pop());
            m_parameters[m_parameters.Count - 1].SetDefaultValue(parInitializer.Value, ((ParserRuleContext)context.children[0]).Start);
            //if (m_variableInitializer.IsError())
            //{
            //    m_errors.UnresolvedIdentifier(m_variableInitializer.Token.Line, m_variableInitializer.Token.Column, m_variableInitializer.Value as string);
            //}
        }

        public override void ExitProcedureParameters([NotNull] SBP.ProcedureParametersContext context)
        {
            if (!m_file.TypeScanIncluded)
            {
                foreach (var p in m_parameters)
                {
                    m_currentProcedure.AddParameter(p);
                }
            }
            else
            {
                // Transfer the parameter default values to the existing parameters.
                var procParams = m_currentProcedure.GetFormalParameters();
                for (int i = 0; i < m_parameters.Count; i++)
                {
                    if (m_parameters[i].HasDefaultValue)
                    {
                        procParams[i].SetDefaultValue(m_parameters[i].DefaultValue, m_parameters[i].DefaultValueToken);
                    }
                }
            }
        }

        public override void EnterProcedureBodyOrNothing([NotNull] SBP.ProcedureBodyOrNothingContext context)
        {
            if (!m_file.TypeScanIncluded)
            {
                m_currentProcedure.HasBody = context.Start.Type != SBP.SEMICOLON;
                m_currentProcedure.CreateDelegateType();
            }
        }

        public override void EnterProcedureBody([NotNull] SBP.ProcedureBodyContext context)
        {
            m_scopeStack.Clear();
            m_procedureBaseScope = null;
            m_inFunctionScope = true;
        }

        public override void ExitProcedureBody([NotNull] SBP.ProcedureBodyContext context)
        {
            m_inFunctionScope = false;
            m_currentProcedure.SetProcedureBody(m_procedureBaseScope.GetBlockCode());
        }

        public override void EnterBlock([NotNull] SBP.BlockContext context)
        {
            if (m_scopeStack.Count > 0)
            {
                m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "Block", ProcedureParsingScope.ScopeType.Block));
            }
            else
            {
                m_procedureBaseScope = new ProcedureParsingScope(null, "Procedure", ProcedureParsingScope.ScopeType.Procedure);
                m_scopeStack.Push(m_procedureBaseScope);
            }
            m_expressionData.PushStackLevel("Block");   // "Livrem og seler"
        }

        public override void ExitBlock([NotNull] SBP.BlockContext context)
        {
            m_expressionData.PopStackLevel();   // Just remove the level.

            var block = m_scopeStack.Pop();
            if (block.Type != ProcedureParsingScope.ScopeType.Procedure)
            {
                if (m_scopeStack.Peek().Type == ProcedureParsingScope.ScopeType.Block)
                {
                    m_scopeStack.Peek().AddStatementCode(block.GetBlockCode());
                }
                else
                {
                    m_scopeStack.Peek().AddSubStatement(block);
                }
            }
            else
            {
                // End of procedure body. Handled in ExitProcedureBody().
            }
        }

        public override void EnterSubStatement([NotNull] SBP.SubStatementContext context)
        {
            if (m_enteredLoopStatement)
            {
                try
                {
                    m_scopeStack.Peek().SetupForLoop();
                }
                catch (Exception e)
                {
                    m_errors.InternalError(context.Start.Line, context.Start.Column, e.Message);
                }
                m_enteredLoopStatement = false;
            }
            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "sub", ProcedureParsingScope.ScopeType.SubStatement));
        }

        public override void ExitSubStatement([NotNull] SBP.SubStatementContext context)
        {
            var sub = m_scopeStack.Pop();
            if (sub.StatementCount == 1)   // Just a single statement (not block)
            {
                // Add the sub-statement scope (containing the single statement)
                m_scopeStack.Peek().AddSubStatement(sub);
            }
            else if (sub.StatementCount == 0)   // Must be a block statement then.
            {
                // Add the block scope saved as a sub-expression in the sub-statement scope.
                m_scopeStack.Peek().AddSubStatement(sub.GetSubStatements()[0]);
            }
        }

        private Expression CreateEnterStatement(int line, int column)
        {
            return Expression.Call(
                    m_currentProcedure.ContextReferenceInternal,
                    typeof(IScriptCallContext).GetMethod("EnterStatement"),
                    Expression.Constant(line),
                    Expression.Constant(column));
        }

        private void AddEnterStatement(SBP.StatementContext context, string entryTimeVariable = null)
        {
            if (m_scopeStack.Peek().Type <= ProcedureParsingScope.ScopeType.Block)
            {
                m_scopeStack.Peek().AddStatementCode(this.CreateEnterStatement(context.Start.Line, context.Start.Column));
            }
        }

        public override void EnterBlockStatement([NotNull] SBP.BlockStatementContext context)
        {
            m_scopeStack.Peek().SetAttributes();
            m_enteredLoopStatement = false;
        }

        public override void ExitBlockStatement([NotNull] SBP.BlockStatementContext context)
        {
        }

        public override void ExitBlockStatementAttributes([NotNull] SBP.BlockStatementAttributesContext context)
        {
            m_scopeStack.Peek().SetAttributes(m_lastAttributes);
            m_lastAttributes = null;
        }

        #region Statements

        #region Normal Procedure Call

        public override void EnterCallStatement([NotNull] SBP.CallStatementContext context)
        {
            this.AddEnterStatement(context);
            m_callAssignmentTarget = null;
            m_callAssignmentAwait = false;
        }

        public override void ExitCallStatement([NotNull] SBP.CallStatementContext context)
        {
            var left = m_expressionData.Pop();
            var argumentStack = m_arguments.Pop();
            var propertyBlock = m_lastElementPropertyBlock;

            if (!CheckArgumentExpressionsForErrors(context, argumentStack.ToArray()))
            {
                return;
            }

            left = this.ResolveIfIdentifier(left, true);        // Now done in EnterMethodArguments() above.

            this.HandleParensExpression(context, true, left, argumentStack, null, propertyBlock);
        }

        public override void ExitCallAssignment([NotNull] SBP.CallAssignmentContext context)
        {
            if (context.ChildCount == 1)
            {
                if (context.Start.Type == SBP.AWAIT)
                {
                    m_callAssignmentAwait = true;
                    return;
                }
                else if (context.Start.Type == SBP.START)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (context.ChildCount == 3)
            {
                var child = context.GetChild(2) as Antlr4.Runtime.Tree.TerminalNodeImpl;
                if (child != null && child.Payload.Type == SBP.AWAIT)
                {
                    m_callAssignmentAwait = true;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            m_callAssignmentTarget = m_expressionData.Pop();
            m_callAssignmentTarget = this.ResolveIfIdentifier(m_callAssignmentTarget, true);
        }

        #endregion

        public override void EnterIfStatement([NotNull] SBP.IfStatementContext context)
        {
            this.AddEnterStatement(context);
            m_expressionData.PushStackLevel("IfStatement");
        }

        public override void ExitIfStatement([NotNull] SBP.IfStatementContext context)
        {
            var levelData = m_expressionData.PopStackLevel();
            var condition = levelData.Stack.Pop();
            var subStatements = m_scopeStack.Peek().GetSubStatements();
            var attributes = m_scopeStack.Peek().GetAttributes();

            condition = this.ResolveForGetOperation(condition, TypeReference.TypeBool);

            if (!CheckExpressionsForErrors(context, condition))
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                return;
            }

            Expression trueStatement;
            if (subStatements[0].Type == ProcedureParsingScope.ScopeType.Block)
            {
                trueStatement = subStatements[0].GetBlockCode();   // TODO: Add some logging and stuff
            }
            else
            {
                trueStatement = subStatements[0].GetOnlyStatementCode();
            }

            if (trueStatement == null)
            {
                trueStatement = Expression.Empty();
            }

            Expression falseStatement = null;
            if (subStatements.Count == 2)
            {
                if (subStatements[1].Type == ProcedureParsingScope.ScopeType.Block)
                {
                    falseStatement = subStatements[1].GetBlockCode();   // TODO: Add some logging and stuff
                }
                else
                {
                    falseStatement = subStatements[1].GetOnlyStatementCode();
                }
            }

            if (falseStatement == null)
            {
                falseStatement = Expression.Empty();
            }

            if (condition.IsValueType && condition.DataType.Type == typeof(bool))
            {
                switch (subStatements.Count)
                {
                    case 1:     // Without 'else'
                        m_scopeStack.Peek().AddStatementCode(Expression.IfThen(condition.ExpressionCode, trueStatement));
                        break;
                    case 2:     // With 'else'
                        m_scopeStack.Peek().AddStatementCode(Expression.IfThenElse(condition.ExpressionCode, trueStatement, falseStatement));
                        break;
                    default:
                        throw new NotImplementedException("This should never happen !!!");
                }
            }
            else
            {
                throw new NotImplementedException("Something wrong with the condition expression.");
            }
        }

        #region Looping

        public override void EnterForStatement([NotNull] SBP.ForStatementContext context)
        {
            this.AddEnterStatement(context);
            m_enteredLoopStatement = true;
            m_expressionData.PushStackLevel("for-init");
            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "for-init", ProcedureParsingScope.ScopeType.Block));
            m_forInitVariables.Push(new List<Expression>());
        }

        public override void ExitForVariableDeclaration([NotNull] SBP.ForVariableDeclarationContext context)
        {
            foreach (var variable in m_variables)
            {
                TypeReference type = m_variableType;
                if (type.Type == typeof(VarSpecifiedType))
                {
                    if (variable.Initializer.IsError())
                    {
                        return;     // Just leave; no point in spending more time on this variable.
                    }
                    else if (variable.Initializer.DataType != null)
                    {
                        type = variable.Initializer.DataType;
                    }
                    else if (variable.Initializer.ExpressionCode != null)
                    {
                        type = new TypeReference(variable.Initializer.ExpressionCode.Type);
                    }
                    else
                    {
                        m_errors.SymanticError(variable.Initializer.Token.Line, variable.Initializer.Token.Column, false, "Unknown value type for assignment to variable with type 'var'.");
                        break;
                    }
                }

                var scope = m_scopeStack.Peek();
                var v = scope.AddVariable(variable.Name, type, null, EntryModifiers.Private, m_file.FilePath, context.Start.Line);
                // Needs to be a stack for each scope so we don't initialize variables in incorrect scopes
                m_forInitVariables.Peek().Add(Expression.Assign(v.VariableExpression, variable.Initializer.ExpressionCode));
            }
        }

        public override void ExitForCondition([NotNull] SBP.ForConditionContext context)
        {
            // As for-loops can have multiple expressions within them, we ensure we choose the right expression
            // by popping when we exit the "ForCondition" part of the for-loop, meaning the second part of the
            // three-part initialization of the for-loop.
            // We use a stack as multiple for loops can be within each other.
            m_forCondition.Push(m_expressionData.Peek().Stack.Pop());

            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "for-loop", ProcedureParsingScope.ScopeType.Block));
            m_scopeStack.Peek().ForConditionExists = true;
        }

        public override void EnterForUpdate([NotNull] SBP.ForUpdateContext context)
        {
            m_expressionData.PushStackLevel("for-update");

            // If no conditions are present, we create the scopestack here
            if (!m_scopeStack.Peek().ForConditionExists)
            {
                m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "for-loop", ProcedureParsingScope.ScopeType.Block));
            }

            m_scopeStack.Peek().ForUpdateExists = true;
        }

        public override void ExitForControl([NotNull] ForControlContext context)
        {
            // If no conditions and no updates are present, we create the scopestack here
            if (!m_scopeStack.Peek().ForConditionExists && !m_scopeStack.Peek().ForUpdateExists)
            {
                m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "for-loop", ProcedureParsingScope.ScopeType.Block));
            }
        }

        public override void ExitForStatement([NotNull] SBP.ForStatementContext context)
        {
            var forLoopScope = m_scopeStack.Pop();
            var forOuterScope = m_scopeStack.Pop();

            var forInitVariables = m_forInitVariables.Pop();

            // Contains the expressions in the for-update part of the for-loop
            var forUpdateExpressions = (forLoopScope.ForUpdateExists ? m_expressionData.PopStackLevel().Stack : new());
            var forInitExpressions = m_expressionData.PopStackLevel().Stack;

            // Contains the part of the for-loop that contains the condition
            var condition = (forLoopScope.ForConditionExists ? m_forCondition.Pop() : new SBExpressionData(Expression.Constant(true)));

            var subStatements = forLoopScope.GetSubStatements();
            var attributes = forLoopScope.GetAttributes();

            ProcedureVariable varLoopIndex = null;
            ProcedureVariable varEntryTime = null;
            ProcedureVariable varTimeoutTime = null;
            Expression timeout = null;

            var props = forLoopScope.GetProperties();

            m_lastElementPropertyBlock = null;

            condition = this.ResolveForGetOperation(condition, TypeReference.TypeBool);
            if (condition.IsError())
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                return;
            }

            var conditionExpression = condition.ExpressionCode;

            if (condition.IsValueType && condition.DataType.Type != typeof(bool))
            {
                m_errors.SymanticError(condition.Token.Line, condition.Token.Column, false, "Something wrong with the condition expression.");
                return;
            }

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            var isBlockSub = (subStatements[0].Type == ProcedureParsingScope.ScopeType.Block);
            if (isBlockSub)
            {
                breakLabel = forLoopScope.BreakLabel;
                continueLabel = forLoopScope.ContinueLabel;
            }

            var statementExpressions = new List<Expression>();
            var loopExpressions = new List<Expression>
            {
                Expression.IfThen(
                    Expression.Not(conditionExpression),
                    Expression.Block(
                        Expression.Call(
                            m_currentProcedure.ContextReferenceInternal,
                            typeof(IScriptCallContext).GetMethod("SetLoopExitReason", new Type[] { typeof(string) }),
                            Expression.Constant("Expression")),
                        Expression.Break(breakLabel)))
            };

            varLoopIndex = forOuterScope.AddVariable(
                CreateStatementVariableName(context, "forLoop_index"),
                TypeReference.TypeInt64,
                new SBExpressionData(Expression.Constant(0L)),
                EntryModifiers.Private,
                null, -1);
            loopExpressions.Add(Expression.Increment(varLoopIndex.VariableExpression));     // index++; therefore index = 1 inside and after first iteration.

            // TODO: Add some logging, interactive break check, timeout and other stuff
            // TODO: Combine this with the same in the while loop
            #region Attribute Handling

            if ((m_currentProcedure.Flags & ProcedureFlags.IsFunction) == ProcedureFlags.None && props != null)
            {
                foreach (var property in props)
                {
                    if (property.Is("Timeout", PropertyBlockEntryType.Value))
                    {
                        object toValue = (((PropertyBlockValue)property).Value);
                        if (toValue is TimeSpan)
                        {
                            timeout = Expression.Constant((TimeSpan)toValue);
                        }
                        else if (((PropertyBlockValue)property).IsStringOrIdentifier)
                        {
                            var s = ((PropertyBlockValue)property).ValueAsString();
                            if (s.IsIdentifier())
                            {
                                var timeoutExpression = ResolveIdentifierForGetOperation(s, true, TypeReference.TypeTimeSpan);
                                if (timeoutExpression.IsError())
                                {
                                    m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                                    return;
                                }
                                timeout = timeoutExpression.ExpressionCode;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            throw new NotImplementedException("Timeout data type or expression not implemented or supported.");
                        }

                        varTimeoutTime = m_scopeStack.Peek().AddVariable(
                            CreateStatementVariableName(context, "forLoop_TimeoutTime"),
                            TypeReference.TypeDateTime,
                            new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                            EntryModifiers.Private,
                            null, -1);
                    }
                    else if (property.Is("Stoppable", PropertyBlockEntryType.Flag))
                    {
                        loopExpressions.Add(
                            Expression.IfThen(
                                Expression.Call(
                                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                                    typeof(ICallContext).GetMethod(nameof(ICallContext.StopRequested), new Type[] { })),
                                Expression.Block(
                                    Expression.Call(
                                        m_currentProcedure.ContextReferenceInternal,
                                        typeof(IScriptCallContext).GetMethod("Log", new Type[] { typeof(string) }),
                                        Expression.Constant("Loop stopped by user!")),
                                    Expression.Break(breakLabel))));
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Unknown property: \"");
                        sb.Append(property.Name);
                        sb.Append("\" on loop.");
                        // TODO: Figure out a way to give a proper error message for Timeout, as we would hit Timeout here
                        //       even if we write Timeout correctly but forget to give it a value
                        switch (property.Name.ToLower())
                        {
                            case "stoppable":
                                sb.Append(" Did you mean \"Stoppable\"?");
                                break;
                            default:
                                sb.Append(" Check spelling or parameters.");
                                break;
                        }
                        sb.Append(" Keep in mind properties are case-sensitive.");
                        m_errors.SymanticError(property.Line, -1, false, sb.ToString());
                    }
                }
            }
            #endregion


            varEntryTime = forOuterScope.AddVariable(
                            CreateStatementVariableName(context, "forLoop_EntryTime"),
                            TypeReference.TypeDateTime,
                            new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                            EntryModifiers.Private,
                            null, -1);

            var loggingEnabled = Expression.Property(
                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                    typeof(ICallContext).GetProperty("LoggingEnabled"));

            var timeoutLoggingCall = Expression.Call(
                m_currentProcedure.ContextReferenceInternal,
                typeof(IScriptCallContext).GetMethod("Log", new Type[] { typeof(string) }),
                Expression.Constant("Loop timeout!"));

            List<Expression> timeAssignments = new List<Expression>();
            timeAssignments.Add(
                Expression.Assign(varEntryTime.VariableExpression, Expression.Property(null, typeof(DateTime).GetProperty("Now"))));

            if (varTimeoutTime != null)
            {
                timeAssignments.Add(
                    Expression.Assign(varTimeoutTime.VariableExpression, Expression.Add(varEntryTime.VariableExpression, timeout)));
                loopExpressions.Add(
                    Expression.IfThen(
                        Expression.Not(conditionExpression),
                        Expression.Break(breakLabel)));
                loopExpressions.Add(
                    Expression.IfThen(
                        Expression.GreaterThan(
                            Expression.Property(null, typeof(DateTime).GetProperty("Now")),
                            varTimeoutTime.VariableExpression),
                        Expression.Block(
                            Expression.IfThen(loggingEnabled, timeoutLoggingCall),
                            Expression.Call(
                                m_currentProcedure.ContextReferenceInternal,
                                typeof(IScriptCallContext).GetMethod("SetLoopExitReason", new Type[] { typeof(string) }),
                                Expression.Constant("Timeout")),
                            Expression.Break(breakLabel))));
            }

            statementExpressions.AddRange(forInitVariables);

            foreach (var expression in forInitExpressions)
            {
                statementExpressions.Add(expression.ExpressionCode);
            }

            foreach (var assignment in timeAssignments)
            {
                statementExpressions.Add(assignment);
            }

            if (isBlockSub) // 0-N statements with {} around
            {
                var subStatementBlockCode = subStatements[0].GetBlockCode();
                if (subStatementBlockCode != null)
                {
                    loopExpressions.Add(subStatementBlockCode);
                }
            }
            else // Only a single statement without {} around
            {
                loopExpressions.Add(subStatements[0].GetOnlyStatementCode());
            }

            // Add the continue label we jump to in case of a "continue" statement
            // right before the for update expressions, so the for loop still updates
            // when we write continue;
            loopExpressions.Add(Expression.Label(continueLabel));
            foreach (var expression in forUpdateExpressions)
            {
                loopExpressions.Add(expression.ExpressionCode);
            }
            statementExpressions.Add(
                Expression.Loop(
                    Expression.Block(loopExpressions),
                    breakLabel));

            List<ProcedureVariable> forVariables = forOuterScope.GetVariables();
            List<Expression> forLoopExpression = new List<Expression>();
            forLoopExpression.Add(Expression.Block(forVariables.Select(v => ((ParameterExpression)v.VariableExpression)), statementExpressions.ToArray()));

            m_scopeStack.Peek().AddStatementCode(forLoopExpression.ToArray());
        }

        public override void EnterWhileStatement([NotNull] SBP.WhileStatementContext context)
        {
            this.AddEnterStatement(context);
            m_expressionData.PushStackLevel("WhileStatement");
            m_enteredLoopStatement = true;
            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "while", ProcedureParsingScope.ScopeType.Block));
        }

        public override void ExitWhileStatement([NotNull] SBP.WhileStatementContext context)
        {
            var whileScope = m_scopeStack.Pop();
            var levalData = m_expressionData.PopStackLevel();
            var condition = levalData.Stack.Pop();
            var subStatements = whileScope.GetSubStatements();
            var attributes = whileScope.GetAttributes();
            ProcedureVariable varLoopIndex = null;
            ProcedureVariable varEntryTime = null;
            ProcedureVariable varTimeoutTime = null;
            Expression timeout = null;

            var props = whileScope.GetProperties();

            condition = this.ResolveForGetOperation(condition, TypeReference.TypeBool);
            if (condition.IsError())
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                return;
            }

            var conditionExpression = condition.ExpressionCode;

            if (condition.IsValueType && condition.DataType.Type != typeof(bool))
            {
                m_errors.SymanticError(condition.Token.Line, condition.Token.Column, false, "Something wrong with the condition expression.");
                return;
            }

            var breakLabel = Expression.Label();
            var continueLabel = Expression.Label();

            var isBlockSub = (subStatements[0].Type == ProcedureParsingScope.ScopeType.Block);
            if (isBlockSub)
            {
                breakLabel = whileScope.BreakLabel;
                continueLabel = whileScope.ContinueLabel;
            }

            var statementExpressions = new List<Expression>();
            var loopExpressions = new List<Expression>
            {
                this.CreateEnterStatement(context.Start.Line, context.Start.Column),
                Expression.IfThen(
                    Expression.Not(conditionExpression),
                    Expression.Block(
                        Expression.Call(
                            m_currentProcedure.ContextReferenceInternal,
                            typeof(IScriptCallContext).GetMethod("SetLoopExitReason", new Type[] { typeof(string) }),
                            Expression.Constant("Expression")),
                        Expression.Break(breakLabel)))
            };

            varLoopIndex = whileScope.AddVariable(
                CreateStatementVariableName(context, "whileLoop_index"),
                TypeReference.TypeInt64,
                new SBExpressionData(Expression.Constant(0L)),
                EntryModifiers.Private,
                null, -1);
            loopExpressions.Add(Expression.Increment(varLoopIndex.VariableExpression));     // index++; therefore index = 1 inside and after first iteration.

            // TODO: Add some logging, interactive break check, timeout and other stuff
            // TODO: Combine this with the same in the for loop
            #region Attribute Handling

            if ((m_currentProcedure.Flags & ProcedureFlags.IsFunction) == ProcedureFlags.None && props != null)
            {
                foreach (var property in props)
                {
                    if (property.Is("Timeout", PropertyBlockEntryType.Value))
                    {
                        object toValue = (((PropertyBlockValue)property).Value);
                        if (toValue is TimeSpan)
                        {
                            timeout = Expression.Constant((TimeSpan)toValue);
                        }
                        else if (((PropertyBlockValue)property).IsStringOrIdentifier)
                        {
                            var s = ((PropertyBlockValue)property).ValueAsString();
                            if (s.IsIdentifier())
                            {
                                var timeoutExpression = ResolveIdentifierForGetOperation(s, true, TypeReference.TypeTimeSpan);
                                if (timeoutExpression.IsError())
                                {
                                    m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                                    return;
                                }
                                timeout = timeoutExpression.ExpressionCode;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            throw new NotImplementedException("Timeout data type or expression not implemented or supported.");
                        }

                        varTimeoutTime = m_scopeStack.Peek().AddVariable(
                            CreateStatementVariableName(context, "whileLoop_TimeoutTime"),
                            TypeReference.TypeDateTime,
                            new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                            EntryModifiers.Private,
                            null, -1);
                    }
                    else if (property.Is("Stoppable", PropertyBlockEntryType.Flag))
                    {
                        loopExpressions.Add(
                            Expression.IfThen(
                                Expression.Call(
                                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                                    typeof(ICallContext).GetMethod(nameof(ICallContext.StopRequested), new Type[] { })),
                                Expression.Block(
                                    Expression.Call(
                                        m_currentProcedure.ContextReferenceInternal,
                                        typeof(IScriptCallContext).GetMethod("Log", new Type[] { typeof(string) }),
                                        Expression.Constant("Loop stopped by user!")),
                                    Expression.Break(breakLabel))));
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Unknown property: \"");
                        sb.Append(property.Name);
                        sb.Append("\" on loop.");
                        // TODO: Figure out a way to give a proper error message for Timeout, as we would hit Timeout here
                        //       even if we write Timeout correctly but forget to give it a value
                        switch(property.Name.ToLower())
                        {
                            case "stoppable":
                                sb.Append(" Did you mean \"Stoppable\"?");
                                break;
                            default:
                                sb.Append(" Check spelling or parameters.");
                                break;
                        }
                        sb.Append(" Keep in mind properties are case-sensitive.");
                        m_errors.SymanticError(property.Line, -1, false, sb.ToString());
                    }
                }
            }
            #endregion


            varEntryTime = whileScope.AddVariable(
                            CreateStatementVariableName(context, "whileLoop_EntryTime"),
                            TypeReference.TypeDateTime,
                            new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                            EntryModifiers.Private,
                            null, -1);

            var loggingEnabled = Expression.Property(
                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                    typeof(ICallContext).GetProperty("LoggingEnabled"));

            var timeoutLoggingCall = Expression.Call(
                m_currentProcedure.ContextReferenceInternal,
                typeof(IScriptCallContext).GetMethod("Log", new Type[] { typeof(string) }),
                Expression.Constant("Loop timeout!"));

            List<Expression> timeAssignments = new List<Expression>();
            timeAssignments.Add(
                Expression.Assign(varEntryTime.VariableExpression, Expression.Property(null, typeof(DateTime).GetProperty("Now"))));

            if (varTimeoutTime != null)
            {
                timeAssignments.Add(
                    Expression.Assign(varTimeoutTime.VariableExpression, Expression.Add(varEntryTime.VariableExpression, timeout)));
                loopExpressions.Add(
                    Expression.IfThen(
                        Expression.GreaterThan(
                            Expression.Property(null, typeof(DateTime).GetProperty("Now")),
                            varTimeoutTime.VariableExpression),
                        Expression.Block(
                            Expression.IfThen(loggingEnabled, timeoutLoggingCall),
                            Expression.Call(
                                m_currentProcedure.ContextReferenceInternal,
                                typeof(IScriptCallContext).GetMethod("SetLoopExitReason", new Type[] { typeof(string) }),
                                Expression.Constant("Timeout")),
                            Expression.Break(breakLabel))));
            }

            foreach (var assignment in timeAssignments)
            {
                statementExpressions.Add(assignment);
            }

            if (isBlockSub)
            {
                statementExpressions.Add(
                    Expression.Loop(
                        subStatements[0].GetBlockCode(loopExpressions, null),
                        breakLabel,
                        continueLabel));
            }
            else
            {
                loopExpressions.Add(subStatements[0].GetOnlyStatementCode());
                statementExpressions.Add(
                    Expression.Loop(
                        Expression.Block(loopExpressions),
                        breakLabel,
                        continueLabel));
            }

            List<ProcedureVariable> whileVariables = whileScope.GetVariables(); // Contains the entry time and timeout variables
            List<Expression> whileLoopExpression = new List<Expression>();
            whileLoopExpression.Add(Expression.Block(whileVariables.Select(v => ((ParameterExpression)v.VariableExpression)), statementExpressions.ToArray()));

            m_scopeStack.Peek().AddStatementCode(whileLoopExpression.ToArray());
        }

        public override void EnterDoWhileStatement([NotNull] SBP.DoWhileStatementContext context)
        {
            this.AddEnterStatement(context);
            //m_lastPropertyBlock = null;
            m_expressionData.PushStackLevel("DoWhileStatement");
            m_enteredLoopStatement = true;
        }

        public override void ExitDoWhileStatement([NotNull] SBP.DoWhileStatementContext context)
        {
        }

        public override void ExitBreakStatement([NotNull] SBP.BreakStatementContext context)
        {
            this.AddEnterStatement(context);
            var scopeForLoop = this.TryGetLoopScope();
            if (scopeForLoop != null)
            {
                m_scopeStack.Peek().AddStatementCode(
                    Expression.Block(
                        Expression.Call(
                            m_currentProcedure.ContextReferenceInternal,
                            typeof(IScriptCallContext).GetMethod("SetLoopExitReason", new Type[] { typeof(string) }),
                            Expression.Constant("Break")),
                        Expression.Break(scopeForLoop.BreakLabel)));
            }
            else
            {
                throw new NotImplementedException("Parsing error: loop scope not found.");
            }
        }

        public override void ExitContinueStatement([NotNull] SBP.ContinueStatementContext context)
        {
            this.AddEnterStatement(context);
            var scopeForLoop = this.TryGetLoopScope();
            if (scopeForLoop != null)
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Continue(scopeForLoop.ContinueLabel));
            }
            else
            {
                throw new NotImplementedException("Parsing error: loop scope not found.");
            }
        }

        private ProcedureParsingScope TryGetLoopScope()
        {
            return m_scopeStack.FirstOrDefault(s => s.BreakLabel != null);
        }

        #endregion

        #region Using Statement

        public override void EnterUsingStatement([NotNull] SBP.UsingStatementContext context)
        {
            // Create a sub-scope in case the using statement contains a variable declaraton. That variable
            // should only be visible within this statement scope.
            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "UsingStatement", ProcedureParsingScope.ScopeType.Block));

            this.AddEnterStatement(context);
        }

        public override void ExitUsingStatement([NotNull] SBP.UsingStatementContext context)
        {
            var subStatements = m_scopeStack.Peek().GetSubStatements();

            Expression scopeCode = null;
            if (subStatements[0].Type == ProcedureParsingScope.ScopeType.Block)
            {
                scopeCode = subStatements[0].GetBlockCode(null, null);
            }
            else
            {
                scopeCode = subStatements[0].GetOnlyStatementCode();
            }

            var usingVariable = m_scopeStack.Peek().AddVariable("usingVariable_" + context.Start.Line.ToString(), new TypeReference(typeof(IDisposable)), null, EntryModifiers.Private, null, -1);
            var variableAssignment = m_scopeStack.Peek().UsingVariableAssignment;
            var usingVariableAssignment = Expression.Assign(usingVariable.VariableExpression, variableAssignment);

            var disposeHelper = typeof(ExecutionHelperMethods).GetMethod(
                nameof(ExecutionHelperMethods.DisposeObject));

            var usingCode = Expression.TryFinally(
                Expression.Block(
                    usingVariableAssignment,
                    scopeCode
                ),
                Expression.Call(disposeHelper, m_currentProcedure.ContextReferenceInternal, usingVariable.VariableExpression));
            m_scopeStack.Peek().AddStatementCode(usingCode);

            var statementBlock = m_scopeStack.Pop();
            m_scopeStack.Peek().AddStatementCode(statementBlock.GetBlockCode());
        }

        public override void EnterUsingExpression([NotNull] SBP.UsingExpressionContext context)
        {
            var child = context.GetChild(0) as Antlr4.Runtime.RuleContext;
            if (child.RuleIndex == SBP.RULE_simpleVariableDeclaration)
            {
            }
            else if (child.RuleIndex == SBP.RULE_expression)
            {
                m_expressionData.PushStackLevel("UsingStatement");
            }
            else
            {
                throw new NotImplementedException(String.Format("What? Unknown using expression type (rule = {0}).", child.RuleIndex));
            }
        }

        public override void ExitUsingExpression([NotNull] SBP.UsingExpressionContext context)
        {
            //var usingVariable = m_scopeStack.Peek().AddVariable("usingVariable_" + context.start.Line.ToString(), typeof(IDisposable), null, EntryModifiers.Private);

            Expression usingExpression = null;

            var child = context.GetChild(0) as Antlr4.Runtime.RuleContext;
            if (child.RuleIndex == SBP.RULE_simpleVariableDeclaration)
            {
                if (m_variableInitializer.IsConstant && m_variableInitializer.Value == null)
                {
                    // Convert the null value to the type of the variable
                    if (m_variableType.Type == typeof(string)) m_variableInitializer = new SBExpressionData(TypeReference.TypeString, null);
                }
                if (m_variableType == null)
                {
                    throw new NotImplementedException();
                }
                else if (m_variableType.Type != typeof(VarSpecifiedType))
                {
                    if (m_variableInitializer.IsValueType &&
                        m_variableInitializer.IsConstant &&
                        m_variableInitializer.Value == null)
                    {
                        m_variableInitializer.NarrowGetValueType(m_variableType);
                    }
                    else if (m_variableType != m_variableInitializer.DataType && !m_variableType.Type.IsAssignableFrom(m_variableInitializer.DataType.Type))
                    {
                        m_errors.InternalError(context.Start.Line, context.Start.Column, "Conversion of variable initializer is not implemented.");
                    }
                }
                if (m_variableType.Type == typeof(VarSpecifiedType))
                {
                    m_variableType = m_variableInitializer.DataType;
                }

                var scope = m_scopeStack.Peek();
                var v = scope.AddVariable(m_variableName, m_variableType, null, EntryModifiers.Private, m_file.FilePath, context.Start.Line);
                usingExpression = Expression.Assign(v.VariableExpression, m_variableInitializer.ExpressionCode);

                m_variableName = null;
                m_variableInitializer = null;
            }
            else if (child.RuleIndex == SBP.RULE_expression)
            {
                var levelData = m_expressionData.PopStackLevel();
                var exp = levelData.Stack.Pop();

                exp = this.ResolveForGetOperation(exp);

                if (!exp.IsValueType)
                {
                    throw new NotImplementedException("Something wrong with the using expression; it is not an IDisposable type.");
                }

                usingExpression = exp.ExpressionCode;
            }
            else
            {
                throw new NotImplementedException(String.Format("What? Unknown using expression type (rule = {0}).", child.RuleIndex));
            }

            if ((!usingExpression.Type.IsClass && !usingExpression.Type.IsInterface) || !typeof(IDisposable).IsAssignableFrom(usingExpression.Type))
            {
                throw new NotImplementedException("Something wrong with the using expression; it is not an IDisposable type.");
            }
            m_scopeStack.Peek().UsingVariableAssignment = usingExpression;  // Save for later
        }

        #endregion

        #region Return

        public override void EnterReturnStatement([NotNull] SBP.ReturnStatementContext context)
        {
            this.AddEnterStatement(context);
            m_expressionData.PushStackLevel("Return Statement");
        }

        public override void ExitReturnStatement([NotNull] SBP.ReturnStatementContext context)
        {
            var levelData = m_expressionData.PopStackLevel();
            if (context.ChildCount == 2)
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Return(m_currentProcedure.ReturnLabel));
                return;
            }
            if (m_currentProcedure.ReturnType == null)
            {
                m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Return value missing.");
                return;
            }
            if (levelData.Stack.Count == 0)
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "");
                return;
            }

            var exp = levelData.Stack.Pop();
            exp = this.ResolveForGetOperation(exp).NarrowGetValueType();
            var code = exp.ExpressionCode;
            if (exp.IsError())
            {
                code = Expression.Default(m_currentProcedure.ReturnType.Type);
                return;
            }

            // If container, get the value of the container.
            if (exp.ExpressionCode.Type.IsGenericType && exp.ExpressionCode.Type.GetGenericTypeDefinition() == typeof(IValueContainer<>))
            {
                code = Expression.Call(
                    code,
                    code.Type.GetMethod(nameof(IValueContainer<int>.GetTypedValue)),    // Note: 'int' is just used to make compiler happy.
                    Expression.Convert(Expression.Constant(null), typeof(Logging.ILogger)));
            }

            if (!m_currentProcedure.ReturnType.Type.IsAssignableFrom(code.Type))
            {
                m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Expression data type is not compatible with the procedure return type.");
                return;
            }

            var procedureReference = Expression.Convert(Expression.Property(m_currentProcedure.ContextReferenceInternal, nameof(IScriptCallContext.Self)), typeof(FileProcedure));
            code = Expression.Call(procedureReference, nameof(FileProcedure.OnReturn), new Type[] { code.Type }, code);

            m_scopeStack.Peek().AddStatementCode(Expression.Return(m_currentProcedure.ReturnLabel, code));
        }

        #endregion

        #region Throw

        public override void EnterThrowStatement([NotNull] SBP.ThrowStatementContext context)
        {
            base.EnterThrowStatement(context);
        }

        public override void ExitThrowStatement([NotNull] SBP.ThrowStatementContext context)
        {
            base.ExitThrowStatement(context);
        }

        #endregion

        #region Test Step

        private int m_stepIndex;
        private string m_stepTitle;

        public override void EnterStepStatement([NotNull] SBP.StepStatementContext context)
        {
            m_stepIndex = -1;
            m_stepTitle = "";
        }

        public override void ExitStepStatement([NotNull] SBP.StepStatementContext context)
        {
            if (m_stepIndex > 0)
            {
                m_currentProcedure.SetStepIndex(m_stepIndex);
            }
            else
            {
                m_stepIndex = m_currentProcedure.GetNextStepIndex();
            }
            m_scopeStack.Peek().AddStatementCode(
                Expression.Call(
                    m_currentProcedure.ContextReferenceInternal,
                    typeof(IScriptCallContext).GetMethod(nameof(IScriptCallContext.EnterTestStep)),
                    Expression.Constant(context.Start.Line),
                    Expression.Constant(context.Start.Column),
                    Expression.Constant(m_stepIndex),
                    Expression.Constant(m_stepTitle)));
        }

        public override void ExitStepIndex([NotNull] SBP.StepIndexContext context)
        {
            m_stepIndex = Int32.Parse(context.GetText());
        }

        public override void ExitStepTitle([NotNull] SBP.StepTitleContext context)
        {
            m_stepTitle = ParseStringLiteral(context.GetText(), context);
        }

        #endregion

        #region Log Statement

        private string m_logStatementModifier;

        public override void EnterLogStatement([NotNull] SBP.LogStatementContext context)
        {
            this.AddEnterStatement(context);
            m_expressionData.PushStackLevel("LogStatement");
            m_logStatementModifier = null;
        }

        public override void ExitLogStatement([NotNull] SBP.LogStatementContext context)
        {
            var levelData = m_expressionData.PopStackLevel();
            if (levelData.Stack.Count == 0)
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "");
                return;
            }
            var exp = levelData.Stack.Pop();
            exp = this.ResolveIfIdentifier(exp, m_inFunctionScope);
            if (exp.IsError())
            {
                return;
            }

            var code = exp.IsError() ? Expression.Constant("<EXPRESSION ERROR>") : ResolveForGetOperation(exp).ExpressionCode;

            if (code != null)
            {
                var propertyBlock = m_lastElementPropertyBlock;

                var logExpressionResultVar = Expression.Variable(code.Type, "logExpressionValue");
                var assignLogExpressionResultVar = Expression.Assign(logExpressionResultVar, code);
                var logStringVar = Expression.Variable(typeof(string), "logString");

                var loggingEnabled = Expression.Property(
                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                    typeof(ICallContext).GetProperty("LoggingEnabled"));

                var logValue = code;
                if (code.Type != typeof(string))
                {
                    if (code.Type.IsPrimitive || code.Type.IsEnum)
                    {
                        logValue = Expression.Call(logExpressionResultVar, typeof(object).GetMethod("ToString", new Type[] { }));
                    }
                    else
                    {
                        if (typeof(IEnumerable<string>).IsAssignableFrom(code.Type))
                        {
                            #region Special handling of IEnumerable<string>

                            var helper = typeof(ExecutionHelperMethods).GetMethod(
                                nameof(ExecutionHelperMethods.LogList));

                            var helperCall = Expression.Call(helper,
                                m_currentProcedure?.ContextReferenceInternal,
                                code);

                            var logListStatementBlock = Expression.IfThen(
                                loggingEnabled,
                                Expression.Block(
                                    new ParameterExpression[] { logExpressionResultVar },
                                    Expression.TryCatch(
                                        helperCall,
                                        Expression.Catch(
                                            typeof(Exception),
                                            Expression.Empty()))));

                            m_scopeStack.Peek().AddStatementCode(logListStatementBlock);

                            return;
                            #endregion
                        }
                        else
                        {
                            logValue = Expression.Condition(
                                Expression.Equal(logExpressionResultVar, Expression.Constant(null)),
                                Expression.Constant("<null>"),
                                Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) }),
                                        Expression.Property(
                                            Expression.Call(logExpressionResultVar, typeof(object).GetMethod("GetType", new Type[] { })),
                                            "FullName"),
                                        Expression.Constant(" - "),
                                        Expression.Call(logExpressionResultVar, typeof(object).GetMethod("ToString", new Type[] { }))));
                        }
                    }
                }
                else
                {
                    logValue = Expression.Condition(
                        Expression.Equal(logExpressionResultVar, Expression.Constant(null)),
                        Expression.Constant("<null>"),
                        Expression.Convert(logExpressionResultVar, typeof(string)));
                }

                var loggingCall = Expression.Call(
                    m_currentProcedure.ContextReferenceInternal,
                    typeof(IScriptCallContext).GetMethod("LogStatement", new Type[] { typeof(string) }),
                    logStringVar);

                var statementBlock = Expression.IfThen(
                    loggingEnabled,
                    Expression.Block(
                        new ParameterExpression[] { logExpressionResultVar, logStringVar },
                        Expression.TryCatch(
                            Expression.Block(
                                assignLogExpressionResultVar,
                                Expression.Assign(logStringVar, logValue),
                                loggingCall),
                            Expression.Catch(
                                typeof(Exception),
                                Expression.Empty()))));

                m_scopeStack.Peek().AddStatementCode(statementBlock);
            }
        }

        public override void ExitLogModifier([NotNull] SBP.LogModifierContext context)
        {
            m_logStatementModifier = context.GetText();
        }

        #endregion

        #region Expect

        public override void EnterExpectStatement([NotNull] SBP.ExpectStatementContext context)
        {
            this.AddEnterStatement(context);
            m_expressionData.PushStackLevel("ExpectStatement");
            m_isSimpleExpectWithValue = false;
            var expression = context.GetChild(context.ChildCount - 2).GetChild(1).GetChild(0) as SBP.ExpressionContext;

            if (expression is SBP.ExpBinaryContext || expression is SBP.ExpBetweenContext || expression is SBP.ExpEqualsWithToleranceContext)
            {
                m_isSimpleExpectWithValue = true;
            }
        }

        public override void ExitExpectStatement([NotNull] SBP.ExpectStatementContext context)
        {
            var isAssert = (context.children[0].GetText() == "assert");

            var levelData = m_expressionData.PopStackLevel();
            var expression = levelData.Stack.Pop();
            expression = this.ResolveForGetOperation(expression);
            if (!CheckExpressionsForErrors(context, expression))
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                return;
            }

            ExpressionType nodetype = expression.ExpressionCode.NodeType;

            string expressionText = GetNodeText(context.GetChild(context.ChildCount - 2).GetChild(1));
            string title = String.Empty;
            if (context.ChildCount > 3)
            {
                title = context.GetChild(1).GetText();
                title = title.Substring(1, title.Length - 2);   // Strip quotes
            }

            if (expression.ExpressionCode.Type != typeof(bool))
            {
                m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Expression for 'expect' statement is not boolean.");
            }
            else
            {
                LabelTarget returnLabel = Expression.Label(typeof(bool));
                var contextParameter = Expression.Parameter(typeof(IScriptCallContext), "context");

                var expectCall = Expression.Call(
                    s_ExpectStatement,
                    m_currentProcedure.ContextReferenceInternal,
                    expression.ExpressionCode,
                    isAssert ? Expression.Constant(Verdict.Error) : Expression.Constant(Verdict.Fail),
                    Expression.Constant(title),
                    Expression.Constant(expressionText));

                var conditionalReturn = Expression.Condition(
                    expectCall,
                    Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type)),
                    Expression.Empty());

                var stepCode = Expression.Block(
                    Expression.Assign(
                        Expression.Property(m_currentProcedure.ContextReferenceInternal, nameof(IScriptCallContext.IsSimpleExpectStatementWithValue)), Expression.Constant(m_isSimpleExpectWithValue)),
                    conditionalReturn);

                m_scopeStack.Peek().AddStatementCode(stepCode);
            }

            m_isSimpleExpectWithValue = false;
        }

        #endregion

        public override void ExitEmptyStatement([NotNull] SBP.EmptyStatementContext context)
        {
            m_scopeStack.Peek().AddStatementCode(Expression.Default(typeof(void)));
        }

        #region Local Variable

        public override void EnterLocalVariableDeclarationStatement([NotNull] SBP.LocalVariableDeclarationStatementContext context)
        {
            this.AddEnterStatement(context);    // TODO: Not if no initializer, and only for static if setting.
            m_variableModifier = VariableModifier.None;
        }

        public override void ExitLocalVariableDeclarationStatement([NotNull] SBP.LocalVariableDeclarationStatementContext context)
        {
            TypeReference type = m_variableType;
            VariableModifier modifier = m_variableModifier;
            if (type == null)
            {
                return;
            }
            if (modifier == VariableModifier.None)
            {
                if (type.Type == typeof(VarSpecifiedType))
                {
                    if (m_variables[0].Initializer.IsError())
                    {
                        return;     // Just leave; no point in spending more time on this variable.
                    }
                    else if (m_variables[0].Initializer.DataType != null)
                    {
                        type = m_variables[0].Initializer.DataType;
                    }
                    else if (m_variables[0].Initializer.ExpressionCode != null)
                    {
                        type = new TypeReference(m_variables[0].Initializer.ExpressionCode.Type);
                    }
                    else
                    {
                        throw new NotImplementedException("Unknown value type for assignment.");
                    }
                }
                foreach (var variable in m_variables)
                {
                    if (variable.Initializer.IsError())
                    {
                        return;
                    }
                    if (!type.IsAssignableFrom(variable.Initializer.DataType))
                    {
                        m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Variable assignment of incompatible type.");
                        return;
                    }
                    if (m_scopeStack.Peek().StatementCount > 0)
                    {
                        var scope = m_scopeStack.Peek();
                        var v = scope.AddVariable(variable.Name, type, null, EntryModifiers.Private, m_file.FilePath, context.Start.Line /* TODO: correct */);
                        scope.AddStatementCode(Expression.Assign(v.VariableExpression, variable.Initializer.ExpressionCode));
                    }
                    else
                    {
                        m_scopeStack.Peek().AddVariable(variable.Name, type, variable.Initializer, EntryModifiers.Private, m_file.FilePath, context.Start.Line /* TODO: correct */);
                    }
                }
            }
            else    // Some kind of static
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Expression Statement

        public override void EnterExpressionStatement([NotNull] SBP.ExpressionStatementContext context)
        {
            m_expressionData.PushStackLevel("ExpressionStatement @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
        }

        public override void ExitExpressionStatement([NotNull] SBP.ExpressionStatementContext context)
        {
            var expression = m_expressionData.PopStackLevel().Stack.Pop();
            if (expression.IsError())
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());   // Add ampty statement, to make the rest of the error handling easier.
            }
            else
            {
                var expressionStatement = expression.ExpressionCode;

                var conditionalReturn = Expression.Condition(
                    Expression.Call(s_PostExpressionStatement, m_currentProcedure.ContextReferenceInternal),
                    Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type)),
                    Expression.Empty());

                m_scopeStack.Peek().AddStatementCode(
                    Expression.Block(
                        this.CreateEnterStatement(context.Start.Line, context.Start.Column),
                        expressionStatement,
                        conditionalReturn));
            }
        }

        #endregion

        #endregion

        public override void EnterStatementarguments([NotNull] SBP.StatementargumentsContext context)
        {
        }

        public override void ExitStatementarguments([NotNull] SBP.StatementargumentsContext context)
        {
        }

        #region Helpers and procedure framework

        private Expression GetContextAccess()
        {
            return m_currentProcedure.ContextReference;
        }

        static private string CreateStatementVariableName(ParserRuleContext context, string name)
        {
            return $"_{name}_{context.Start.Line}_{context.Start.Column}";
        }

        #endregion
    }
}
