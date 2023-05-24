using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;

namespace StepBro.Core.ScriptData
{
    internal class FileElementTypeDef : FileElement
    {
        private TypeReference m_datatype = null;

        public FileElementTypeDef(IScriptFile file, int line, string @namespace, string name)
            : base(file, line, null, @namespace, name, AccessModifier.None, FileElementType.TypeDef)
        {
        }

        public void SetType(TypeReference type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (!type.IsTypedef()) throw new ArgumentException("Type must be a 'TypeDef'.");
            m_datatype = type;
        }

        protected override TypeReference GetDataType()
        {
            return m_datatype;
        }

        internal override int ParseSignature(StepBroListener listener, bool reportErrors)
        {
            return base.ParseSignature(listener, reportErrors);
        }
    }
}
