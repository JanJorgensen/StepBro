using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
        private FileProcedure m_currentProcedure = null;    // The procedure currently being parsed.
        private bool m_inFunctionScope = false;
        private FileProcedure m_lastProcedure = null;       // The last procedure the parser ended parsing.
        private ProcedureParsingScope m_procedureBaseScope = null;
        private Stack<ProcedureParsingScope> m_scopeStack = new Stack<ProcedureParsingScope>();
        private bool m_enteredLoopStatement = false;
        private string m_modifiers = null;
        //private bool m_awaitsExpectExpression = false;

        public IFileProcedure LastParsedProcedure { get { return m_lastProcedure; } }

        private SBExpressionData m_callAssignmentTarget = null;
        private bool m_callAssignmentAwait = false;
        private Stack<Stack<SBExpressionData>> m_arguments = new Stack<Stack<SBExpressionData>>();
        private Stack<Stack<SBExpressionData>> m_statementExpressions = new Stack<Stack<SBExpressionData>>();
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
            var stack = m_expressionData.PopStackLevel();
            var parInitializer = this.ResolveForGetOperation(stack.Pop());
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
            var stack = m_expressionData.PopStackLevel();
            var condition = stack.Pop();
            var subStatements = m_scopeStack.Peek().GetSubStatements();
            var attributes = m_scopeStack.Peek().GetAttributes();

            condition = this.ResolveForGetOperation(condition);

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
            //m_lastPropertyBlock = null;
            m_expressionData.PushStackLevel("ForStatement");
            m_enteredLoopStatement = true;
        }

        public override void ExitForStatement([NotNull] SBP.ForStatementContext context)
        {
        }

        public override void EnterWhileStatement([NotNull] SBP.WhileStatementContext context)
        {
            this.AddEnterStatement(context);
            //m_lastPropertyBlock = null;
            m_expressionData.PushStackLevel("WhileStatement");
            m_enteredLoopStatement = true;
            m_scopeStack.Push(new ProcedureParsingScope(m_scopeStack.Peek(), "while", ProcedureParsingScope.ScopeType.Block));
        }

        public override void ExitWhileStatement([NotNull] SBP.WhileStatementContext context)
        {
            var whileScope = m_scopeStack.Pop();
            var stack = m_expressionData.PopStackLevel();
            var condition = stack.Pop();
            var subStatements = whileScope.GetSubStatements();
            var attributes = whileScope.GetAttributes();
            ProcedureVariable varLoopIndex = null;
            ProcedureVariable varEntryTime = null;
            ProcedureVariable varTimeoutTime = null;
            Expression timeout = null;

            var props = m_lastElementPropertyBlock;
            m_lastElementPropertyBlock = null;

            condition = this.ResolveForGetOperation(condition);
            if (condition.IsError())
            {
                m_scopeStack.Peek().AddStatementCode(Expression.Empty());
                return;
            }

            var conditionExpression = condition.ExpressionCode;

            if (condition.IsValueType && condition.DataType.Type != typeof(bool))
            {
                throw new NotImplementedException("Something wrong with the condition expression.");
            }

            var breakLabel = Expression.Label();

            var isBlockSub = (subStatements[0].Type == ProcedureParsingScope.ScopeType.Block);
            if (isBlockSub)
            {
                breakLabel = whileScope.BreakLabel;
            }

            var statementExpressions = new List<Expression>();
            var loopExpressions = new List<Expression>();
            loopExpressions.Add(
                Expression.IfThen(
                    Expression.Not(conditionExpression),
                    Expression.Break(breakLabel)));

            varLoopIndex = m_scopeStack.Peek().AddVariable(
                CreateStatementVariableName(context, "whileLoop_index"),
                TypeReference.TypeInt64,
                new SBExpressionData(Expression.Constant(0L)),
                EntryModifiers.Private);
            loopExpressions.Add(Expression.Increment(varLoopIndex.VariableExpression));     // index++; therefore index = 1 inside and after first iteration.

            // TODO: Add some logging, interactive break check, timeout and other stuff
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
                            EntryModifiers.Private);
                    }
                }
            }
            #endregion


            varEntryTime = m_scopeStack.Peek().AddVariable(
                            CreateStatementVariableName(context, "whileLoop_EntryTime"),
                            TypeReference.TypeDateTime,
                            new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                            EntryModifiers.Private);


            varEntryTime = m_scopeStack.Peek().AddVariable(
                CreateStatementVariableName(context, "whileLoop_EntryTime"),
                TypeReference.TypeDateTime,
                new SBExpressionData(Expression.Field(null, typeof(DateTime).GetField("MinValue"))),
                EntryModifiers.Private);

            var loggingEnabled = Expression.Property(
                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                    typeof(ICallContext).GetProperty("LoggingEnabled"));

            var timeoutLoggingCall = Expression.Call(
                m_currentProcedure.ContextReferenceInternal,
                typeof(IScriptCallContext).GetMethod("Log", new Type[] { typeof(string) }),
                Expression.Constant("Loop timeout!"));

            m_scopeStack.Peek().AddStatementCode(
                Expression.Assign(varEntryTime.VariableExpression, Expression.Property(null, typeof(DateTime).GetProperty("Now"))));

            if (varTimeoutTime != null)
            {
                m_scopeStack.Peek().AddStatementCode(
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
                            Expression.Break(breakLabel))));
            }

            if (isBlockSub)
            {
                statementExpressions.Add(
                    Expression.Loop(
                        subStatements[0].GetBlockCode(loopExpressions, null),
                        breakLabel,
                        subStatements[0].ContinueLabel));
            }
            else
            {
                loopExpressions.Add(subStatements[0].GetOnlyStatementCode());
                statementExpressions.Add(
                    Expression.Loop(
                        Expression.Block(loopExpressions),
                        breakLabel));
            }

            m_scopeStack.Peek().AddStatementCode(statementExpressions.ToArray());
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
                m_scopeStack.Peek().AddStatementCode(Expression.Break(scopeForLoop.BreakLabel));
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

            var usingVariable = m_scopeStack.Peek().AddVariable("usingVariable_" + context.Start.Line.ToString(), new TypeReference(typeof(IDisposable)), null, EntryModifiers.Private);
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
                var v = scope.AddVariable(m_variableName, m_variableType, null, EntryModifiers.Private);
                usingExpression = Expression.Assign(v.VariableExpression, m_variableInitializer.ExpressionCode);

                m_variableName = null;
                m_variableInitializer = null;
            }
            else if (child.RuleIndex == SBP.RULE_expression)
            {
                var stack = m_expressionData.PopStackLevel();
                var exp = stack.Pop();

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
            var stack = m_expressionData.PopStackLevel();
            if (stack.Count == 0)
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "");
                return;
            }

            var exp = stack.Pop();
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
                    code.Type.GetMethod("GetTypedValue"),
                    Expression.Convert(Expression.Constant(null), typeof(Logging.ILogger)));
            }


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
            var stack = m_expressionData.PopStackLevel();
            if (stack.Count == 0)
            {
                m_errors.InternalError(context.Start.Line, context.Start.Column, "");
                return;
            }
            var exp = stack.Pop();
            exp = this.ResolveIfIdentifier(exp, m_inFunctionScope);
            if (exp.IsError())
            {
                return;
            }

            var code = exp.IsError() ? Expression.Constant("<EXPRESSION ERROR>") : exp.ExpressionCode;

            if (code != null)
            {
                var propertyBlock = m_lastElementPropertyBlock;

                var resultVar = Expression.Variable(code.Type, "logValue");
                var assignVar = Expression.Assign(resultVar, code);

                var loggingEnabled = Expression.Property(
                    Expression.Convert(m_currentProcedure.ContextReferenceInternal, typeof(ICallContext)),
                    typeof(ICallContext).GetProperty("LoggingEnabled"));

                var logValue = code;
                if (code.Type != typeof(string))
                {
                    if (code.Type.IsPrimitive || code.Type.IsEnum)
                    {
                        logValue = Expression.Call(resultVar, typeof(object).GetMethod("ToString", new Type[] { }));
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
                                logValue);

                            var logListStatementBlock = Expression.IfThen(
                                loggingEnabled,
                                Expression.Block(
                                    new ParameterExpression[] { resultVar },
                                    Expression.TryCatch(
                                        Expression.Block(
                                            assignVar,
                                            helperCall),
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
                                Expression.Equal(resultVar, Expression.Constant(null)),
                                Expression.Constant("<null>"),
                                Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) }),
                                        Expression.Property(
                                            Expression.Call(resultVar, typeof(object).GetMethod("GetType", new Type[] { })),
                                            "FullName"),
                                        Expression.Constant(" - "),
                                        Expression.Call(resultVar, typeof(object).GetMethod("ToString", new Type[] { }))));
                        }
                    }
                }

                var loggingCall = Expression.Call(
                    m_currentProcedure.ContextReferenceInternal,
                    typeof(IScriptCallContext).GetMethod("LogStatement", new Type[] { typeof(string) }),
                    logValue);

                var statementBlock = Expression.IfThen(
                    loggingEnabled,
                    Expression.Block(
                        new ParameterExpression[] { resultVar },
                        Expression.TryCatch(
                            Expression.Block(
                                assignVar,
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
            //m_awaitsExpectExpression = true;
        }

        public override void ExitExpectStatement([NotNull] SBP.ExpectStatementContext context)
        {
            var isAssert = (context.children[0].GetText() == "assert");

            var stack = m_expressionData.PopStackLevel();
            var expression = stack.Pop();
            expression = this.ResolveForGetOperation(expression);
            if (expression.IsError())
            {
                return;
            }

            string expressionText = context.GetChild(context.ChildCount - 2).GetChild(1).GetText();
            string title = String.Empty;
            if (context.ChildCount > 3)
            {
                title = context.GetChild(1).GetText();
                title = title.Substring(1, title.Length - 2);   // Strip quotes
            }

            //if (expression.ExpressionCode.NodeType == ExpressionType.Equal ||
            //    expression.ExpressionCode.NodeType == ExpressionType.NotEqual)
            //{
            //    throw new NotImplementedException();
            //}

            if (!(expression.ExpressionCode.Type == typeof(bool)))
            {
                m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Expression for 'expect' statement is not boolean.");
            }
            else
            {
                LabelTarget returnLabel = Expression.Label(typeof(bool));
                var contextParameter = Expression.Parameter(typeof(IScriptCallContext), "context");
                var actualValueParameter = Expression.Parameter(typeof(string).MakeByRefType(), "actualValue");

                // delegate bool ExpectStatementEvaluationDelegate(IScriptCallContext context, out string actualValue);

                var evaluationDelegate = Expression.Lambda<ExpectStatementEvaluationDelegate>(
                        Expression.Block(
                            Expression.Condition(
                                expression.ExpressionCode,
                                Expression.Block(
                                    Expression.Assign(actualValueParameter, Expression.Constant("<TRUE>")),
                                    Expression.Label(returnLabel, Expression.Constant(true))),
                                Expression.Block(
                                    Expression.Assign(actualValueParameter, Expression.Constant("<FALSE>")),
                                    Expression.Label(returnLabel, Expression.Constant(false))))),
                        contextParameter,
                        actualValueParameter);

                var expectCall = Expression.Call(
                    s_ExpectStatement,
                    m_currentProcedure.ContextReferenceInternal,
                    evaluationDelegate,
                    isAssert ? Expression.Constant(Verdict.Error) : Expression.Constant(Verdict.Fail),
                    Expression.Constant(title),
                    Expression.Constant(expressionText));

                var conditionalReturn = Expression.Condition(
                    expectCall,
                    Expression.Return(m_currentProcedure.ReturnLabel, Expression.Default(m_currentProcedure.ReturnType.Type)),
                    Expression.Empty());

                m_scopeStack.Peek().AddStatementCode(conditionalReturn);
            }
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
                        var v = scope.AddVariable(variable.Name, type, null, EntryModifiers.Private);
                        scope.AddStatementCode(Expression.Assign(v.VariableExpression, variable.Initializer.ExpressionCode));
                    }
                    else
                    {
                        m_scopeStack.Peek().AddVariable(variable.Name, type, variable.Initializer, EntryModifiers.Private);
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
            var expression = m_expressionData.PopStackLevel().Pop();
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
