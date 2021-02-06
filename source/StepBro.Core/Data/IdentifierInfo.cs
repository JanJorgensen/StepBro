using System;

namespace StepBro.Core.Data
{
    public enum IdentifierType
    {
        UnresolvedType,
        DotNetNamespace,
        DotNetType,
        DotNetMethod,
        DotNetProperty,
        DotNetField,
        /// <summary>
        /// A file (might be script file) by its name.
        /// </summary>
        FileByName,
        /// <summary>
        /// A namespace in one or more StepBro script files.
        /// </summary>
        FileNamespace,
        /// <summary>
        /// An element in a StepBro script file (variable, procedure, enum type etc.).
        /// </summary>
        FileElement,
        /// <summary>
        /// A parameter for the current procedure.
        /// </summary>
        Parameter,
        /// <summary>
        /// A local variable (not a global or .net object field).
        /// </summary>
        Variable,
        /// <summary>
        /// Parameter for lambda expression.
        /// </summary>
        LambdaParameter,
        /// <summary>
        /// A variable stored in a VariableContainer (a global variable or a host resource).
        /// </summary>
        VariableContainer,
        /// <summary>
        /// An partner reference to a script file procedure.
        /// </summary>
        ElementPartner
    }

    public interface IIdentifierInfo
    {
        string Name { get; }
        string FullName { get; }
        IdentifierType Type { get; }
        TypeReference DataType { get; }
        object Reference { get; }
    }

    public class IdentifierInfo : IIdentifierInfo
    {
        public string Name { get; private set; }
        public string FullName { get; private set; }
        public IdentifierType Type { get; private set; }
        public TypeReference DataType { get; private set; }
        public object Reference { get; private set; }

        public IdentifierInfo(string name, string fullName, IdentifierType type, TypeReference dataType, object reference)
        {
            this.Name = name;
            this.FullName = fullName;
            this.Type = type;
            this.DataType = dataType;
            this.Reference = reference;
        }

        public IdentifierInfo(string name, string fullName, IdentifierType type, TypeReference dataType)
        {
            this.Name = name;
            this.FullName = fullName;
            this.Type = type;
            this.DataType = dataType;
            this.Reference = dataType.DynamicType;
        }

        public override string ToString()
        {
            return this.Type.ToString() + " " + this.DataType.ToString();
        }
    }
}
