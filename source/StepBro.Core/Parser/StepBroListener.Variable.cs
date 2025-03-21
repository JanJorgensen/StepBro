﻿using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
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
                            else if (m_variableType.Type.IsPrimitive && m_variableType.Type.IsPrimitiveNarrowableIntType())
                            {
                                m_variableInitializer = new SBExpressionData(
                                    Expression.Convert(m_variableInitializer.ExpressionCode, m_variableType.Type));
                            }
                            else
                            {
                                m_variableInitializer.NarrowGetValueType();
                                if (m_variableInitializer.DataType.Type != m_variableType.Type)
                                {
                                    string additional = " Value type: " + m_variableInitializer.DataType.Type.TypeNameSimple() + ".";
                                    if (m_variableInitializer.DataType.HasProcedureReference)
                                    {
                                        additional = " Value: procedure " + (m_variableInitializer.DataType.DynamicType as IFileProcedure).Name + ".";
                                    }
                                    var message = "Value is not compatible with the variable type." + additional;
                                    m_errors.SymanticError(
                                        context.Start.Line,
                                        context.Start.Column,
                                        false,
                                        message);
                                    m_variableInitializer = null;
                                }
                            }
                        }
                    }
                }

                SBExpressionData resolvedIdentifier = ResolveQualifiedIdentifier(m_variableName, true);
                if (!resolvedIdentifier.IsUnknownIdentifier)
                {
                    string additional = "";
                    //string fileFirstDeclared = null;
                    if (resolvedIdentifier.IsProcedureReference)
                    {
                        var proc = ((IProcedureReference)resolvedIdentifier.Value).ProcedureData;
                        additional = $" First declared: {System.IO.Path.GetFileName(proc.SourceFile)} line {proc.SourceLine}.";
                    }
                    else if (resolvedIdentifier.Value != null && resolvedIdentifier.Value is IIdentifierInfo idinfo && !String.IsNullOrEmpty(idinfo.SourceFile) && idinfo.SourceLine >= 0)
                    {
                        additional = $" First declared: {System.IO.Path.GetFileName(idinfo.SourceFile)} line {idinfo.SourceLine}.";
                    }
                    else if (resolvedIdentifier.Token != null)
                    {
                        additional = $" First declared: line {resolvedIdentifier.Token.Line}.";
                    }
                    m_errors.SymanticError(
                        context.Start.Line,
                        context.Start.Column,
                        false,
                        $"Illegal to declare variable with same name ('{m_variableName}') as another element or type. " + additional);

                    m_variableInitializer = null;   // The variable should not be created, when there is an error.
                }

                if (m_variableInitializer != null)
                {
                    m_variables.Add(
                        new VariableData(
                            m_variableName,
                            m_variableType,
                            m_variableInitializer));
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
            var levelData = m_expressionData.PopStackLevel();

            // If stack.Count is 0, then there is no body and we do not initialize to anything
            if (levelData.Stack.Count != 0)
            {
                m_variableInitializer = levelData.Stack.Pop();
                m_variableInitializer = this.ResolveForGetOperation(m_variableInitializer, targetType: m_variableType, reportIfUnresolved: true);
            }
        }
    }
}
