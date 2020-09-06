using System;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;

namespace StepBro.Core.Data
{
    /// <summary>
    /// A "higher level" version of System.Type, with a reference to dynamic type object.
    /// </summary>
    public class TypeReference
    {
        private readonly Type m_type;
        private readonly object m_dynamicType;

        public TypeReference(Type type, object reference = null)
        {
            //if (type == typeof(TypeReference)) throw new ArgumentException("type");
            //if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IProcedureReference<>))
            //{
            //    System.Diagnostics.Debug.Assert(!type.GenericTypeArguments[0].IsGenericType);
            //}
            //if (reference != null && reference.GetType().IsGenericType && reference.GetType().GetGenericTypeDefinition() == typeof(IProcedureReference<>))
            //{
            //    throw new Exception();
            //}
            m_type = type;
            m_dynamicType = reference;
        }

        public Type Type { get { return m_type; } }
        public object DynamicType { get { return m_dynamicType; } }

        public override string ToString()
        {
            if (this.DynamicType != null) return "TR-" + this.DynamicType.ToString();
            else return "TR-" + this.Type.ToString();
        }

        public bool HaveProcedureReference { get { return (m_dynamicType != null && m_dynamicType is IFileProcedure); } }

        public override bool Equals(object obj)
        {
            if (obj is TypeReference)
            {
                var trObj = obj as TypeReference;
                if (trObj == null || trObj.Type != this.Type) return false;
                if ((this.DynamicType == null) != (trObj.DynamicType != null)) return false;
                if (this.DynamicType != null && Object.ReferenceEquals(this.DynamicType, trObj.DynamicType)) return false;
                return true;
            }
            else if (obj is Type)
            {
                return (this.Type == (obj as Type) && this.DynamicType == null);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this.DynamicType != null) ? this.Type.GetHashCode() ^ this.DynamicType.GetHashCode() : this.Type.GetHashCode();
        }

        public static explicit operator TypeReference(Type b) => new TypeReference(b);

        public static TypeReference TypeVoid { get; } = new TypeReference(typeof(void));
        public static TypeReference TypeBool { get; } = new TypeReference(typeof(bool));
        public static TypeReference TypeString { get; } = new TypeReference(typeof(string));
        public static TypeReference TypeInt32 { get; } = new TypeReference(typeof(int));
        public static TypeReference TypeInt64 { get; } = new TypeReference(typeof(long));
        public static TypeReference TypeDouble { get; } = new TypeReference(typeof(double));
        public static TypeReference TypeDateTime { get; } = new TypeReference(typeof(DateTime));
        public static TypeReference TypeTimeSpan { get; } = new TypeReference(typeof(TimeSpan));
        public static TypeReference TypeVerdict { get; } = new TypeReference(typeof(Verdict));
        public static TypeReference TypeObject { get; } = new TypeReference(typeof(object));
        public static TypeReference TypeProcedure { get; } = new TypeReference(typeof(IProcedureReference));
        public static TypeReference TypeFunction { get; } = new TypeReference(typeof(IFunctionReference));
        public static TypeReference TypeTestList { get; } = new TypeReference(typeof(ITestList));
        public static TypeReference TypeType { get; } = new TypeReference(typeof(TypeReference));
        public static TypeReference TypeAsyncObject { get; } = new TypeReference(typeof(Tasks.IAsyncResult<object>));
    }
}
