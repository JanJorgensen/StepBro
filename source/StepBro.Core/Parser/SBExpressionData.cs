using Antlr4.Runtime;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace StepBro.Core.Parser
{
    public enum SBExpressionType
    {
        Namespace,
        Constant,
        Identifier,
        Expression,
        ThisReference,
        ApplicationObject,
        GlobalVariableReference,
        LocalVariableReference,
        HostApplicationVariableReference,
        Indexing,
        TypeReference,
        GenericTypeDefinition,
        PropertyReference,
        MethodReference,
        ScriptNamespace,
        ProcedureReference,
        ProcedurePartner,
        ProcedurePropertyReference,
        ProcedureCustomPropertyReference,
        TestListReference,
        FileElementOverride,
        DatatableReference,
        AwaitExpression,
        DynamicObjectMember,
        DynamicAsyncObjectMember,
        // Errors:
        ExpressionError,
        UnknownIdentifier,
        UnsupportedOperation,
    };

    public class SBExpressionData
    {
        public SBExpressionType ReferencedType;
        public TypeReference DataType;
        public Expression ExpressionCode;
        public Expression InstanceCode = null;
        public string Instance = null;
        private object m_value;
        public string Argument;
        public string ParameterName;
        public int ArgumentIndex = 0;
        public Antlr4.Runtime.IToken Token = null;
        public bool SuggestsAutomaticTypeConversion = false;

        public SBExpressionData(
            SBExpressionType referencedtype = SBExpressionType.Expression,
            TypeReference type = null,
            Expression code = null,
            object value = null,
            string argument = null,
            string parameterName = null,
            string instanceName = null,
            Expression instanceCode = null,
            bool automaticTypeConvert = false,
            Antlr4.Runtime.IToken token = null)
        {
            System.Diagnostics.Debug.Assert(type == null || !type.Type.IsGenericTypeDefinition);
            System.Diagnostics.Debug.Assert(type == null || !type.Type.IsGenericParameter);
            System.Diagnostics.Debug.Assert(value == null || !(value is IFileProcedure));
            this.ReferencedType = referencedtype;
            this.DataType = type ?? (code != null ? (TypeReference)(code.Type) : null);
            this.ExpressionCode = code;
            this.m_value = value;
            this.Argument = argument;
            this.ParameterName = parameterName;
            this.Instance = instanceName;
            this.InstanceCode = instanceCode;
            this.SuggestsAutomaticTypeConversion = automaticTypeConvert;
            this.Token = token;
        }

        public SBExpressionData(SBExpressionType type, string message, string value, Antlr4.Runtime.IToken token = null)
        {
            this.ReferencedType = type;
            this.DataType = null;
            this.ExpressionCode = null;
            this.m_value = value;
            this.Argument = message;
            this.ParameterName = null;
            this.Token = token;
        }

        public SBExpressionData(object value, Antlr4.Runtime.IToken token = null)
        {
            if (value == null) throw new ArgumentNullException();
            this.ReferencedType = SBExpressionType.Constant;
            this.DataType = (TypeReference)(value.GetType());
            this.ExpressionCode = Expression.Constant(value);
            this.m_value = value;
            this.Argument = null;
            this.ParameterName = null;
            this.Token = token;
        }

        public SBExpressionData(TypeReference type, object value, Antlr4.Runtime.IToken token = null)
        {
            this.ReferencedType = SBExpressionType.Constant;
            this.DataType = type;
            this.ExpressionCode = Expression.Constant(value, type.Type);
            this.m_value = value;
            this.Argument = null;
            this.ParameterName = null;
            this.Token = token;
        }
        public static SBExpressionData Constant(TypeReference type, object value, Antlr4.Runtime.IToken token = null)
        {
            System.Diagnostics.Debug.Assert(!type.Type.IsGenericTypeDefinition);
            System.Diagnostics.Debug.Assert(!type.Type.IsGenericParameter);
            System.Diagnostics.Debug.Assert(value == null || !(value is IFileProcedure));
            return new SBExpressionData(
                SBExpressionType.Constant,
                type,
                Expression.Constant(value, type.Type),
                value,
                token: token);
        }

        public SBExpressionData(Expression expression) : this(expression, RefTypeFromExpressionType(expression))
        {
        }

        public SBExpressionData(Expression expression, SBExpressionType type)
        {
            if (expression == null) throw new ArgumentNullException();
            this.ReferencedType = type;
            this.DataType = (TypeReference)expression.Type;
            this.ExpressionCode = expression;
            this.m_value = null;
            this.Argument = null;
            this.ParameterName = null;
        }

        public SBExpressionData(SBExpressionType type)
        {
            this.ReferencedType = type;
            this.DataType = null;
            this.ExpressionCode = null;
            this.m_value = null;
            this.Argument = null;
            this.ParameterName = null;
        }

        public SBExpressionData(Expression instance, List<MethodInfo> methods)
        {
            //System.Diagnostics.Debug.Assert(type == null || !type.IsGenericTypeDefinition);
            //System.Diagnostics.Debug.Assert(type == null || !type.IsGenericParameter);
            this.ReferencedType = SBExpressionType.MethodReference;
            if (methods.Count == 1 && !methods[0].IsGenericMethodDefinition)
            {
                this.DataType = null;
            }
            this.ExpressionCode = instance;
            this.m_value = methods;
            this.Argument = null;
            this.ParameterName = null;
        }

        public SBExpressionData NewExpressionCode(Expression exp)
        {
            var value = m_value;
            if (value != null && value is List<MethodInfo>)
            {
                value = new List<MethodInfo>(value as List<MethodInfo>);
            }
            return new SBExpressionData(
                ReferencedType,
                DataType,
                exp,
                value,
                Argument,
                ParameterName,
                null,
                InstanceCode,
                SuggestsAutomaticTypeConversion,
                Token)
            { ArgumentIndex = ArgumentIndex };
        }

        public object Value
        {
            get { return m_value; }
            set
            {
                this.m_value = value;
            }
        }

        public bool IsError()
        {
            return (this.ReferencedType >= SBExpressionType.ExpressionError);
        }

        public bool IsResolved
        {
            get
            {
                switch (ReferencedType)
                {
                    case SBExpressionType.Identifier:
                    case SBExpressionType.UnknownIdentifier:
                    case SBExpressionType.UnsupportedOperation:
                        //case SBExpressionType.OperationError:
                        return false;
                    default:
                        return true;
                }
            }
        }

        private static SBExpressionType RefTypeFromExpressionType(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Parameter) return SBExpressionType.LocalVariableReference;
            else return SBExpressionType.Expression;
        }

        public static SBExpressionData CreateIdentifier(string name, string argument = null, IToken token = null)
        {
            return new SBExpressionData(SBExpressionType.Identifier, null, null, name, argument, token: token);
        }

        public static SBExpressionData CreateAwaitExpression(Expression expression, IToken token = null)
        {
            return new SBExpressionData(SBExpressionType.AwaitExpression, null, expression, token: token);
        }

        public override string ToString()
        {
            var parts = new List<string>();
            parts.Add(this.ReferencedType.ToString());
            if (this.DataType != null) parts.Add(this.DataType.ToString());
            if (this.Value != null) parts.Add(this.Value.ToString());
            return "SBExpressionData " + String.Join(", ", parts);
        }

        public bool IsNamed { get { return String.IsNullOrEmpty(ParameterName) == false; } }

        public bool IsValueType
        {
            get
            {
                switch (ReferencedType)
                {
                    case SBExpressionType.Namespace:
                    case SBExpressionType.Identifier:
                    case SBExpressionType.TypeReference:
                    case SBExpressionType.MethodReference:
                    case SBExpressionType.ProcedureReference:
                    case SBExpressionType.TestListReference:
                    case SBExpressionType.FileElementOverride:
                    case SBExpressionType.DatatableReference:
                        //case SBExpressionType.OperationError:
                        return false;

                    case SBExpressionType.Constant:
                    case SBExpressionType.Expression:
                    case SBExpressionType.GlobalVariableReference:
                    case SBExpressionType.LocalVariableReference:
                    case SBExpressionType.PropertyReference:
                    case SBExpressionType.Indexing:
                        return true;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        //public Type GetDataType()
        //{
        //    if (this.DataType != null) return this.DataType;
        //    if (this.ExpressionCode != null) return this.ExpressionCode.Type;
        //    return null;
        //}

        public bool IsInt { get { return DataType.Equals(typeof(long)); } }
        public bool IsDecimal { get { return DataType.Equals(typeof(double)); } }
        public bool IsString { get { return DataType.Equals(typeof(string)); } }
        public bool IsIdentifier { get { return DataType.Equals(typeof(Identifier)); } }
        public bool IsBool { get { return DataType.Equals(typeof(bool)); } }
        public bool IsDateTime { get { return DataType.Equals(typeof(DateTime)); } }
        public bool IsTimespan { get { return DataType.Equals(typeof(TimeSpan)); } }
        public bool IsVerdict { get { return DataType.Equals(typeof(Verdict)); } }
        public bool IsUnresolvedIdentifier { get { return ReferencedType == SBExpressionType.Identifier; } }
        public bool IsUnknownIdentifier { get { return ReferencedType == SBExpressionType.UnknownIdentifier; } }
        public bool IsObject { get { return DataType.Type == typeof(object); } }
        public bool IsNullable
        {
            get
            {
                return (DataType.Type.IsGenericType && DataType.Type.GetGenericTypeDefinition() == typeof(Nullable<>));
            }
        }
        public bool IsConstant { get { return ReferencedType == SBExpressionType.Constant; } }
        public bool IsExpression { get { return ReferencedType == SBExpressionType.Expression; } }
        public bool IsPropertyReference { get { return ReferencedType == SBExpressionType.PropertyReference; } }
        public bool IsProcedureReference { get { return ReferencedType == SBExpressionType.ProcedureReference; } }
        public bool IsMethodReference { get { return ReferencedType == SBExpressionType.MethodReference; } }
        public bool IsTestList { get { return ReferencedType == SBExpressionType.TestListReference; } }

        public bool IsAwaitExpression { get { return ReferencedType == SBExpressionType.AwaitExpression; } }
        public bool IsDynamicObjectMember { get { return ReferencedType == SBExpressionType.DynamicObjectMember; } }

        public bool IsOverrideElement { get {  return this.ReferencedType == SBExpressionType.FileElementOverride; } }

        public SBExpressionData NarrowGetValueType(TypeReference type = null)
        {
            TypeReference t = type ?? DataType;
            switch (ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                    this.m_value = NarrowTypeByCasting(this.Value);
                    if (m_value != null)
                    {
                        t = (TypeReference)m_value.GetType();
                    }
                    this.DataType = t;
                    this.ExpressionCode = Expression.Constant(m_value, t.Type);
                    break;

                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                case SBExpressionType.Indexing:
                    this.ExpressionCode = NarrowGetValueTypeByConverting(ExpressionCode, t.Type);
                    if (ExpressionCode != null)
                    {
                        this.DataType = new TypeReference(ExpressionCode.Type);
                    }
                    break;
                case SBExpressionType.GlobalVariableReference:
                    break;  // Maybe a global variable will always be the 'correct type'...
                case SBExpressionType.TypeReference:
                    break;
                case SBExpressionType.MethodReference:
                    break;
                case SBExpressionType.ProcedureReference:
                    break;
                case SBExpressionType.DatatableReference:
                    break;
                default:
                    break;
            }
            return this;
        }

        public static object NarrowTypeByCasting(object input)
        {
            if (input == null) return null;
            if (input is long || input is double || input is string || input is Verdict || input is bool) return input;
            if (input is int) return (long)(int)input;
            if (input is short) return (long)(short)input;
            if (input is sbyte) return (long)(sbyte)input;
            if (input is ulong) return (long)(ulong)input;
            if (input is uint) return (long)(ulong)(uint)input;
            if (input is ushort) return (long)(ulong)(ushort)input;
            if (input is byte) return (long)(ulong)(byte)input;
            if (input is float) return (double)(float)input;
            if (input is decimal) return (double)(decimal)input;
            return input;
        }

        public static Expression NarrowGetValueTypeByConverting(Expression expression, Type type)
        {
            if (type == typeof(long) ||
                type == typeof(double) ||
                type == typeof(string) ||
                type == typeof(Verdict) ||
                type == typeof(bool)) return expression;
            if (type.IsPrimitive)
            {
                if (type == typeof(Int32) || type == typeof(Int16) || type == typeof(SByte) ||
                    type == typeof(UInt32) || type == typeof(UInt16) || type == typeof(Byte) ||
                    type == typeof(UInt64))
                    return Expression.Convert(expression, typeof(long));
                if (type == typeof(float) || type == typeof(decimal))
                    return Expression.Convert(expression, typeof(double));
            }
            //else if (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null)
            //{
            //    return Expression.Constant(null, type);
            //}
            else
            {

            }
            return expression;
        }

        public SBExpressionData ConvertToInt64()
        {
            switch (ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                    this.m_value = System.Convert.ToInt64(Value);
                    this.ExpressionCode = Expression.Constant((Int64)Value);
                    this.DataType = (TypeReference)typeof(Int64);
                    break;
                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                    this.ExpressionCode = Expression.Convert(ExpressionCode, typeof(Int64));
                    this.DataType = (TypeReference)typeof(Int64);
                    break;
                case SBExpressionType.GlobalVariableReference:
                    break;
                case SBExpressionType.TypeReference:
                    break;
                case SBExpressionType.MethodReference:
                    break;
                case SBExpressionType.ProcedureReference:
                    break;
                case SBExpressionType.DatatableReference:
                    break;
                default:
                    break;
            }
            return this;
        }

        public SBExpressionData ConvertToDouble()
        {
            switch (ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                    this.m_value = System.Convert.ToDouble(Value);
                    this.ExpressionCode = Expression.Constant((Double)m_value);
                    this.DataType = (TypeReference)typeof(Double);
                    break;
                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                    this.ExpressionCode = Expression.Convert(ExpressionCode, typeof(double));
                    //this.ExpressionCode = Expression.Call(typeof(System.Convert).GetMethod("ToDouble", new Type[] { this.DataType }), this.ExpressionCode);
                    this.DataType = (TypeReference)typeof(Double);
                    break;
                case SBExpressionType.GlobalVariableReference:
                    break;
                case SBExpressionType.TypeReference:
                    break;
                case SBExpressionType.MethodReference:
                    break;
                case SBExpressionType.ProcedureReference:
                    break;
                case SBExpressionType.DatatableReference:
                    break;
                default:
                    break;
            }
            return this;
        }

        public Expression MethodInstanceReference
        {
            get
            {
                if (ReferencedType != SBExpressionType.MethodReference)
                {
                    throw new NotSupportedException("Property can only be used for method references.");
                }
                return ExpressionCode;
            }
        }

        public int GetMethodCount()
        {
            if (ReferencedType == SBExpressionType.MethodReference)
            {
                if (m_value == null) return 0;
                return ((List<MethodInfo>)m_value).Count;
            }
            throw new NotSupportedException("The reference type is not 'MethodReference'.");
        }

        public List<MethodInfo> GetMethods() { return m_value as List<MethodInfo>; }

        public void AddMethod(MethodInfo method)
        {
            if (ReferencedType == SBExpressionType.MethodReference)
            {
                var list = m_value as List<MethodInfo>;
                if (list == null) throw new NotSupportedException("The element does not contain a method list.");
                list.Add(method);
            }
            else
            {
                throw new NotSupportedException("The reference type is not 'MethodReference'.");
            }
        }

        public void RemoveMethod(MethodInfo method)
        {
            if (ReferencedType == SBExpressionType.MethodReference)
            {
                var list = m_value as List<MethodInfo>;
                if (list == null) throw new NotSupportedException("The element does not contain a method list.");
                if (!list.Remove(method))
                {
                    throw new Exception("The specified method was not found in the method list.");
                }
            }
            else
            {
                throw new NotSupportedException("The reference type is not 'MethodReference'.");
            }
        }

        public StepBro.Core.Api.NamespaceList NamespaceList
        {
            get { return (StepBro.Core.Api.NamespaceList)m_value; }
        }
    }
}
