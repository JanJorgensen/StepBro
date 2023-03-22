using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementOverride : FileElement
    {
        private TypeReference m_datatype = null;

        public FileElementOverride(IScriptFile file, int line, IFileElement parentElement, string @namespace, string name) 
            : base(file, line, parentElement, @namespace, name, AccessModifier.None, FileElementType.Override)
        {
            this.BaseElementName = name;
        }

        protected override TypeReference GetDataType()
        {
            if (m_datatype == null) m_datatype = new TypeReference(typeof(IFileElement), this);
            return m_datatype;
        }
    }
}
