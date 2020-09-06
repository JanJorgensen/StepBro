﻿using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private VariableModifier m_variableModifier = VariableModifier.None;
        private TypeReference m_variableType;
        private string m_variableName = "";
        private SBExpressionData m_variableInitializer = null;
        private List<NamedData<SBExpressionData>> m_variables;

        public void CreateVariablesList()
        {
            m_variables = new List<NamedData<SBExpressionData>>();
        }

        public override void EnterVariableModifier([NotNull] TSP.VariableModifierContext context)
        {
            var modifier = context.GetText();
            if (modifier == "static") m_variableModifier = VariableModifier.Static;
            else if (modifier == "execution") m_variableModifier = VariableModifier.Execution;
            else if (modifier == "session") m_variableModifier = VariableModifier.Session;
        }

        public override void EnterSimpleVarDeclarators([NotNull] TSP.SimpleVarDeclaratorsContext context)
        {
            this.CreateVariablesList();
        }

        public override void EnterVariableType([NotNull] TSP.VariableTypeContext context)
        {
            m_variableType = null;
            m_expressionData.PushStackLevel("VariableType");
        }

        public override void ExitVariableType([NotNull] TSP.VariableTypeContext context)
        {
            m_expressionData.PopStackLevel();
            m_variableType = m_typeStack.Pop();
        }

        public override void EnterVariableDeclaratorId([NotNull] TSP.VariableDeclaratorIdContext context)
        {
            m_variableName = context.GetText();
        }

        public override void ExitVariableDeclaratorWithoutAssignment([NotNull] TSP.VariableDeclaratorWithoutAssignmentContext context)
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
                    throw new NotImplementedException();
                }
            }
        }

        public override void ExitVariableDeclarator([NotNull] TSP.VariableDeclaratorContext context)
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
                                m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "");
                                throw new NotImplementedException("Convertion of variable initializer is not implemented.");
                            }
                        }
                    }
                }
                m_variables.Add(
                    new NamedData<SBExpressionData>(
                        m_variableName,
                        m_variableInitializer));
                m_variableName = null;
                m_variableInitializer = null;
            }
        }

        public override void ExitVariableInitializerArray([NotNull] TSP.VariableInitializerArrayContext context)
        {
            base.ExitVariableInitializerArray(context);
        }

        public override void EnterVariableInitializerExpression([NotNull] TSP.VariableInitializerExpressionContext context)
        {
            m_expressionData.PushStackLevel("VariableInitializerExpression @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
        }

        public override void ExitVariableInitializerExpression([NotNull] TSP.VariableInitializerExpressionContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            m_variableInitializer = this.ResolveForGetOperation(stack.Pop());
            if (m_variableInitializer.IsError())
            {
                m_errors.UnresolvedIdentifier(m_variableInitializer.Token.Line, m_variableInitializer.Token.Column, m_variableInitializer.Value as string);
            }
        }
    }
}
