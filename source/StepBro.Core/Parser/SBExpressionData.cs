using Antlr4.Runtime;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace StepBro.Core.Parser
{
    public enum HomeType { Immediate, LocalVariable, GlobalVariable }

    public enum SBExpressionType
    {
        Namespace,
        Constant,
        Identifier,
        Expression,
        ThisReference,
        GlobalVariableReference,
        LocalVariableReference,
        Indexing,
        TypeReference,
        PropertyReference,
        MethodReference,
        ScriptNamespace,
        ProcedureReference,
        ProcedurePartner,
        ProcedurePropertyReference,
        ProcedureCustomPropertyReference,
        TestListReference,
        DatatableReference,
        AwaitExpression,
        DynamicObjectMember,
        DynamicAsyncObjectMember,
        // Errors:
        OperationError,
        UnknownIdentifier,
        UnsupportedOperation,
        ExpressionError
    };

    public class SBExpressionData
    {
        public HomeType Home;
        public SBExpressionType ReferencedType;
        public TypeReference DataType;
        public Expression ExpressionCode;
        public Expression InstanceCode = null;
        public object Value;
        public string Argument;
        public string ParameterName;
        public int ArgumentIndex = 0;
        public Antlr4.Runtime.IToken Token = null;
        public bool SuggestsAutomaticTypeConversion = false;

        public SBExpressionData(
            HomeType home = HomeType.Immediate,
            SBExpressionType referencedtype = SBExpressionType.Expression,
            TypeReference type = null,
            Expression code = null,
            object value = null,
            string argument = null,
            string parameterName = null,
            Expression instance = null,
            bool automaticTypeConvert = false,
            Antlr4.Runtime.IToken token = null)
        {
            System.Diagnostics.Debug.Assert(type == null || !type.Type.IsGenericTypeDefinition);
            System.Diagnostics.Debug.Assert(type == null || !type.Type.IsGenericParameter);
            System.Diagnostics.Debug.Assert(value == null || !(value is IFileProcedure));
            Home = home;
            ReferencedType = referencedtype;
            DataType = type ?? (code != null ? (TypeReference)(code.Type) : null);
            ExpressionCode = code;
            Value = value;
            Argument = argument;
            ParameterName = parameterName;
            InstanceCode = instance;
            SuggestsAutomaticTypeConversion = automaticTypeConvert;
            Token = token;
        }

        public SBExpressionData(SBExpressionType type, string message, string value, Antlr4.Runtime.IToken token = null)
        {
            Home = HomeType.Immediate;
            ReferencedType = type;
            DataType = null;
            ExpressionCode = null;
            Value = value;
            Argument = message;
            ParameterName = null;
            Token = token;
        }

        public SBExpressionData(object value, Antlr4.Runtime.IToken token = null)
        {
            if (value == null) throw new ArgumentNullException();
            Home = HomeType.Immediate;
            ReferencedType = SBExpressionType.Constant;
            DataType = (TypeReference)(value.GetType());
            ExpressionCode = Expression.Constant(value);
            Value = value;
            Argument = null;
            ParameterName = null;
            Token = token;
        }

        public SBExpressionData(TypeReference type, object value, Antlr4.Runtime.IToken token = null)
        {
            Home = HomeType.Immediate;
            ReferencedType = SBExpressionType.Constant;
            DataType = type;
            ExpressionCode = Expression.Constant(value, type.Type);
            Value = value;
            Argument = null;
            ParameterName = null;
            Token = token;
        }
        public static SBExpressionData Constant(TypeReference type, object value, Antlr4.Runtime.IToken token = null)
        {
            System.Diagnostics.Debug.Assert(!type.Type.IsGenericTypeDefinition);
            System.Diagnostics.Debug.Assert(!type.Type.IsGenericParameter);
            System.Diagnostics.Debug.Assert(value == null || !(value is IFileProcedure));
            return new SBExpressionData(
                HomeType.Immediate,
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
            Home = HomeType.Immediate;
            ReferencedType = type;
            DataType = (TypeReference)expression.Type;
            ExpressionCode = expression;
            Value = null;
            Argument = null;
            ParameterName = null;
        }

        public SBExpressionData(SBExpressionType type)
        {
            Home = HomeType.Immediate;
            ReferencedType = type;
            DataType = null;
            ExpressionCode = null;
            Value = null;
            Argument = null;
            ParameterName = null;
        }

        public SBExpressionData(Expression instance, List<MethodInfo> methods)
        {
            //System.Diagnostics.Debug.Assert(type == null || !type.IsGenericTypeDefinition);
            //System.Diagnostics.Debug.Assert(type == null || !type.IsGenericParameter);
            Home = HomeType.Immediate;
            ReferencedType = SBExpressionType.MethodReference;
            if (methods.Count == 1 && !methods[0].IsGenericMethodDefinition)
            {
                DataType = null;
            }
            ExpressionCode = instance;
            Value = methods;
            Argument = null;
            ParameterName = null;
        }

        public SBExpressionData NewExpressionCode(Expression exp)
        {
            var value = Value;
            if (value != null && value is List<MethodInfo>)
            {
                value = new List<MethodInfo>(value as List<MethodInfo>);
            }
            return new SBExpressionData(
                Home,
                ReferencedType,
                DataType,
                exp,
                value,
                Argument,
                ParameterName,
                InstanceCode,
                SuggestsAutomaticTypeConversion,
                Token)
            { ArgumentIndex = ArgumentIndex };
        }

        public bool IsError()
        {
            return (this.ReferencedType >= SBExpressionType.OperationError);
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
                    case SBExpressionType.OperationError:
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
            return new SBExpressionData(HomeType.Immediate, SBExpressionType.Identifier, null, null, name, argument, token: token);
        }

        public static SBExpressionData CreateAwaitExpression(Expression expression, IToken token = null)
        {
            if (expression == null) throw new ArgumentNullException();
            return new SBExpressionData(HomeType.Immediate, SBExpressionType.AwaitExpression, null, expression, token: token);
        }

        public override string ToString()
        {
            if (Value != null)
            {
                return ReferencedType.ToString() + ":" + Value.ToString();
            }
            return base.ToString();
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
                    case SBExpressionType.DatatableReference:
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

        public SBExpressionData NarrowGetValueType(TypeReference type = null)
        {
            TypeReference t = type ?? DataType;
            switch (ReferencedType)
            {
                case SBExpressionType.Namespace:
                    break;
                case SBExpressionType.Constant:
                    Value = NarrowTypeByCasting(Value);
                    if (Value != null)
                    {
                        t = (TypeReference)Value.GetType();
                    }
                    DataType = t;
                    ExpressionCode = Expression.Constant(Value, t.Type);
                    break;

                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                case SBExpressionType.Indexing:
                    ExpressionCode = NarrowGetValueTypeByConverting(ExpressionCode, t.Type);
                    if (ExpressionCode != null)
                    {
                        DataType = new TypeReference(ExpressionCode.Type);
                    }
                    break;
                case SBExpressionType.GlobalVariableReference:
                    throw new NotImplementedException();
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
                    Value = System.Convert.ToInt64(Value);
                    ExpressionCode = Expression.Constant((Int64)Value);
                    DataType = (TypeReference)typeof(Int64);
                    break;
                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                    ExpressionCode = Expression.Convert(ExpressionCode, typeof(Int64));
                    DataType = (TypeReference)typeof(Int64);
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
                    Value = System.Convert.ToDouble(Value);
                    ExpressionCode = Expression.Constant((Double)Value);
                    DataType = (TypeReference)typeof(Double);
                    break;
                case SBExpressionType.Identifier:
                    break;
                case SBExpressionType.Expression:
                case SBExpressionType.LocalVariableReference:
                case SBExpressionType.PropertyReference:
                    ExpressionCode = Expression.Convert(ExpressionCode, typeof(double));
                    //this.ExpressionCode = Expression.Call(typeof(System.Convert).GetMethod("ToDouble", new Type[] { this.DataType }), this.ExpressionCode);
                    DataType = (TypeReference)typeof(Double);
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
                if (Value == null) return 0;
                return ((List<MethodInfo>)Value).Count;
            }
            throw new NotSupportedException("The reference type is not 'MethodReference'.");
        }

        public List<MethodInfo> GetMethods() { return Value as List<MethodInfo>; }

        public void AddMethod(MethodInfo method)
        {
            if (ReferencedType == SBExpressionType.MethodReference)
            {
                var list = Value as List<MethodInfo>;
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
                var list = Value as List<MethodInfo>;
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
            get { return (StepBro.Core.Api.NamespaceList)Value; }
        }
    }
}
