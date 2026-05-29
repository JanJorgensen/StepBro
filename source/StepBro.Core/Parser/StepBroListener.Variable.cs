using Antlr4.Runtime.Misc;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
        private bool m_isInVariableInitializer = false;
        private ParameterExpression m_variableInitializerParameterScriptFile = null;
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
                m_variableInitializer = AssignmentExpressionCreateCastOrConvertIfNeeded(context, m_variableInitializer, m_variableType);

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

        private SBExpressionData AssignmentExpressionCreateCastOrConvertIfNeeded([NotNull] Antlr4.Runtime.ParserRuleContext context, SBExpressionData expression, TypeReference targetType)
        {
            if (expression.IsError())
            {
                if (targetType.Type != typeof(VarSpecifiedType))
                {
                    object def = null;
                    if (targetType.Type.IsValueType)
                    {
                        def = Activator.CreateInstance(targetType.Type);
                    }
                    expression = SBExpressionData.Constant(targetType, def);
                }
                else
                {
                    // Just set the type to 'object' then.
                    expression = SBExpressionData.Constant(TypeReference.TypeObject, null);
                }
                return expression;
            }
            if (expression.IsConstant && expression.Value == null)
            {
                // Convert the null value to the type of the variable
                if (targetType.Type == typeof(string))
                {
                    expression = SBExpressionData.Constant(TypeReference.TypeString, null);
                }
            }
            else if (expression.IsAwaitExpression)
            {
                expression = new SBExpressionData(
                    this.MakeAwaitOperation(
                        expression.ExpressionCode, context, true, targetType.Type));
            }
            if (targetType.Type != typeof(VarSpecifiedType))
            {
                if (expression.IsValueType &&
                    expression.IsConstant &&
                    expression.Value == null)
                {
                    expression.NarrowGetValueType(targetType);
                }
                else if (targetType != expression.DataType && !targetType.Type.IsAssignableFrom(expression.DataType.Type))
                {
                    expression = this.CastProcedureAssignmentArgumentIfNeeded(targetType, expression);
                    if (expression.DataType.Type != targetType.Type)
                    {
                        if (expression.DataType.Type == typeof(object) && expression.SuggestsAutomaticTypeConversion)
                        {
                            expression = new SBExpressionData(
                                Expression.Convert(expression.ExpressionCode, targetType.Type));
                        }
                        else if (targetType.Type.IsPrimitive && 
                            targetType.Type.IsPrimitiveIntType() && 
                            expression.DataType.Type.IsPrimitive &&
                            expression.DataType.Type.IsPrimitiveIntType())
                        {
                            expression = new SBExpressionData(
                                Expression.Convert(expression.ExpressionCode, targetType.Type));
                        }
                        else
                        {
                            expression.NarrowGetValueType();
                            var useableAssignment = CheckAndConvertValueForAssignment(expression.ExpressionCode, targetType.Type);
                            if (useableAssignment != null)
                            {
                                expression = new SBExpressionData(useableAssignment);
                            }
                            else
                            {
                                if (context != null)
                                {
                                    string additional = " Value type: " + expression.DataType.Type.TypeNameSimple() + ".";
                                    if (expression.DataType.HasProcedureReference)
                                    {
                                        additional = " Value: procedure " + (expression.DataType.DynamicType as IFileProcedure).Name + ".";
                                    }
                                    var message = "Value is not compatible with the variable type." + additional;
                                    m_errors.SymanticError(
                                        context.Start.Line,
                                        context.Start.Column,
                                        false,
                                        message);
                                }
                                expression = null;
                            }
                        }
                    }
                }
            }
            return expression;
        }

        public override void ExitVariableInitializerArray([NotNull] SBP.VariableInitializerArrayContext context)
        {
            base.ExitVariableInitializerArray(context);
        }

        public override void EnterVariableInitializerExpression([NotNull] SBP.VariableInitializerExpressionContext context)
        {
            m_expressionData.PushStackLevel("VariableInitializerExpression @" + context.Start.Line.ToString() + ", " + context.Start.Column.ToString());
            m_isInVariableInitializer = true;
            m_variableInitializerParameterScriptFile = Expression.Parameter(typeof(IScriptFile), "file");
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
            m_isInVariableInitializer = false;
        }
    }
}
