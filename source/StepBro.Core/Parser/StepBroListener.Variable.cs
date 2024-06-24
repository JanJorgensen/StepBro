using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private struct VariableData
        {
            public string Name;
            public TypeReference Type;
            public SBExpressionData Initializer;

            public VariableData(string name, TypeReference type, SBExpressionData initializer)
            {
                this.Name = name;
                this.Type = type;
                this.Initializer = initializer;
            }
        }
        private VariableModifier m_variableModifier = VariableModifier.None;
        private TypeReference m_variableType;
        private TypeReference m_creatorType;
        private string m_variableName = "";
        private SBExpressionData m_variableInitializer = null;
        private List<VariableData> m_variables;

        public void CreateVariablesList()
        {
            m_variables = new List<VariableData>();
        }

        public override void EnterVariableModifier([NotNull] SBP.VariableModifierContext context)
        {
            var modifier = context.GetText();
            if (modifier == "static") m_variableModifier = VariableModifier.Static;
            else if (modifier == "execution") m_variableModifier = VariableModifier.Execution;
            else if (modifier == "session") m_variableModifier = VariableModifier.Session;
        }

        public override void EnterSimpleVarDeclarators([NotNull] SBP.SimpleVarDeclaratorsContext context)
        {
            this.CreateVariablesList();
        }

        public override void EnterVariableType([NotNull] SBP.VariableTypeContext context)
        {
            m_variableType = null;
            m_expressionData.PushStackLevel("VariableType");
        }

        public override void ExitVariableType([NotNull] SBP.VariableTypeContext context)
        {
            m_expressionData.PopStackLevel();
            m_variableType = m_typeStack.Pop();
        }

        public override void EnterCtorClassType([NotNull] SBP.CtorClassTypeContext context)
        {
            m_creatorType = null;
            m_expressionData.PushStackLevel("CTORType");
        }

        public override void ExitCtorClassType([NotNull] SBP.CtorClassTypeContext context)
        {
            m_expressionData.PopStackLevel();
            m_creatorType = m_typeStack.Pop();
        }

        public override void EnterVariableDeclaratorId([NotNull] SBP.VariableDeclaratorIdContext context)
        {
            m_variableName = context.GetText();
        }

        public override void EnterVariableDeclaratorQualifiedId([NotNull] SBP.VariableDeclaratorQualifiedIdContext context)
        {
            m_variableName = context.GetText();
        }

        public override void ExitVariableDeclaratorWithoutAssignment([NotNull] SBP.VariableDeclaratorWithoutAssignmentContext context)
        {
            if (m_variableType != null)
            {
                if (m_variableType.Type.IsValueType)
                {
                    m_variableInitializer = SBExpressionData.Constant(m_variableType, Activator.CreateInstance(m_variableType.Type));
                }
                else if (m_variableType.Type == typeof(string))
                {
                    m_variableInitializer = SBExpressionData.Constant(m_variableType, null);
                }
                else if (m_variableType.Type.IsClass)
                {
                    var defaultConstructor = m_variableType.Type.GetConstructor(new Type[0]);
                    try
                    {
                        var newExpression = Expression.New(defaultConstructor);
                        m_variableInitializer = new SBExpressionData(newExpression);
                    }
                    catch (ArgumentNullException)
                    {
                        m_errors.SymanticError(context.Start.Line, context.Start.Column, false, $"Type {m_variableType.Type.Name} does not have a default constructor");
                    }
                }
            }
        }

        public override void ExitVariableDeclarator([NotNull] SBP.VariableDeclaratorContext context)
        {
            if (m_variableType != null)
            {
                //m_variableInitializer = ResolveIfIdentifier(m_variableInitializer, m_inFunctionScope);
                if (m_variableInitializer.IsError())
                {
                    if (m_variableType.Type != typeof(VarSpecifiedType))
                    {
                        object def = null;
                        if (m_variableType.Type.IsValueType)
                        {
                            def = Activator.CreateInstance(m_variableType.Type);
                        }
                        m_variableInitializer = SBExpressionData.Constant(m_variableType, def);
                    }
                    else
                    {
                        // Just set the type to 'object' then.
                        m_variableInitializer = SBExpressionData.Constant(TypeReference.TypeObject, null);
                    }
                }
                if (m_variableInitializer.IsConstant && m_variableInitializer.Value == null)
                {
                    // Convert the null value to the type of the variable
                    if (m_variableType.Type == typeof(string))
                    {
                        m_variableInitializer = SBExpressionData.Constant(TypeReference.TypeString, null);
                    }
                }
                else if (m_variableInitializer.IsAwaitExpression)
                {
                    m_variableInitializer = new SBExpressionData(
                        this.MakeAwaitOperation(
                            m_variableInitializer.ExpressionCode, context, true, m_variableType.Type));
                }
                if (m_variableType.Type != typeof(VarSpecifiedType))
                {
                    if (m_variableInitializer.IsValueType &&
                        m_variableInitializer.IsConstant &&
                        m_variableInitializer.Value == null)
                    {
                        m_variableInitializer.NarrowGetValueType(m_variableType);
                    }
                    else if (m_variableType != m_variableInitializer.DataType && !m_variableType.Type.IsAssignableFrom(m_variableInitializer.DataType.Type))
                    {
                        m_variableInitializer = this.CastProcedureAssignmentArgumentIfNeeded(m_variableType, m_variableInitializer);
                        if (m_variableInitializer.DataType.Type != m_variableType.Type)
                        {
                            if (m_variableInitializer.DataType.Type == typeof(object) && m_variableInitializer.SuggestsAutomaticTypeConversion)
                            {
                                m_variableInitializer = new SBExpressionData(
                                    Expression.Convert(m_variableInitializer.ExpressionCode, m_variableType.Type));
                            }
                            else
                            {
                                m_variableInitializer.NarrowGetValueType();
                                if (m_variableInitializer.DataType.Type != m_variableType.Type)
                                {
                                    m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Conversion of variable initializer is not implemented.");
                                }
                            }
                        }
                    }
                }

                SBExpressionData resolvedIdentifier = ResolveQualifiedIdentifier(m_variableName, true);
                if (resolvedIdentifier.IsUnknownIdentifier)
                {
                    m_variables.Add(
                        new VariableData(
                            m_variableName,
                            m_variableType,
                            m_variableInitializer));
                }
                else
                {
                    int lineFirstDeclared = -1;
                    if (resolvedIdentifier.IsProcedureReference)
                    {
                        lineFirstDeclared = ((IProcedureReference)resolvedIdentifier.Value).ProcedureData.Line;
                    }
                    else if (resolvedIdentifier.Token != null)
                    {
                        lineFirstDeclared = resolvedIdentifier.Token.Line;
                    }
                    m_errors.SymanticError(
                        context.Start.Line, 
                        context.Start.Column, 
                        false,
                        $"Illegal to declare variable with same name as another variable or other type of element in the same scope. Variable: {m_variableName}." + (lineFirstDeclared != -1 ? $" First declared: Line {lineFirstDeclared}." : ""));
                }
                m_variableName = null;
                m_variableInitializer = null;
            }
        }

        public override void ExitVariableInitializerArray([NotNull] SBP.VariableInitializerArrayContext context)
        {
            base.ExitVariableInitializerArray(context);
        }

        public override void EnterVariableInitializerExpression([NotNull] SBP.VariableInitializerExpressionContext context)
        {
            m_expressionData.PushStackLevel("VariableInitializerExpression @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
        }

        public override void ExitVariableInitializerExpression([NotNull] SBP.VariableInitializerExpressionContext context)
        {
            var stack = m_expressionData.PopStackLevel();

            // If stack.Count is 0, then there is no body and we do not initialize to anything
            if (stack.Count != 0)
            {
                m_variableInitializer = this.ResolveForGetOperation(stack.Pop(), targetType: m_variableType);
            }

            if (m_variableInitializer.IsError())
            {
                m_errors.UnresolvedIdentifier(m_variableInitializer.Token.Line, m_variableInitializer.Token.Column, m_variableInitializer.Value as string);
            }
        }
    }
}
