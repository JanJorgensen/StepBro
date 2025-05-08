using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementEnum : FileElement, IFileEnumType
    {
        private SoftEnumType m_createdType = null;

        public FileElementEnum(
            IScriptFile file,
            AccessModifier access,
            int line,
            IFileElement parentElement,
            string @namespace,
            string name,
            SoftEnumType type) :
        base(file, line, parentElement, @namespace, name, access, FileElementType.EnumDefinition)
        {
            m_createdType = type;
        }

        protected override TypeReference GetDataType()
        {
            if (m_createdType != null)
            {
                return (TypeReference)(m_createdType.GetType());
            }
            return null;
        }
    }
}
