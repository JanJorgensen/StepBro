using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    public enum FileElementType
    {
        Using,
        Namespace,
        EnumDeclaration,
        ProcedureDeclaration,
        FileVariable,
        TestList,
        Datatable,
        /// <summary>
        /// The element is an 'overrider' for another file element, and therefore will not be directly accessible itself during execution.
        /// </summary>
        Override,
        /// <summary>
        /// A 'type definition' that defines a named type from another data type.
        /// </summary>
        TypeDef
    }
}
