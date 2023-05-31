using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;

namespace StepBro.Core.ScriptData
{
    internal class FileElementTypeDef : FileElement
    {
        private StepBroTypeScanListener.ScannedTypeDescriptor m_declaration = null;
        private TypeReference m_typeReference = null;

        public FileElementTypeDef(IScriptFile file, int line, string @namespace, string name)
            : base(file, line, null, @namespace, name, AccessModifier.None, FileElementType.TypeDef)
        {
            System.Diagnostics.Debug.WriteLine("TYPEDEF");
        }

        public void SetDeclaration(StepBroTypeScanListener.ScannedTypeDescriptor declaration)
        {
            m_declaration = declaration;
        }

        //public void SetType(TypeReference type)
        //{
        //    if (type == null) throw new ArgumentNullException("type");
        //    if (!type.IsTypedef()) throw new ArgumentException("Type must be a 'TypeDef'.");
        //    m_datatype = type;
        //}

        protected override TypeReference GetDataType()
        {
            return m_typeReference;
        }

        internal override int ParseSignature(StepBroListener listener, bool reportErrors)
        {
            if (m_declaration.ResolvedType == null)
            {
                var numUnresolved = listener.ParseTypedef(m_declaration, reportErrors: reportErrors, token: m_declaration.Token);
                if (numUnresolved == 0 && m_declaration.ResolvedType != null)
                {
                    m_typeReference = new TypeReference(new TypeDef(this.Name, m_declaration.ResolvedType));
                }
                return numUnresolved;
            }
            return 0;
        }
    }
}
