using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using StepBro.Core.Data;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private int m_lambdaParentRule = 0;
        private List<Tuple<Type, string>> m_lambdaTypedParameters = null;
        private SBExpressionData m_lambdaLeftExpression = null;
        private Type m_lambdaDelegateTargetType = null;
        private List<Tuple<string, Type>> m_lambdaDelegateGenericArguments = null;
        private Type m_lambdaDelegateReturnType;

        public override void EnterLambdaExpression([NotNull] SBP.LambdaExpressionContext context)
        {
            m_lambdaDelegateTargetType = null;
            m_lambdaDelegateGenericArguments = null;
            m_lambdaDelegateReturnType = null;
            m_lambdaLeftExpression = null;
            m_lambdaParentRule = context.Parent.Parent.RuleIndex;
            m_lambdaTypedParameters = null;

            switch (m_lambdaParentRule)
            {
                case SBP.RULE_argument:
                    m_lambdaLeftExpression = m_leftOfMethodCallExpression;
                    break;
                case SBP.RULE_variableInitializer:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void ExitLambdaSimpleParameter([NotNull] SBP.LambdaSimpleParameterContext context)
        {
            if (m_lambdaParentRule != SBP.RULE_argument)
            {
                throw new NotSupportedException();
            }
            this.SetupUntypedLambdaParameters(context.GetText());
        }

        public override void ExitLambdaMoreParameters([NotNull] SBP.LambdaMoreParametersContext context)
        {
            if (m_lambdaParentRule != SBP.RULE_argument)
            {
                throw new NotSupportedException();
            }
            throw new NotImplementedException();
            //this.SetupLambdaParameters();
        }

        public override void ExitLambdaTypedParameters([NotNull] SBP.LambdaTypedParametersContext context)
        {
            m_lambdaTypedParameters = m_parameters.Select(pt => new Tuple<Type, string>(pt.Type.Type, pt.Name)).ToList();
            this.SetupTypedLambdaParameters();
        }

        public override void ExitLambdaNoParameters([NotNull] SBP.LambdaNoParametersContext context)
        {
            if (m_lambdaParentRule == SBP.RULE_argument)
            {
                this.SetupUntypedLambdaParameters();
            }
            else if (m_lambdaParentRule == SBP.RULE_variableInitializer)
            {
                m_lambdaTypedParameters = new List<Tuple<Type, string>>();
                this.SetupTypedLambdaParameters();
            }
        }

        private void SetupTypedLambdaParameters()
        {
            if (m_variableType == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (m_variableType.Type != typeof(VarSpecifiedType))
                {
                    if (m_variableType.Type.IsDelegate())
                    {
                        throw new NotSupportedException("TODO: Parser error");
                    }
                    m_lambdaDelegateTargetType = m_variableType.Type;
                }
                var scope = new ProcedureParsingScope(m_scopeStack.Peek(), "Lambda", ProcedureParsingScope.ScopeType.Lambda);
                m_scopeStack.Push(scope);

                foreach (var p in m_lambdaTypedParameters)
                {
                    scope.AddLambdaExpressionParameter(p.Item1, p.Item2);
                }
            }
        }

        private struct PrivateMethodInfoForLambdaResolver
        {
            public MethodInfo Method;
            public List<Tuple<string, Type>> MethodGenericArguments;
            public int ParameterIndex;
            public List<Type> LambdaParameterTypes;
            public Type DelegateType;
            public Type DelegateReturnType;
            public PrivateMethodInfoForLambdaResolver(MethodInfo method, int parIndex)
            {
                Method = method;
                MethodGenericArguments = null;
                ParameterIndex = parIndex;
                LambdaParameterTypes = null;
                DelegateType = null;
                DelegateReturnType = null;
            }
        }

        private void SetupUntypedLambdaParameters(params string[] parameterNames)
        {
            if (m_lambdaLeftExpression.IsMethodReference)
            {
                var possibleMethods = m_lambdaLeftExpression.GetMethods().ToArray();
                var methods = new List<PrivateMethodInfoForLambdaResolver>();
                var instance = m_lambdaLeftExpression.ExpressionCode;

                // Find methods where this parameter is a delegate that have the right number of parameters.
                foreach (var method in possibleMethods)
                {
                    bool matching = false;
                    bool isExtension = method.IsExtension();
                    var parameterIndex = m_argumentIndex + (isExtension ? 1 : 0);
                    var parameters = method.GetParameters();
                    var parTypes = new List<Type>();
                    var resolverMethodData = new PrivateMethodInfoForLambdaResolver(method, parameterIndex);

                    if (parameterIndex < parameters.Length)
                    {
                        var parameter = parameters[parameterIndex];
                        var targetType = parameter.ParameterType;
                        if (targetType.IsDelegate())
                        {
                            var invokeMethod = targetType.GetMethod("Invoke");
                            var targetParameters = invokeMethod.GetParameters();
                            var returnType = invokeMethod.ReturnType;
                            resolverMethodData.DelegateReturnType = returnType;
                            if (parameterNames.Length == targetParameters.Length)
                            {
                                matching = true;
                                if (targetType.HasGenericArguments())       // Only possible if at the same time is an extension method. (!!) 
                                {
                                    var mga = method.GetGenericArguments().Select(t => new Tuple<string, Type>(t.Name, t)).ToList();
                                    resolverMethodData.MethodGenericArguments = mga;

                                    // Resolve those generic arguments from the instance.
                                    var typedArgs = instance.Type.GetTypedGenericArguments(parameters[0].ParameterType);
                                    foreach (var ti in typedArgs)
                                    {
                                        var index = mga.FindIndex(t => t.Item1 == ti.Item1);
                                        mga[index] = new Tuple<string, Type>(mga[index].Item1, ti.Item2);
                                    }

                                    // Get types of delegate input parameters.
                                    for (int i = 0; i < parameterNames.Length; i++)
                                    {
                                        var t = targetParameters[i].ParameterType;
                                        if (t.IsGenericParameter)
                                        {
                                            var index = mga.FindIndex(tt => tt.Item1 == t.Name);
                                            if (index >= 0)
                                            {
                                                t = mga[index].Item2;
                                            }
                                            else
                                            {
                                                // The type of this input parameter was not found.
                                                matching = false;
                                                break;
                                            }
                                        }
                                        parTypes.Add(t);
                                    }
                                    if (matching && returnType.IsGenericParameter)
                                    {
                                        var nUnknownTypes = mga.Count(tt => tt.Item2.IsGenericParameter);
                                        if (nUnknownTypes > 1) matching = false;
                                        else if (nUnknownTypes == 1)
                                        {
                                            matching = mga.Exists(tt => tt.Item2 == returnType);
                                        }
                                    }
                                    resolverMethodData.LambdaParameterTypes = parTypes;
                                }
                                else
                                {
                                    resolverMethodData.LambdaParameterTypes =
                                        parameterNames.Select((n, i) => targetParameters[i].ParameterType).ToList();
                                    resolverMethodData.DelegateType = targetType;
                                }
                            }
                        }
                    }
                    if (matching)
                    {
                        methods.Add(resolverMethodData);
                    }
                    else
                    {
                        m_lambdaLeftExpression.RemoveMethod(method);    // Remove the method as a possible match, to avoid further processing.
                    }
                }

                if (methods.Count == 1)
                {
                    var methodData = methods[0];
                    var method = methodData.Method;

                    var scope = new ProcedureParsingScope(m_scopeStack.Peek(), "Lambda", ProcedureParsingScope.ScopeType.Lambda);
                    m_scopeStack.Push(scope);

                    for (int i = 0; i < parameterNames.Length; i++)
                    {
                        scope.AddLambdaExpressionParameter(methodData.LambdaParameterTypes[i], parameterNames[i]);
                    }

                    m_lambdaDelegateTargetType = methodData.DelegateType;
                    m_lambdaDelegateReturnType = methodData.DelegateReturnType;
                    m_lambdaDelegateGenericArguments = methodData.MethodGenericArguments;
                }
                else
                {
                    throw new NotImplementedException("Having more than one matching methods is not supported. Maybe it never will, and this should just be a parsing error.");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        //private bool IsMethodMatching

        public override void ExitLambdaExpression([NotNull] SBP.LambdaExpressionContext context)
        {
            var lambdaExpressionCode = m_expressionData.Pop();

            var scope = m_scopeStack.Pop();
            var parameters = new List<ParameterExpression>(scope.GetLambdaParameters());
            var delegateType = m_lambdaDelegateTargetType;


            if (m_lambdaParentRule == SBP.RULE_argument)
            {
                // Udfordringen: finde typen, som typisk er parameter til metoden som kaldes.
                // Dette er faktisk nødvendigt for at kende data-typen af parametrene til denne lambda-expression.

                // Det bør være sådan at ALLE input-parametre til delegate (lambda expression) er kendte, så en eventuelt
                // ukendt type for retur parameter kan bestemmes ved at parse lambda expression.
                // Det er derfor nødvendigt at der i denne parsing kan ignoreres at retur typen er ukendt.


                //m_scopeStack.Peek().AddStatementCode();

                if (delegateType == null)
                {
                    var methodGenericTypedef = m_lambdaLeftExpression.GetMethods().First();
                    var returnType = lambdaExpressionCode.DataType;
                    int i = m_lambdaDelegateGenericArguments.FindIndex(tt => tt.Item2 == m_lambdaDelegateReturnType);
                    if (i >= 0)
                    {
                        m_lambdaDelegateGenericArguments[i] = new Tuple<string, Type>(m_lambdaDelegateGenericArguments[i].Item1, returnType.Type);
                        var method = methodGenericTypedef.MakeGenericMethod(m_lambdaDelegateGenericArguments);
                        m_lambdaLeftExpression.RemoveMethod(methodGenericTypedef);
                        m_lambdaLeftExpression.AddMethod(method);
                        bool isExtension = method.IsExtension();
                        var parameterIndex = m_argumentIndex + (isExtension ? 1 : 0);
                        delegateType = method.GetParameters()[parameterIndex].ParameterType;
                    }
                }

            }
            else if (m_lambdaParentRule == SBP.RULE_variableInitializer)
            {
                if (delegateType == null)
                {
                    var returnType = lambdaExpressionCode.DataType;
                    delegateType = Expression.GetDelegateType(
                        parameters.Select(p => p.Type).Concat(returnType.Type).ToArray());
                }
                else
                {
                    // TODO: Check the parameters.
                }

            }

            var lambda = Expression.Lambda(delegateType, lambdaExpressionCode.ExpressionCode, true, parameters);
            m_expressionData.Push(new SBExpressionData(lambda));
        }
    }
}
