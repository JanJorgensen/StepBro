﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Host;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser;

internal partial class StepBroListener
{
    private static MethodInfo s_GetFileConstant = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetFileConstant));
    private static MethodInfo s_GetGlobalVariable = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetFileVariable));
    private static MethodInfo s_GetHostVariable = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetHostVariable));
    private static MethodInfo s_GetProcedure = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetProcedure));
    private static MethodInfo s_GetProcedureTyped = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetProcedureTyped));
    private static MethodInfo s_PostProcedureCallResultHandling = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.PostProcedureCallResultHandling));
    private static MethodInfo s_GetFileElement = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetFileElement));
    private static MethodInfo s_DynamicProcedureCall = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicProcedureCall));
    private static MethodInfo s_DynamicFunctionCall = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.DynamicFunctionCall));
    private static MethodInfo s_CastProcedureReference = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.CastToSpecificProcedureType));
    private static MethodInfo s_GetPartnerFromProcedure = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetPartnerReferenceFromProcedureReference));
    private static MethodInfo s_GetPartner = typeof(ExecutionHelperMethods).GetMethod(nameof(ExecutionHelperMethods.GetPartnerReference));

    private static string s_ScriptUtilsFullNamePrefix = typeof(Execution.ScriptUtils).FullName + ".";
    private static SBExpressionData s_ScriptUtilsTypeData = new SBExpressionData(SBExpressionType.TypeReference, new TypeReference(typeof(Execution.ScriptUtils)));

    public override void ExitQualifiedName([NotNull] SBP.QualifiedNameContext context)
    {
        m_expressionData.Push(SBExpressionData.CreateIdentifier(context.GetText(), token: context.Start as CommonToken));
    }

    public override void ExitPrimaryOrQualified([NotNull] SBP.PrimaryOrQualifiedContext context)
    {
        this.PushIfIdentifier(context);
    }

    public override void ExitIdentifierOrQualified([NotNull] SBP.IdentifierOrQualifiedContext context)
    {
        this.PushIfIdentifier(context);
    }

    // Checks if the current context is an identifier, and pushes it to the expression stack.
    private void PushIfIdentifier(ParserRuleContext context)
    {
        if (context.ChildCount == 1)
        {
            var child = context.GetChild(0) as TerminalNodeImpl;
            if (child != null && child.Payload.Type == SBP.IDENTIFIER)
            {
                m_expressionData.Push(SBExpressionData.CreateIdentifier(context.GetText(), token: child.Payload as CommonToken));
            }
        }
    }

    public override void ExitKeyword([NotNull] SBP.KeywordContext context)
    {
        m_expressionData.Push(SBExpressionData.CreateIdentifier(context.GetText(), token: context.Start as CommonToken));
    }

    public override void ExitExpDotIdentifier([NotNull] SBP.ExpDotIdentifierContext context)
    {
        var left = m_expressionData.Pop();
        left = this.ResolveIfIdentifier(left, true);
        if (CheckExpressionsForErrors(context, left))
        {
            if (left.IsUnresolvedIdentifier || left.IsError())
            {
                m_expressionData.Push(new SBExpressionData(
                    SBExpressionType.ExpressionError, "Error parsing 'dot' operation.", context.GetText(), new TokenOrSection(context.Start, context.Stop, context.GetText())));
            }
            else
            {
                var child = context.GetChild(2) as TerminalNodeImpl;
                var identifier = SBExpressionData.CreateIdentifier(child.GetText(), token: child.Payload as CommonToken);
                var result = this.ResolveDotIdentifier(left, identifier);
                if (result == null)
                {
                    var payload = context.GetChild(2).Payload as CommonToken;
                    m_errors.SymanticError(payload.Line, payload.StartIndex, false, $"Unknown identifier: '{identifier.Value as string}'.");
                    result = new SBExpressionData(
                        SBExpressionType.ExpressionError, "Error parsing 'dot' operation.", context.GetText(), new TokenOrSection(context.Start, context.Stop, context.GetText()));
                }
                else if (result.IsError())
                {
                    var payload = context.GetChild(2).Payload as CommonToken;
                    m_errors.SymanticError(payload.Line, payload.StartIndex, false, result.Argument);
                }
                m_expressionData.Push(result);
            }
        }
    }

    public SBExpressionData ResolveIfIdentifier(
        SBExpressionData input,
        bool inFunctionScope,
        TypeReference targetType = null,
        Func<IIdentifierInfo, bool> predicate = null)
    {
        if (input.IsUnresolvedIdentifier)
        {
            string identifier = (string)input.Value;
            SBExpressionData result = this.ResolveQualifiedIdentifier(identifier, inFunctionScope, true, input.Token, predicate);
            if (result != null)
            {
                // Preserve some of the extra data from input.
                result.Token = input.Token;
                result.ParameterName = input.ParameterName;
                result.Argument = input.Argument;
                return result;
            }
            else
            {
                return input;
            }
        }
        return input;
    }

    public IIdentifierInfo TryGetProcedureParameter(string name)
    {
        if (m_inFunctionScope)
        {
            foreach (var p in m_currentProcedure.Parameters())
            {
                if (p.Name == name) return p;
            }
        }
        return null;
    }

    public IIdentifierInfo TryGetLocalVariable(string name)
    {
        if (m_scopeStack.Count > 0)
        {
            foreach (var scopeIdentifier in m_scopeStack.Peek().KnownIdentifiers())
            {
                if (scopeIdentifier.Name == name) return scopeIdentifier;
            }
        }
        return null;
    }

    public IEnumerable<IFileElement> ListLocalFileElements()
    {
        if (m_file != null)
        {
            foreach (var e in m_file.ListElements())
            {
                yield return e;
            }
        }
    }

    public IEnumerable<IFileElement> ListExternalFileElements()
    {
        foreach (var file in m_file.ListReferencedScriptFiles())
        {
            var minAccessLevel = AccessModifier.Public;
            if (String.Equals(file.Namespace, m_file.Namespace, StringComparison.InvariantCulture))
            {
                minAccessLevel = AccessModifier.Protected;  // When same namespace, the protected elements are also accessible.
            }
            foreach (var e in file.ListElements())
            {
                if (e.AccessLevel >= minAccessLevel) yield return e;
            }
        }
    }

    public IEnumerable<IFileElement> ListFileElementsInScope()
    {
        foreach (var e in this.ListLocalFileElements()) yield return e;
        foreach (var e in this.ListExternalFileElements()) yield return e;
    }

    public IFileElement TryGetFileElementInScope(IEnumerable<UsingData> usings, string name)
    {
        var elements = this.ListFileElementsInScope().ToList();   // Creating list for debugging purposes.
        return elements.FirstOrDefault(e => e.Name.Equals(name, StringComparison.InvariantCulture) && e.ElementType != FileElementType.Override);
    }

    public IFileElement TryGetFileElementInScope(string name)
    {
        return this.TryGetFileElementInScope(m_file?.Usings, name);
    }

    //[Obsolete]
    //public IFileProcedure TryGetProcedureFromDelegateType(Type type)
    //{
    //    var procedure = this.ListLocalFileElements().Where(e => e is IFileProcedure).FirstOrDefault(p => p.DataType.Type == type) as IFileProcedure;

    //    while (procedure != null && procedure.BaseElement != null)
    //    {
    //        procedure = procedure.BaseElement as IFileProcedure;
    //    }
    //    return procedure;
    //}

    public object TryResolveSymbol(IFileProcedure procedureScope, string symbol)
    {
        var resolved = this.ResolveQualifiedIdentifier(symbol, false);
        if (resolved == null)
        {
            resolved = this.ResolveQualifiedType(symbol);
        }
        if (resolved != null)
        {
            switch (resolved.ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                case SBExpressionType.Identifier:
                case SBExpressionType.GlobalVariableReference:
                case SBExpressionType.PropertyReference:
                case SBExpressionType.TestListReference:
                case SBExpressionType.FileElementOverride:
                case SBExpressionType.ProcedurePartner:
                    return resolved.Value;
                case SBExpressionType.TypeReference:
                    return resolved.DataType;
                case SBExpressionType.MethodReference:
                    return (resolved.Value != null && resolved.Value is List<MethodInfo> methods) ? methods[0] : null;
                case SBExpressionType.ThisReference:
                    return typeof(IProcedureThis);
                case SBExpressionType.ProcedureReference:
                    return ((IProcedureReference)resolved.Value).ProcedureData;

                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.Indexing:
                case SBExpressionType.GenericTypeDefinition:
                case SBExpressionType.ScriptNamespace:
                case SBExpressionType.ProcedurePropertyReference:
                case SBExpressionType.ProcedureCustomPropertyReference:
                case SBExpressionType.DatatableReference:
                case SBExpressionType.AwaitExpression:
                case SBExpressionType.DynamicObjectMember:
                case SBExpressionType.DynamicAsyncObjectMember:
                case SBExpressionType.ExpressionError:
                case SBExpressionType.UnknownIdentifier:
                case SBExpressionType.UnsupportedOperation:
                    break;
                default:
                    break;
            }
        }
        return null;
    }

    private SBExpressionData ResolveQualifiedIdentifier(
        string identifier,
        bool inFunctionScope,
        bool reportUnresolved = false,
        IToken token = null,
        Func<IIdentifierInfo, bool> predicate = null)
    {
        if (String.IsNullOrWhiteSpace(identifier)) throw new ArgumentException("Empty identifier string.");

        if (identifier.Contains('.'))
        {
            string[] parts = identifier.Split('.');
            int c = parts.Length;

            SBExpressionData result = this.ResolveSingleIdentifier(parts[0], inFunctionScope, reportUnresolved, token);
            if (result != null && !result.IsError())
            {
                for (int i = 1; i < c; i++)
                {
                    result = this.ResolveDotIdentifier(result, SBExpressionData.CreateIdentifier(parts[i], token: token));
                    if (result == null)
                    {
                        return null;
                    }
                }
            }
            return result;
        }
        else
        {
            return this.ResolveSingleIdentifier(identifier, inFunctionScope, reportUnresolved, token, predicate);
        }
    }

    private SBExpressionData ResolveSingleIdentifier(
        string identifier,
        bool inFunctionScope,
        bool reportUnresolved = false,
        IToken token = null,
        Func<IIdentifierInfo, bool> predicate = null)
    {
        IIdentifierInfo foundIdentifier = null;
        if (inFunctionScope)
        {
            foundIdentifier = this.TryGetLocalVariable(identifier);
            if (foundIdentifier != null) goto returnFound;
            foundIdentifier = this.TryGetProcedureParameter(identifier);
            if (foundIdentifier != null) goto returnFound;

            if (m_addonManager != null)
            {
                foundIdentifier = m_addonManager.Lookup(m_file?.Usings, identifier);
                if (foundIdentifier != null)
                {
                    return this.IdentifierToExpressionData(foundIdentifier, token);
                }
                var foundScriptUtilsAccess = this.ResolveDotIdentifierTypeReference(s_ScriptUtilsTypeData, SBExpressionData.CreateIdentifier(identifier, token: token));
                if (foundScriptUtilsAccess != null)
                {
                    return foundScriptUtilsAccess;
                }
            }
        }
        if (m_file != null)
        {
            if (String.Equals(identifier, m_file.Namespace, StringComparison.InvariantCulture))
            {
                return new SBExpressionData(SBExpressionType.ScriptNamespace, "", identifier, token);
            }
            else
            {
                foreach (var fu in m_file.ListResolvedFileUsings())
                {
                    if (String.Equals(identifier, fu.Namespace, StringComparison.InvariantCulture))
                    {
                        return new SBExpressionData(SBExpressionType.ScriptNamespace, "", identifier, token);
                    }
                }
            }

            var identifiers = m_file.LookupIdentifier(identifier, predicate);
            if (identifiers != null)
            {
                //if (identifiers.Count > 1)
                //{
                //    var selection = SelectOneIdentifier(identifiers);
                //    if (selection >= 0)
                //    {
                //        foundIdentifier = identifiers[selection];
                //    }
                //    else
                //    {
                //        throw new ParsingErrorException((token != null) ? token.Line : -1, identifier, "More than one alternative. ");
                //    }
                //}
                //else
                {
                    foundIdentifier = identifiers[0];       // NOTE: This might not always be the best solution.
                }
            }

            if (foundIdentifier == null)
            {
                foundIdentifier = this.TryGetFileElementInScope(m_file?.Usings, identifier);
            }
            //if (foundIdentifier == null && inFunctionScope)
            //{
            //    if (m_addonManager != null)
            //    {
            //        var foundScriptUtilsAccess = this.ResolveDotIdentifierTypeReference(s_ScriptUtilsTypeData, SBExpressionData.CreateIdentifier(identifier, token: token));
            //        if (foundScriptUtilsAccess != null)
            //        {
            //            return foundScriptUtilsAccess;
            //        }
            //    }
            //}
        }

        if (foundIdentifier == null)
        {
            var host = ServiceManager.GlobalIfReady?.Get<IHost>();
            if (host != null)
            {
                var hostObjectContainer = host.ListObjectContainers().FirstOrDefault(c => c.FullName == identifier);
                if (hostObjectContainer != null)
                {
                    foundIdentifier = new IdentifierInfo(
                        hostObjectContainer.FullName,
                        hostObjectContainer.FullName,
                        IdentifierType.HostVariable,
                        new TypeReference((hostObjectContainer.Object != null) ? hostObjectContainer.Object.GetType() : typeof(void)),
                        hostObjectContainer);
                }
            }
        }

    returnFound:
        if (foundIdentifier != null)
        {
            return this.IdentifierToExpressionData(foundIdentifier, token);
        }

        var foundType = this.ResolveSingleIdentifierType(identifier, false, token);
        if (foundType != null)
        {
            return foundType;
        }

        if (reportUnresolved)
        {
            m_errors.UnresolvedIdentifier(token, identifier);
        }
        return new SBExpressionData(SBExpressionType.UnknownIdentifier, "", identifier, token);
    }

    private SBExpressionData ResolveQualifiedType(string identifier, bool reportUnresolved = false, IToken token = null)
    {
        if (identifier.Contains('.'))
        {
            string[] parts = identifier.Split('.');
            int c = parts.Length;

            SBExpressionData result = this.ResolveSingleIdentifierType(parts[0], reportUnresolved, token);
            if (result != null)
            {
                for (int i = 1; i < c; i++)
                {
                    result = this.ResolveDotIdentifier(result, SBExpressionData.CreateIdentifier(parts[i], token: token));
                    if (result == null)
                    {
                        return null;
                    }
                }
                if (result != null)
                {
                    return result;
                }
            }
        }
        else
        {
            return this.ResolveSingleIdentifierType(identifier, reportUnresolved, token);
        }

        return null;
    }

    private SBExpressionData ResolveSingleIdentifierType(string identifier, bool reportUnresolved = false, IToken token = null)
    {
        IIdentifierInfo foundIdentifier = null;

        if (m_file != null)
        {
            // Search files, and skip override elements.
            var identifiers = m_file.LookupIdentifier(identifier, i => i.Type != IdentifierType.FileElement || ((FileElement)i).ElementType != FileElementType.Override);
            if (identifiers != null)
            {
                if (identifiers.Count > 1)
                {
                    return new SBExpressionData(
                        SBExpressionType.TypeReference,
                        (TypeReference)identifiers[0].DataType,
                        value: identifiers.Select(i => i.DataType.Type).ToList());
                }

                return this.IdentifierToExpressionData(identifiers[0], token);
            }

            foundIdentifier = this.TryGetFileElementInScope(m_file?.Usings, identifier);    // File elements can also act as types.
            if (foundIdentifier != null)
            {
                return this.IdentifierToExpressionData(foundIdentifier, token);
            }

            #region TBD
            foreach (var nsUsing in m_file.ListResolvedNamespaceUsings())
            {
                var foundViaUsing = this.ResolveDotIdentifier(nsUsing, SBExpressionData.CreateIdentifier(identifier, token: token));
                if (foundViaUsing != null)
                {
                    return foundViaUsing;
                }
            }
            #endregion
        }
        if (m_addonManager != null)
        {
            foundIdentifier = m_addonManager.Lookup(m_file?.Usings, identifier);
            if (foundIdentifier != null)
            {
                return this.IdentifierToExpressionData(foundIdentifier, token);
            }
        }

        if (reportUnresolved)
        {
            if (token != null)
            {
                m_errors.UnresolvedType(token.Line, token.Column, identifier);
            }
            else
            {
                m_errors.UnresolvedType(-1, -1, identifier);
            }
        }
        return null;
    }

    private SBExpressionData IdentifierToExpressionData(IIdentifierInfo identifier, IToken token = null)
    {
        SBExpressionData result = null;
        if (identifier.Type == IdentifierType.DotNetType)
        {
            result = new SBExpressionData(SBExpressionType.TypeReference, (TypeReference)identifier.DataType);
        }
        else if (identifier.Type == IdentifierType.DotNetMethod)
        {
            var methods = new List<MethodInfo>();
            methods.Add(identifier.Reference as MethodInfo);
            result = new SBExpressionData(
                SBExpressionType.MethodReference,   // Expression type
                null,                               // Data type
                null,                               // The instance expression
                methods,                            // The instance expression
                token: token);
        }
        else if (identifier.Type == IdentifierType.DotNetNamespace)
        {
            result = new SBExpressionData(SBExpressionType.Namespace, value: identifier.Reference);
        }
        else if (identifier.Type == IdentifierType.Variable || identifier.Type == IdentifierType.Parameter || identifier.Type == IdentifierType.LambdaParameter)
        {
            result = new SBExpressionData(SBExpressionType.LocalVariableReference, (TypeReference)identifier.DataType, (Expression)identifier.Reference, identifier);
        }
        else if (identifier.Type == IdentifierType.HostVariable)
        {
            var container = identifier.Reference as IObjectContainer;
            var getHostVariableTyped = s_GetHostVariable.MakeGenericMethod(container.Object.GetType());
            var context = (m_inFunctionScope) ? m_currentProcedure.ContextReferenceInternal : Expression.Constant(null, typeof(Execution.IScriptCallContext));
            result = new SBExpressionData(
                SBExpressionType.Expression,
                identifier.DataType,
                Expression.Call(
                    getHostVariableTyped,
                    context,
                    Expression.Constant(identifier.Name)),
                null,
                instanceName: identifier.Name);
        }
        else if (identifier.Type == IdentifierType.FileElement)
        {
            var element = identifier as IFileElement;
            switch (element.ElementType)
            {
                case FileElementType.Const:
                    {
                        var constantElement = element as FileConstant;
                        var getConstantTyped = s_GetFileConstant.MakeGenericMethod(constantElement.DataType.Type);
                        int fileID = (m_inFunctionScope && Object.ReferenceEquals(constantElement.ParentFile, m_file)) ? -1 : ((ScriptFile)constantElement.ParentFile).UniqueID;
                        var context = (m_inFunctionScope) ? m_currentProcedure.ContextReferenceInternal : Expression.Constant(null, typeof(Execution.IScriptCallContext));

                        result = new SBExpressionData(
                            SBExpressionType.Expression,
                            constantElement.DataType,
                            Expression.Call(
                                getConstantTyped,
                                context,
                                Expression.Constant(fileID),
                                Expression.Constant(constantElement.UniqueID)),
                            constantElement,                                                         // Make access to the file element, and thereby the declared type.
                            instanceName: identifier.Name);
                    }
                    break;
                case FileElementType.Config:
                    {
                        var configVariable = element as FileConfigValue;
                        var container = configVariable.VariableOwnerAccess.Container;
                        var containerType = typeof(IValueContainer<>).MakeGenericType(container.DataType.Type);
                        var getGlobalVariableTyped = s_GetGlobalVariable.MakeGenericMethod(container.DataType.Type);
                        var context = (m_inFunctionScope) ? m_currentProcedure.ContextReferenceInternal : Expression.Constant(null, typeof(Execution.IScriptCallContext));
                        result = new SBExpressionData(
                            SBExpressionType.GlobalVariableReference,
                            (TypeReference)containerType,
                            Expression.Call(
                                getGlobalVariableTyped,
                                context,
                                Expression.Constant((container as IValueContainer).UniqueID)),
                            configVariable,                                                         // Make access to the file element, and thereby the declared type.
                            instanceName: identifier.Name);
                    }
                    break;
                case FileElementType.ProcedureDeclaration:
                    {
                        var procedure = element as FileProcedure;
                        int fileID = Object.ReferenceEquals(procedure.ParentFile, m_file) ? -1 : ((ScriptFile)procedure.ParentFile).UniqueID;
                        var procGetterMethod = s_GetProcedure;
                        if (procedure.DelegateType != null)
                        {
                            procGetterMethod = s_GetProcedureTyped.MakeGenericMethod(procedure.DelegateType);
                        }

                        var getProc = Expression.Call(
                            procGetterMethod,
                            (m_inFunctionScope) ? m_currentProcedure.ContextReferenceInternal : Expression.Constant(null, typeof(Execution.IScriptCallContext)),
                            Expression.Constant(fileID),
                            Expression.Constant(procedure.UniqueID));

                        result = new SBExpressionData(
                            SBExpressionType.ProcedureReference,
                            procedure.DataType,
                            getProc,
                            identifier.Reference);
                    }
                    break;

                case FileElementType.FileVariable:
                    {
                        var fileVariable = element as FileVariable;
                        var container = fileVariable.VariableOwnerAccess.Container;
                        var containerType = typeof(IValueContainer<>).MakeGenericType(container.DataType.Type);
                        var getGlobalVariableTyped = s_GetGlobalVariable.MakeGenericMethod(container.DataType.Type);
                        var context = (m_inFunctionScope) ? m_currentProcedure.ContextReferenceInternal : Expression.Constant(null, typeof(Execution.IScriptCallContext));
                        result = new SBExpressionData(
                            SBExpressionType.GlobalVariableReference,
                            (TypeReference)containerType,
                            Expression.Call(
                                getGlobalVariableTyped,
                                context,
                                Expression.Constant((container as IValueContainer).UniqueID)),
                            fileVariable,                                                         // Make access to the file element, and thereby the declared type.
                            instanceName: identifier.Name);
                    }
                    break;

                case FileElementType.TestList:
                    {
                        var list = element as ITestList;
                        Expression getList = null;

                        if (m_inFunctionScope)
                        {
                            getList = Expression.Convert(
                                Expression.Call(
                                    s_GetFileElement,
                                    m_currentProcedure.ContextReferenceInternal,
                                    Expression.Constant(list.UniqueID)),
                                typeof(ITestList));
                        }

                        result = new SBExpressionData(
                            SBExpressionType.TestListReference,
                            new TypeReference(typeof(ITestList), list),
                            getList,
                            list);
                    }
                    break;

                case FileElementType.Override:
                    {
                        var overrider = element;

                        result = new SBExpressionData(
                            SBExpressionType.FileElementOverride,
                            new TypeReference(typeof(IFileElement), overrider),
                            null,
                            overrider);
                    }
                    break;

                case FileElementType.TypeDef:
                    {
                        var typedef = element as FileElementTypeDef;

                        result = new SBExpressionData(
                            SBExpressionType.TypeReference,
                            typedef.DataType,
                            null,
                            typedef);
                    }
                    break;

                case FileElementType.UsingAlias:
                    {
                        var aliasType = element as FileElementUsingAlias;

                        result = new SBExpressionData(
                            SBExpressionType.TypeReference,
                            aliasType.DataType,
                            null,
                            aliasType);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        return result;
    }

    private SBExpressionData ResolveDotIdentifier(IIdentifierInfo left, SBExpressionData right)
    {
        SBExpressionData leftAsExpData = null;
        switch (left.Type)
        {
            case IdentifierType.UnresolvedType:
                throw new NotImplementedException();    // TODO: Handle error
            case IdentifierType.DotNetNamespace:
                leftAsExpData = new SBExpressionData(SBExpressionType.Namespace, null, null, left.Reference);
                break;
            case IdentifierType.DotNetType:
                leftAsExpData = new SBExpressionData(SBExpressionType.TypeReference, left.DataType);
                break;
            //case IdentifierType.FileByName:
            //    break;
            case IdentifierType.FileNamespace:
                leftAsExpData = new SBExpressionData(SBExpressionType.ScriptNamespace, null, null, left.Reference);
                break;
            default:
                throw new NotImplementedException();
        }
        return this.ResolveDotIdentifier(leftAsExpData, right);
    }

    private SBExpressionData ResolveDotIdentifier(SBExpressionData left, SBExpressionData right)
    {
        SBExpressionData result = null;

        switch (left.ReferencedType)
        {
            case SBExpressionType.Namespace:
                result = this.ResolveDotIdentifierNamespace(left, right);
                break;
            case SBExpressionType.GlobalVariableReference:
                result = this.ResolveDotIdentifierGlobalVariableReference(left, right);
                break;
            case SBExpressionType.Identifier:
            //throw new NotSupportedException("Don't think this is supposed to be called ever.");
            //result = this.ResolveDotIdentifierIdentifier(left, right);
            //break;
            case SBExpressionType.Constant:
            case SBExpressionType.Expression:
            case SBExpressionType.Indexing:
            case SBExpressionType.LocalVariableReference:
            case SBExpressionType.PropertyReference:
            case SBExpressionType.TestListReference:
                result = this.ResolveDotIdentifierInstanceReference(left, right, true);
                break;
            case SBExpressionType.FileElementOverride:
                {
                    var overrider = (FileElementOverride)(left.Value);
                    var rootElement = IdentifierToExpressionData(overrider.GetRootBaseElement(), left.Token);
                    if (overrider.HasTypeOverride)
                    {
                        rootElement.Value = left.Value;
                    }
                    result = this.ResolveDotIdentifier(rootElement, right);
                }
                break;
            case SBExpressionType.TypeReference:
                result = this.ResolveDotIdentifierTypeReference(left, right);
                break;
            case SBExpressionType.ScriptNamespace:
            case SBExpressionType.ProcedureReference:
                result = this.ResolveDotIdentifierInstanceReference(left, right, true);
                break;
            case SBExpressionType.DynamicObjectMember:
                result = new SBExpressionData(
                    SBExpressionType.DynamicObjectMember,
                    value: ((string)left.Value) + "." + (string)right.Value,        // Just concatenate the name of the member
                    instanceCode: left.InstanceCode,                                // Same instance reference
                    token: left.Token);
                break;
            default:
                throw new Exception("Illegal left side of dot-operator; " + left.ToString());
        }

        return result;
    }

    private SBExpressionData ResolveDotIdentifierNamespace(SBExpressionData left, SBExpressionData right)
    {
        NamespaceList subs = null;
        var rightString = right.Value as string;
        if (left.NamespaceList.TryGetSubList(rightString, ref subs))
        {
            return new SBExpressionData(SBExpressionType.Namespace, null, null, subs);
        }
        else
        {
            // If the class is static we will not be adding it (At least for now) - Static classes have been giving issues in other cases and does not seem to be used
            //var type = left.NamespaceList.ListTypes(false).FirstOrDefault(ti => ti.Name == rightString && !(ti.IsAbstract && ti.IsSealed));

            var types = left.NamespaceList.ListTypes(false).Where(ti => ti.Name == rightString).ToArray();
            var gtName = rightString + "`";
            var genericTypedefs = left.NamespaceList.ListTypes(false).Where(ti => ti.Name.StartsWith(gtName)).ToList();
            if (genericTypedefs.Count > 0)
            {
                return new SBExpressionData(
                    SBExpressionType.GenericTypeDefinition,
                    (types != null && types.Length > 0) ? (TypeReference)types[0] : (TypeReference)null,
                    value: genericTypedefs,
                    token: right.Token);
            }
            else
            {
                if (types != null && types.Length > 0)
                {
                    return new SBExpressionData(
                        SBExpressionType.TypeReference,
                        (TypeReference)types[0],
                        token: right.Token);
                }
                else
                {
                    return null;
                }
            }
        }
    }
    //private TSExpressionData ResolveDotIdentifierIdentifier(TSExpressionData left, string right)
    //{
    //    throw new NotSupportedException("Don't think this is supposed to be called ever.");
    //}

    private SBExpressionData ResolveDotIdentifierGlobalVariableReference(SBExpressionData left, SBExpressionData right)
    {
        var variableDataType = (left.Value as FileElement).DataType;
        var getValue = Expression.Call(
            left.ExpressionCode,        // Code for getting the variable reference
            left.DataType.Type.GetMethod("GetTypedValue"),
            Expression.Constant(null, typeof(StepBro.Core.Logging.ILogger)));
        var instance = new SBExpressionData(
            SBExpressionType.Expression,
            variableDataType,
            getValue,
            left.Value);
        instance.Instance = left.Instance;  // Preserve the instance reference, if it's there.

        return this.ResolveDotIdentifierInstanceReference(instance, right, true);
    }

    //private TSExpressionData ResolveDotIdentifierLocalVariableReference(TSExpressionData left, string right)
    //{
    //    throw new NotImplementedException();
    //}

    private SBExpressionData ResolveDotIdentifierTypeReference(SBExpressionData left, SBExpressionData right)
    {
        var leftType = left.DataType.Type;
        var rightString = right.Value as string;
        if (leftType.IsEnum)
        {
            try
            {
                var value = Enum.Parse(leftType, rightString);
                return new SBExpressionData(SBExpressionType.Constant, left.DataType, Expression.Constant(value), value, token: right.Token);
            }
            catch
            {
                // Not found in the value list.
                return new SBExpressionData(
                    SBExpressionType.UnknownIdentifier,
                    $"Nonexisting enum value '{rightString}' for enum type '{leftType.Name}'.",
                    rightString,
                    right.Token);
            }
        }
        else if (leftType.IsTypeDefinition)
        {
            var methods = leftType.GetMethods().Where(mi => mi.Name == rightString).ToList();
            methods.AddRange(m_addonManager.ListExtensionMethods(leftType, rightString));
            if (methods.Count > 0)
            {
                return new SBExpressionData(
                    SBExpressionType.MethodReference,   // Expression type
                    left.DataType,                      // Data type
                    left.ExpressionCode,                // The instance expression
                    methods,                            // The method list
                    token: right.Token);
            }
            var properties = leftType.GetProperties().Where(pi => String.Equals(pi.Name, rightString, StringComparison.InvariantCulture)).ToArray();
            if (properties.Length == 1)
            {
                var expression = properties[0].IsStatic() ? Expression.Property(null, properties[0]) : null;    // In case an instance property is specified, no expression can be set.
                return new SBExpressionData(
                    SBExpressionType.PropertyReference,         // Expression type
                    (TypeReference)properties[0].PropertyType,  // Data type
                    expression,                                 // The property access expression (or null)
                    properties[0],                              // Reference to the property info
                    token: right.Token);
            }
            else if (properties.Length > 1)
            {
                m_errors.SymanticError(right.Token.Line, right.Token.Column, false, $"More than one property with same name: \"{rightString}\".");
                return null;
            }
            var nestedTypes = leftType.GetNestedTypes().Where(nt => String.Equals(nt.Name, rightString, StringComparison.InvariantCulture)).ToArray();
            if (nestedTypes.Length == 1)
            {
                return new SBExpressionData(
                    SBExpressionType.TypeReference,             // Expression type
                    (TypeReference)nestedTypes[0],              // Data type
                    null,                                       // No expression to a type.
                    nestedTypes[0],                             // Reference to the type info
                    token: right.Token);
            }

            return null;    // None found
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private SBExpressionData ResolveDotIdentifierInstanceReference(SBExpressionData left, SBExpressionData right, bool includeBaseAndInterfaces)
    {
        var leftType = left.DataType?.Type;
        var rightString = right.Value as string;
        bool nativeOnly = false;    // Whether to skip file elements.

        if (left.ReferencedType == SBExpressionType.ScriptNamespace)
        {
            var theNamespace = (string)(left.Value);

            var filesInNamespace = m_file.ListResolvedFileUsings().Where(f => f.Namespace == theNamespace).ToList();
            if (String.Equals(theNamespace, m_file.Namespace, StringComparison.InvariantCulture))
            {
                filesInNamespace.Insert(0, m_file);
            }

            foreach (var file in filesInNamespace)
            {
                var found = file.ListPublicElements(theNamespace, false).Where(e => e.Name == rightString).ToList();
                if (found.Count == 1)
                {
                    return this.IdentifierToExpressionData(found[0], right.Token);
                }
                else
                {
                    var line = (left.Token != null) ? left.Token.Line : 0;
                    var column = (left.Token != null) ? left.Token.Column : 0;
                    if (found.Count == 0)
                    {
                        m_errors.SymanticError(line, column, false, $"No elements named \"{rightString}\" could be found in the namespace \"{theNamespace}\" ");
                        return new SBExpressionData(SBExpressionType.ExpressionError);
                    }
                    else
                    {
                        m_errors.SymanticError(line, column, false, $"More than one element named \"{rightString}\" could be found in the namespace \"{theNamespace}\".");
                        return new SBExpressionData(SBExpressionType.ExpressionError);
                    }
                }
            }
        }

        if (rightString.StartsWith('@'))
        {
            nativeOnly = true;
            rightString = rightString.Substring(1);
        }
        if (left.DataType.DynamicType is IFileElement)
        {
            FileElement element = left.DataType.DynamicType as FileElement;
            var partner = element.ListPartners().FirstOrDefault(p => p.Name.Equals(rightString, StringComparison.InvariantCulture));

            if (partner != null)
            {
                FileProcedure partnerProcedure = partner.ProcedureReference as FileProcedure;
                var procType = partnerProcedure.ProcedureReferenceType;

                MethodInfo getPartnerTyped;
                Expression elementReference;
                if (left.DataType.DynamicType is IFileProcedure)
                {
                    getPartnerTyped = s_GetPartnerFromProcedure.MakeGenericMethod(partnerProcedure.DelegateType);
                    elementReference = Expression.Convert(left.ExpressionCode, typeof(IProcedureReference));
                }
                else
                {
                    getPartnerTyped = s_GetPartner.MakeGenericMethod(partnerProcedure.DelegateType);
                    elementReference = Expression.Convert(left.ExpressionCode, typeof(IFileElement));
                }

                var getPartnerProc = Expression.Call(
                    getPartnerTyped,
                    m_currentProcedure.ContextReferenceInternal,
                    elementReference,
                    Expression.Constant(rightString));

                return new SBExpressionData(
                    SBExpressionType.Expression,
                    partnerProcedure.DataType,
                    getPartnerProc,
                    instanceCode: left.ExpressionCode);     // Reference to the procedure reference
            }
        }

        List<IIdentifierInfo> fileIdentifiers = null;
        if (!nativeOnly)
        {
            fileIdentifiers = m_file.LookupIdentifier(rightString)?.Where(fe => fe is FileProcedure && CanUseTypeAsInstance(fe as FileProcedure, left.DataType)).ToList();
            if (fileIdentifiers != null && fileIdentifiers.Count > 0)
            {
                var procedureCallReference = ResolveIfIdentifier(right, true);
                procedureCallReference.InstanceCode = left.ExpressionCode;
                procedureCallReference.Instance = left.Instance;
                return procedureCallReference;
            }
        }

        foreach (var type in left.DataType.Type.SelfBasesAndInterfaces(includeBaseAndInterfaces, includeBaseAndInterfaces))
        {

            var methods = type.GetMethods().Where(mi => mi.Name == rightString).ToList();
            methods.AddRange(m_addonManager.ListExtensionMethods(left.DataType.Type, rightString));
            var properties = type.GetProperties().Where(pi => pi.Name == rightString).ToList();

            //if (methods.Count > 0 && properties.Count > 1)
            //{
            //    throw new NotImplementedException("No handling when both methods and props are matching.");
            //}

            if (properties.Count == 1)
            {
                return new SBExpressionData(
                    SBExpressionType.PropertyReference,                         // Expression type
                    (TypeReference)properties[0].PropertyType,                  // Data type
                    Expression.Property(left.ExpressionCode, properties[0]),    // The property access expression
                    properties[0]);                                             // Reference to the found properties
            }
            else if (properties.Count > 1)
            {
                throw new NotImplementedException("More than one property with same name !!!?");
            }

            if (methods.Count > 0)
            {
                var result = new SBExpressionData(
                    left.ExpressionCode,                // The instance expression
                    methods);                           // Reference to the found methods
                result.Instance = left.Instance;        // Preserve the instance name if present.
                return result;
            }
        }

        if (typeof(IDynamicStepBroObject).IsAssignableFrom(left.DataType.Type))
        {
            //if (left.Value != null)
            //{
            //    var variable = left.Value as IValueContainer;
            //    IDynamicStepBroObject dynamicObject = (IDynamicStepBroObject)variable.GetValue(null);
            //    if (dynamicObject != null)
            //    {
            //        Type propType;
            //        bool isReadonly;
            //        var propSupport = dynamicObject.HasProperty(rightString, out propType, out isReadonly);
            //        if (propSupport == DynamicSupport.Yes)
            //        {
            //            return new SBExpressionData(
            //                HomeType.Immediate,
            //                isReadonly ? SBExpressionType.DynamicObjectPropertyReadonly : SBExpressionType.DynamicObjectProperty,
            //                value: rightString,                 // Name of property
            //                instance: left.ExpressionCode,      // Instance reference
            //                token: right.Token);
            //        }

            //        NamedData<Type>[] parameters;
            //        Type returnType;
            //        var methodSupport = dynamicObject.HasMethod(rightString, out parameters, out returnType);
            //        if (methodSupport == DynamicSupport.Yes)
            //        {
            //            return new SBExpressionData(
            //                HomeType.Immediate,
            //                SBExpressionType.DynamicObjectProcedure,
            //                value: rightString,                 // Name of method
            //                instance: left.ExpressionCode,      // Instance reference
            //                token: right.Token);
            //        }

            //        if (propSupport == DynamicSupport.No && methodSupport == DynamicSupport.No)
            //        {
            //            m_errors.SymanticError(right.Token.Line, right.Token.Column, false, "");
            //            // Just leave with 'unknown' to enable parsing to finish without troubles.
            //        }
            //    }
            //}

            return new SBExpressionData(
                SBExpressionType.DynamicObjectMember,
                value: rightString,                 // Name of method
                instanceCode: left.ExpressionCode,      // Instance reference
                token: right.Token);
        }
        if (typeof(IDynamicAsyncStepBroObject).IsAssignableFrom(left.DataType.Type))
        {
            return new SBExpressionData(
                SBExpressionType.DynamicAsyncObjectMember,
                value: rightString,                 // Name of method
                instanceCode: left.ExpressionCode,      // Instance reference
                token: right.Token);
        }
        return null;
    }

    static bool CanUseTypeAsInstance(FileProcedure procedure, TypeReference type)
    {
        var parameters = procedure.GetFormalParameters();   // Note: The parameters returned from GetFormalParameters() do not include the context parameter.
        return procedure.IsFirstParameterThisReference && parameters.Count >= 1 && parameters[0].Type.IsAssignableFrom(type);
    }
}
