using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileConstant : FileElement, IFileConstant
    {
        private object m_value;

        public FileConstant(
            IScriptFile file,
            AccessModifier access,
            int line,
            string @namespace,
            string name,
            object value) :
                base(file, line, null, @namespace, name, access, FileElementType.Const)
        {
            m_value = value;
        }

        public object Value { get { return m_value; } }

        protected override TypeReference GetDataType()
        {
            return TypeReference.GetSimpleTypeReference(m_value.GetType());
        }
    }
}
