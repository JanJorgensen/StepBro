//#define PRINT_TREE

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Newtonsoft.Json.Serialization;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener : StepBro.Core.Parser.Grammar.StepBroBaseListener
    {
        private enum ElementType { File, FileElement, ProcedureStatement }

        protected ErrorCollector m_errors;
        protected Api.IAddonManager m_addonManager;
        protected ScriptFile m_file;
        private FileElement m_currentFileElement = null;    // The file element currently being parsed.
        private Stack<ElementType> m_currentElementType = new Stack<ElementType>();
        protected AccessModifier m_fileElementModifier = AccessModifier.None;
        protected IToken m_elementStart = null;
        protected string m_name = null;
        protected string m_currentNamespace = null;
        private List<PropertyBlockEntry> m_fileElementAttributes = null;
        protected FileTestList m_currentTestList = null;
        protected Stack<SBExpressionData> m_testListEntryArguments = null;
        //private SBExpressionData m_overrideVariable = null;
        private SBExpressionData m_fileElementReference = null;

        public StepBroListener(ErrorCollector errorCollector)
        {
            m_errors = errorCollector;
            m_addonManager = null;
            m_file = null;
        }

        public StepBroListener(ErrorCollector errorCollector, Api.IAddonManager addonManager, ScriptFile file)
        {
            m_errors = errorCollector;
            m_addonManager = addonManager;
            m_file = file;
            m_currentNamespace = file?.Namespace;
        }

        public string Namespace { get { return m_file.Namespace; } }

        public override void ExitFileProperties([NotNull] SBP.FilePropertiesContext context)
        {
            var props = this.PopPropertyBlockData();
            if (m_file != null)
            {
                m_file.AddFileProperties(props);
            }
            base.ExitFileProperties(context);
        }

        public override void EnterUsingDeclarationWithIdentifier([NotNull] SBP.UsingDeclarationWithIdentifierContext context)
        {
            m_expressionData.PushStackLevel("Using identifier");
        }

        public override void ExitUsingDeclarationWithIdentifier([NotNull] SBP.UsingDeclarationWithIdentifierContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            if (!m_file.TypeScanIncluded)
            {
                var identifierExpression = stack.Pop();
                if (identifierExpression.IsUnresolvedIdentifier)
                {
                    var identifier = (string)identifierExpression.Value;
                    // TODO: check the identifier
                    m_file.AddNamespaceUsing(context.Start.Line, identifier);
                }
            }
        }

        public override void EnterTypeAlias([NotNull] SBP.TypeAliasContext context)
        {
            m_expressionData.PushStackLevel("Using identifier alias");
        }

        public override void ExitTypeAlias([NotNull] SBP.TypeAliasContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            if (!m_file.TypeScanIncluded)
            {
                var identifierExpression = stack.Pop();
                if (identifierExpression.IsUnresolvedIdentifier)
                {
                    var identifier = (string)identifierExpression.Value;
                    // TODO: check the identifier
                    m_file.AddNamespaceUsing(context.Start.Line, identifier);
                    throw new NotImplementedException();
                }
            }
        }

        public override void EnterUsingDeclarationWithPath([NotNull] SBP.UsingDeclarationWithPathContext context)
        {
        }

        public override void ExitUsingDeclarationWithPath([NotNull] SBP.UsingDeclarationWithPathContext context)
        {
            if (!m_file.TypeScanIncluded)
            {
                var path = context.GetChild(context.ChildCount - 2).GetText();
                path = ParseStringLiteral(path, context);
                // TODO: check the path
                m_file.AddFileUsing(context.Start.Line, false, path);
            }
        }

        public override void EnterNamespace([NotNull] SBP.NamespaceContext context)
        {
            this.PrepareForExpressionParsing("namespace");
        }

        public override void ExitNamespace([NotNull] SBP.NamespaceContext context)
        {
            var nsExpression = this.GetExpressionResultUnresolved();
            m_currentNamespace = (string)nsExpression.Value;
        }

        public override void EnterFileElement([NotNull] SBP.FileElementContext context)
        {
            m_lastElementPropertyBlock = null;
            m_fileElementModifier = AccessModifier.Public;    // Default is 'public'.
            m_currentFileElement = null;

            m_fileElementAttributes = m_lastAttributes;
            m_lastAttributes = null;

#if (PRINT_TREE)
            m_indent = m_indent.Substring(0, m_indent.Length - 4) + "|   ";
#endif
        }

        public override void ExitFileElement([NotNull] SBP.FileElementContext context)
        {
            if (m_fileElementAttributes != null)
            {
                var summary = m_fileElementAttributes.FirstOrDefault(e => e.BlockEntryType == PropertyBlockEntryType.Value && e.Name == "Summary" && (e as PropertyBlockValue).IsStringOrIdentifier) as PropertyBlockValue;
                if (summary != null)
                {
                    m_currentFileElement.Summary = summary.ValueAsString();
                }
                var reference = m_fileElementAttributes.FirstOrDefault(e => e.BlockEntryType == PropertyBlockEntryType.Value && e.Name == "Reference" && (e as PropertyBlockValue).IsStringOrIdentifier) as PropertyBlockValue;
                if (summary != null)
                {
                    m_currentFileElement.DocReference = reference.ValueAsString();
                }
            }
        }

        public override void EnterElementModifier([NotNull] SBP.ElementModifierContext context)
        {
            var modifier = context.GetText();
            if (modifier == "public") m_fileElementModifier = AccessModifier.Public;
            else if (modifier == "private") m_fileElementModifier = AccessModifier.Private;
            else if (modifier == "protected") m_fileElementModifier = AccessModifier.Protected;
            else throw new NotImplementedException();
        }

        public override void EnterTypedef([NotNull] SBP.TypedefContext context)
        {
            //var fileElement = new FileElementTypeDef(m_file, context.Start.Line, )
            m_currentFileElement = new FileElementTypeDef(m_file, context.Start.Line, m_currentNamespace, "");
            m_name = null;
        }

        public override void ExitTypedef([NotNull] SBP.TypedefContext context)
        {
            m_currentFileElement.SetName(m_currentNamespace, m_name);
        }

        public override void ExitTypedefName([NotNull] SBP.TypedefNameContext context)
        {
            m_name = context.GetText();
        }

        public override void EnterTypedefType([NotNull] SBP.TypedefTypeContext context)
        {
            m_expressionData.PushStackLevel("TypeDefOuterType");
        }

        public override void ExitTypedefType([NotNull] SBP.TypedefTypeContext context)
        {
            m_expressionData.PopStackLevel();
            var type = m_typeStack.Pop();

            // TODO: Set the type in the FileElementTypedef
        }

        public override void ExitTypeOverride([NotNull] SBP.TypeOverrideContext context)
        {
            // TODO: set the override type for the current file element (variable). Use m_name.
        }

        public override void EnterFileVariableWithPropertyBlock([NotNull] SBP.FileVariableWithPropertyBlockContext context)
        {
            m_variableModifier = VariableModifier.Static;
        }

        public override void EnterFileVariableSimple([NotNull] SBP.FileVariableSimpleContext context)
        {
            m_variableModifier = VariableModifier.Static;
            this.CreateVariablesList();
        }

        public override void ExitFileVariableSimple([NotNull] SBP.FileVariableSimpleContext context)
        {
            System.Diagnostics.Debug.Assert(m_variables.Count == 1);
            TypeReference type = m_variableType;
            var variable = m_variables[0];
            if (type == (TypeReference)typeof(VarSpecifiedType))
            {
                type = variable.Type;
            }
            if (!type.Type.IsAssignableFrom(variable.Initializer.DataType.Type))
            {
                throw new NotImplementedException("Variable assignment of incompatible type.");
            }
            var codeHash = context.GetText().GetHashCode();
            var id = m_file.CreateOrGetFileVariable(
                m_currentNamespace, m_fileElementModifier, variable.Name, type, false,
                context.Start.Line, context.Start.Column, codeHash,
                CreateVariableContainerValueAssignAction(variable.Initializer.ExpressionCode));
            m_file.SetFileVariableModifier(id, m_fileElementModifier);
        }

        public override void ExitFileVariableWithPropertyBlock([NotNull] SBP.FileVariableWithPropertyBlockContext context)
        {
            if (m_variableType == null || m_creatorType == null)
            {
                return;
            }
            var args = (context.children.Any(child => child is SBP.CtorArgumentsContext)) ? m_arguments.Pop() : null;
            var props = m_lastElementPropertyBlock;
            VariableContainerAction createAction = null;
            VariableContainerAction initAction = null;
            VariableContainerAction resetAction = null;

            if (m_variableType.Type.IsValueType || m_variableType.Type == typeof(string))
            {
                var code = Expression.Constant(Activator.CreateInstance(m_creatorType.Type), m_creatorType.Type);
                createAction = CreateVariableContainerValueAssignAction(code);
                resetAction = createAction;
            }
            else
            {
                #region Creator Action

                //if (args != null && args.Count > 0)
                //{
                //}
                //else
                {
                    // First check if there's a ctor with object name as parameter.
                    var ctor = m_creatorType.Type.GetConstructor(new Type[] { typeof(string) });
                    if (ctor != null && ObjectNameAttribute.IsObjectName(ctor.GetParameters()[0]))
                    {
                        var code = Expression.New(ctor, Expression.Constant(m_variableName));
                        createAction = CreateVariableContainerValueAssignAction(code);
                    }
                    else
                    {
                        // else, try using the default ctor.
                        ctor = m_creatorType.Type.GetConstructor(new Type[] { });
                        if (ctor != null)
                        {
                            var code = Expression.New(ctor);
                            createAction = CreateVariableContainerValueAssignAction(code);
                        }
                        else
                        {
                            var createMethods =
                                m_creatorType.Type.GetMethods().Where(
                                    m => String.Equals(m.Name, "Create", StringComparison.InvariantCulture) && m.IsStatic).ToArray();
                            throw new NotImplementedException();
                        }
                    }
                }

                #endregion

                #region Reset Action
                var resettable = m_creatorType.Type.GetInterface(nameof(IResettable));
                if (resettable != null)
                {
                    LabelTarget returnLabel = Expression.Label(typeof(bool));

                    var parameterContainer = Expression.Parameter(typeof(IValueContainerOwnerAccess), "container");
                    var parameterLogger = Expression.Parameter(typeof(ILogger), "logger");

                    var callSetValue = Expression.Call(
                        typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.ResetFileVariable), new Type[] { typeof(IValueContainerOwnerAccess), typeof(ILogger) }),
                        parameterContainer,
                        parameterLogger);

                    var lambdaExpr = Expression.Lambda(
                        typeof(VariableContainerAction),
                        Expression.Block(
                            callSetValue,
                            Expression.Label(returnLabel, Expression.Constant(true))),
                        parameterContainer,
                        parameterLogger);

                    var @delegate = lambdaExpr.Compile();
                    resetAction = (VariableContainerAction)@delegate;
                }
                #endregion
            }

            PropertyBlock customProperties = null;
            if (props != null && props.Count > 0)
            {
                initAction = this.CreateVariableContainerObjectInitAction(m_creatorType.Type, props, m_errors, context.Start);
                if (props.Count(e => e.Tag == null) > 0)
                {
                    customProperties = new PropertyBlock(context.Start.Line);
                    customProperties.AddRange(props.Where(e => e.Tag == null));
                }
            }

            var codeHash = context.GetText().GetHashCode();
            var id = m_file.CreateOrGetFileVariable(
                m_currentNamespace, m_fileElementModifier, m_variableName, m_variableType, true,
                context.Start.Line, context.Start.Column, codeHash,
                resetter: resetAction,
                creator: createAction,
                initializer: initAction,
                fileSetupData: props,
                customSetupData: customProperties);
            m_file.SetFileVariableModifier(id, m_fileElementModifier);

            m_variableName = null;
        }

        internal static VariableContainerAction CreateVariableContainerValueAssignAction(Expression initExpression)
        {
            try
            {
                LabelTarget returnLabel = Expression.Label(typeof(bool));

                var parameterContainer = Expression.Parameter(typeof(IValueContainerOwnerAccess), "container");
                var parameterLogger = Expression.Parameter(typeof(ILogger), "logger");

                var callSetValue = Expression.Call(
                    parameterContainer,
                    typeof(IValueContainerOwnerAccess).GetMethod("SetValue", new Type[] { typeof(object), typeof(ILogger) }),
                    Expression.Convert(initExpression, typeof(object)),
                    parameterLogger);

                var lambdaExpr = Expression.Lambda(
                    typeof(VariableContainerAction),
                    Expression.Block(
                        callSetValue,
                        Expression.Label(returnLabel, Expression.Constant(true))),
                    parameterContainer,
                    parameterLogger);

                var @delegate = lambdaExpr.Compile();
                return (VariableContainerAction)@delegate;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal VariableContainerAction CreateVariableContainerObjectInitAction(
            Type objectType, PropertyBlock properties, ErrorCollector errors, IToken startToken)
        {
            VariableContainerAction action = null;
            var effectiveProperties = properties;

            var resolver = new DefaultContractResolver();
            var contract = resolver.ResolveContract(objectType) as JsonObjectContract;

            var dataSetters = new List<Expression>();
            var objectReference = Expression.Variable(objectType);

            var deviceEntry = properties.TryGetElement(Constants.VARIABLE_DEVICE_REFERENCE);
            if (deviceEntry != null && deviceEntry.BlockEntryType == PropertyBlockEntryType.Value && (deviceEntry as PropertyBlockValue).IsStringOrIdentifier)
            {
                var deviceName = (deviceEntry as PropertyBlockValue).ValueAsString();
                deviceEntry.IsUsedOrApproved = true;
                var devices = StationPropertiesHelper.TryGetStationPropertiesDeviceData();
                if (devices != null)
                {
                    var deviceProps = devices.TryGetDeviceFromStationProperties(deviceName);
                    if (deviceProps != null)
                    {
                        effectiveProperties = deviceProps.MergeStationPropertiesWithLocalProperties(properties);
                    }
                }
            }

            foreach (var entry in effectiveProperties)
            {
                if (entry.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueEntry = entry as PropertyBlockValue;
                    if (String.IsNullOrEmpty(valueEntry.SpecifiedTypeName))
                    {
                        var objectProperty = contract.Properties.FirstOrDefault(p => String.Equals(entry.Name, p.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                        if (objectProperty != null)
                        {
                            entry.Tag = "Property";
                            bool dataError = false;
                            System.Diagnostics.Debug.WriteLine($"Property type: {objectProperty.PropertyType.Name}");
                            Expression valueExpression = null;
                            object value = valueEntry.Value;
                            if (value != null)
                            {
                                if (objectProperty.PropertyType.IsEnum)
                                {
                                    if (value is Identifier)
                                    {
                                        value = ((Identifier)value).Name;
                                    }
                                    if (value is string)
                                    {
                                        if (((string)value).StartsWith(objectProperty.PropertyType.Name + ".", StringComparison.InvariantCulture))
                                        {
                                            value = ((string)value).Substring(objectProperty.PropertyType.Name.Length + 1);
                                        }
                                        try
                                        {
                                            value = Enum.Parse(objectProperty.PropertyType, (string)value);
                                        }
                                        catch
                                        {
                                            dataError = true;
                                            errors.SymanticError(startToken.Line, startToken.Column, false, $"Unknown value for property \"{entry.Name}\".");
                                        }
                                    }
                                    else if (value is long)
                                    {

                                    }
                                    else
                                    {
                                        dataError = true;
                                        errors.SymanticError(startToken.Line, startToken.Column, false, $"Unsupported value for property \"{entry.Name}\".");
                                    }
                                }
                                else if (objectProperty.PropertyType == typeof(string))
                                {
                                    if (value is Identifier)
                                    {
                                        value = ((Identifier)value).Name;
                                    }
                                }
                                else if (!objectProperty.PropertyType.IsPrimitive && objectProperty.PropertyType != typeof(TimeSpan) && objectProperty.PropertyType != typeof(DateTime))
                                {
                                    if (value is Identifier)
                                    {
                                        value = ((Identifier)value).Name;
                                        var resolved = this.ResolveIdentifierForGetOperation(
                                            (string)value,
                                            false,
                                            new TypeReference(objectProperty.PropertyType));
                                        if (resolved != null)
                                        {
                                            if (objectProperty.PropertyType.IsAssignableFrom(resolved.DataType.Type))
                                            {
                                                valueExpression = resolved.ExpressionCode;
                                            }
                                            else
                                            {
                                                dataError = true;
                                                errors.SymanticError(startToken.Line, startToken.Column, false, $"Unresolved value for property \"{entry.Name}\": '{(string)value}'.");
                                            }
                                        }
                                        else
                                        {
                                            dataError = true;
                                            errors.SymanticError(startToken.Line, startToken.Column, false, $"Unresolved value for property \"{entry.Name}\": '{(string)value}'.");
                                        }
                                    }
                                    else
                                    {
                                        dataError = true;
                                        errors.SymanticError(startToken.Line, startToken.Column, false, $"Unsupported value type for property \"{entry.Name}\": '{(string)value.GetType().Name}'.");
                                    }
                                }
                            }
                            if (!dataError)
                            {
                                var vt = (valueExpression != null) ? valueExpression.Type : value?.GetType();
                                if (vt != null && !objectProperty.PropertyType.IsAssignableFrom(vt))
                                {
                                    errors.IncompatibleDataType(startToken.Line, startToken.Column, vt.Name, objectProperty.PropertyType.Name);
                                }
                                else
                                {
                                    if (valueExpression == null)
                                    {
                                        valueExpression = Expression.Constant(value, objectProperty.PropertyType);
                                    }
                                    dataSetters.Add(Expression.Assign(
                                        Expression.Property(objectReference, objectProperty.UnderlyingName),
                                        valueExpression));
                                }
                            }
                        }
                        else
                        {
                            errors.SymanticError(startToken.Line, startToken.Column, false, $"The object has no property named \"{entry.Name}\".");
                        }
                    }
                    else
                    {
                        errors.InternalError(startToken.Line, startToken.Column, $"Element type is not expected (entry \"{entry.Name}\").");
                    }
                }
                //else if (entry.BlockEntryType == PropertyBlockEntryType.Block)
                //{
                //    var blockEntry = entry as PropertyBlock;
                //    var objectProperty = contract.Properties.FirstOrDefault(p => String.Equals(entry.Name, p.PropertyName, StringComparison.InvariantCultureIgnoreCase) && p.GetType() == typeof(PropertyBlock));
                //    if (objectProperty != null)
                //    {
                //        HVOR SKAL DENNE BLOCK GEMMES, SÅ DEN KAN GIVES TIL OBJEKTET EFTER INSTANTIERING ????
                //    }
                //    else 
                //    {
                //        errors.SymanticError(startToken.Line, startToken.Column, false, $"The object has no property named \"{entry.Name}\".");
                //    }
                //}
                else
                {
                    // Not handled yet; just let it fall through.
                }
            }

            bool isSettableFromPropertyBlock = objectType.GetInterface(nameof(ISettableFromPropertyBlock)) != null;

            if (dataSetters.Count > 0 || isSettableFromPropertyBlock)
            {
                try
                {
                    LabelTarget returnLabel = Expression.Label(typeof(bool));
                    var parameterContainer = Expression.Parameter(typeof(IValueContainerOwnerAccess), "container");
                    var parameterLogger = Expression.Parameter(typeof(ILogger), "logger");
                    var getContainerReference = Expression.Property(
                        parameterContainer,
                        typeof(IValueContainerOwnerAccess).GetProperty(nameof(IValueContainerOwnerAccess.Container)));
                    var getObjectReference = Expression.Call(
                        getContainerReference,
                        typeof(IValueContainer).GetMethod(nameof(IValueContainer.GetValue), new Type[] { typeof(ILogger) }),
                        parameterLogger);
                    var objectReferenceAssignment = Expression.Assign(
                        objectReference,
                        Expression.Convert(getObjectReference, objectType));
                    var expressions = new List<Expression>(dataSetters);
                    expressions.Insert(0, objectReferenceAssignment);

                    if (isSettableFromPropertyBlock)
                    {
                        var setterHelper = typeof(ExecutionHelperMethods).GetMethod(
                            nameof(ExecutionHelperMethods.SetupObjectWithPropertyBlock));

                        var propertyBlockSetter = Expression.Call(setterHelper, parameterLogger, parameterContainer);
                        expressions.Add(propertyBlockSetter);
                    }

                    expressions.Add(Expression.Label(returnLabel, Expression.Constant(true)));

                    var lambdaExpr = Expression.Lambda(
                        typeof(VariableContainerAction),
                        Expression.Block(new ParameterExpression[] { objectReference }, expressions),
                        parameterContainer,
                        parameterLogger);
                    var @delegate = lambdaExpr.Compile();
                    action = (VariableContainerAction)@delegate;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return action;
        }

        #region TestList

        public override void EnterTestlist([NotNull] SBP.TestlistContext context)
        {
            m_currentTestList = new FileTestList(m_file, m_fileElementModifier, context.Start.Line, null, m_currentNamespace, "");
        }

        public override void ExitTestListName([NotNull] SBP.TestListNameContext context)
        {
            var name = context.GetText();
            m_currentTestList.SetName(m_currentNamespace, name);
            var existing = m_file.ListElements().Where(e => e.ElementType == FileElementType.TestList).FirstOrDefault(tl => tl.Name.Equals(name)) as FileTestList;
            if (existing != null)
            {
                existing.AccessLevel = m_currentTestList.AccessLevel;
                existing.Line = m_currentTestList.Line;

                m_currentTestList = existing;
            }
            else
            {
                m_file.AddElement(m_currentTestList);
            }
            m_currentFileElement = m_currentTestList;
        }

        public override void ExitTestlist([NotNull] SBP.TestlistContext context)
        {
            m_currentTestList = null;   // Clear, to better detect bugs.
        }

        public override void EnterTestListEntry([NotNull] SBP.TestListEntryContext context)
        {
            m_expressionData.PushStackLevel("TestListEntry");
            m_testListEntryArguments = null;
        }

        public override void ExitTestListEntry([NotNull] SBP.TestListEntryContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            var entryTarget = stack.Pop();
            var referenceName = "";
            if (entryTarget.IsUnresolvedIdentifier)
            {
                referenceName = (string)entryTarget.Value;
            }
            else
            {
                throw new NotImplementedException();
            }
            entryTarget = this.ResolveIfIdentifier(entryTarget, false);
            ITestListEntry entry = null;
            if (entryTarget.IsProcedureReference)
            {
                System.Diagnostics.Debug.Assert(entryTarget.Value != null && entryTarget.Value is IProcedureReference);
                var proc = entryTarget.Value as IProcedureReference;
                entry = m_currentTestList.AddTestCase(referenceName, proc, null);
            }
            else if (entryTarget.IsTestList)
            {
                var list = entryTarget.Value as FileTestList;
                entry = m_currentTestList.AddTestList(referenceName, list);
            }
            else
            {
                m_currentTestList.AddTestEntry((string)entryTarget.Value);
            }
            if (entry != null && m_testListEntryArguments != null)
            {
                foreach (var a in m_testListEntryArguments)
                {
                    entry.Arguments.Add(a.ParameterName, a.Value);
                }
            }
        }

        public override void ExitTestListEntryArguments([NotNull] SBP.TestListEntryArgumentsContext context)
        {
            m_testListEntryArguments = m_arguments.Pop();
        }

        #endregion

        public override void EnterOverrideReference([NotNull] SBP.OverrideReferenceContext context)
        {
            m_expressionData.PushStackLevel("ReferenceName");
        }

        public override void ExitOverrideReference([NotNull] SBP.OverrideReferenceContext context)
        {
            m_fileElementReference = m_expressionData.Peek().Pop();
            m_expressionData.PopStackLevel();

            m_currentFileElement.SetName(m_currentNamespace, m_fileElementReference.Value as string);
            var parent = this.ResolveIfIdentifier(
                m_fileElementReference,
                false,
                predicate: (IIdentifierInfo id) => (id.Type != IdentifierType.FileElement || !Object.ReferenceEquals(((FileElement)id).ParentFile, m_file)));
            if (!parent.IsResolved)
            {
                return;
            }

            IFileElement element = (parent.Value is IProcedureReference) ? (parent.Value as IProcedureReference).ProcedureData : parent.Value as FileElement;
            m_currentFileElement.BaseElement = element;
        }

        public override void EnterFileElementOverride([NotNull] SBP.FileElementOverrideContext context)
        {
            m_currentFileElement = new FileElementOverride(m_file, context.Start.Line, null, m_currentNamespace, "");
        }

        public override void ExitFileElementOverride([NotNull] SBP.FileElementOverrideContext context)
        {
            var existing = m_file.ListElements().Where(e => e.ElementType == FileElementType.Override).FirstOrDefault(tl => tl.Name.Equals(m_currentFileElement.Name)) as FileElementOverride;
            if (existing != null)
            {
                existing.Line = m_currentFileElement.Line;
                existing.BaseElement = m_currentFileElement.BaseElement;
                m_currentFileElement = existing;
            }
            else
            {
                m_file.AddElement(m_currentFileElement as FileElementOverride);
            }

            m_currentFileElement.BaseElementName = m_currentFileElement.Name;
            m_currentFileElement.ParseBaseElement();
            if (m_currentFileElement.BaseElement == null) return;

            if (m_currentFileElement.BaseElement.ElementType == FileElementType.FileVariable && m_lastElementPropertyBlock != null)
            {
                FileVariable fileVariable = m_currentFileElement.BaseElement as FileVariable;
                var parentProperties = ScriptFile.GetFileVariableAllData(fileVariable);
                var mergedProps = parentProperties.Merge(m_lastElementPropertyBlock);
                ScriptFile.SetFileVariableAllData(fileVariable, mergedProps);

                if (mergedProps != null && mergedProps.Count > 0)
                {
                    var initAction = this.CreateVariableContainerObjectInitAction(
                        fileVariable.VariableOwnerAccess.Container.DataType.Type, mergedProps, m_errors, context.Start);
                    fileVariable.VariableOwnerAccess.DataInitializer = initAction;
                    if (mergedProps.Count(e => e.Tag == null) > 0)
                    {
                        var customProperties = new PropertyBlock(context.Start.Line);
                        customProperties.AddRange(mergedProps.Where(e => e.Tag == null));
                        ScriptFile.SetFileVariableCustomData(fileVariable, customProperties);
                    }
                }
            }
            else
            {
                //throw new NotImplementedException();
            }
        }

#if (PRINT_TREE)
        private string m_indent = "";

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            System.Diagnostics.Debug.WriteLine(m_indent + "Enter" + this.ContextName(context) + ":             " + this.ShortContextText(context));
            m_indent += "    ";
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            m_indent = m_indent.Substring(0, m_indent.Length - 4);
            //Console.WriteLine(m_indent + "EXIT  - " + this.ContextName(context) + ":             " + this.ShortContextText(context));
        }

        private string ShortContextText(ParserRuleContext context)
        {
            string text = context.GetText();
            if (text.Length > 80)
            {
                text = text.Substring(0, 75) + " ... " + text.Substring(text.Length - 10);
            }
            return text;
        }

        private string ContextName(ParserRuleContext context)
        {
            string name = context.GetType().Name;
            return name.Substring(0, name.Length - "Context".Length);
        }
#endif

        public override void VisitErrorNode([NotNull] IErrorNode node)
        {
#if (PRINT_TREE)
            System.Diagnostics.Debug.WriteLine(m_indent + "ERROR - " + node.GetText());
#endif
            //var t = node.Payload as CommonToken;
            //if (t != null)
            //{
            //    m_errors.SyntaxError(null, null, t.Type, t.Line, t.Column, node.GetText(), null);
            //}
            //else
            //{
            //    m_errors.SyntaxError(null, null, -1, -1, -1, node.GetText(), null);
            //}
            base.VisitErrorNode(node);
        }
    }
}
