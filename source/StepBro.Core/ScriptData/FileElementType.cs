using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    public enum FileElementType
    {
        /// <summary>
        /// Currently unknown element type.
        /// </summary>
        Unknown,
        /// <summary>
        /// A using-statement.
        /// </summary>
        Using,
        /// <summary>
        /// A namespace declaration.
        /// </summary>
        Namespace,
        /// <summary>
        ///  A definition of a named enum with named values.
        /// </summary>
        EnumDefinition,
        /// <summary>
        ///  A procedure element. 
        /// </summary>
        ProcedureDeclaration,
        /// <summary>
        /// A variable in file scope.
        /// </summary>
        FileVariable,
        /// <summary>
        /// A const value as in C#.
        /// </summary>
        Const,
        /// <summary>
        /// A config value in a value container, but read-only.
        /// </summary>
        Config,
        /// <summary>
        /// A list of test procedure and test list references.
        /// </summary>
        TestList,
        /// <summary>
        /// A datatable definition or reference.
        /// </summary>
        Datatable,
        /// <summary>
        /// The element is an 'overrider' for another file element, and therefore will not be directly accessible itself during execution.
        /// </summary>
        Override,
        /// <summary>
        /// A 'type definition' that defines a named type from another data type.
        /// </summary>
        TypeDef,
        /// <summary>
        /// An alias definition that defines an alias for a data type.
        /// </summary>
        UsingAlias
    }
}
