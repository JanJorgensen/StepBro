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
        }

        public void SetDeclaration(StepBroTypeScanListener.ScannedTypeDescriptor declaration)
        {
            m_declaration = declaration;
        }

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
                    var type = new TypeReference(new TypeDef(this.Name, m_declaration.ResolvedType));
                    if (m_typeReference == null || 
                        !Object.ReferenceEquals(type.Type, m_typeReference.Type) ||
                        !String.Equals((type.DynamicType as TypeDef).Name, (m_typeReference.DynamicType as TypeDef).Name) ||
                        !Object.ReferenceEquals((type.DynamicType as TypeDef).Type.Type, (m_typeReference.DynamicType as TypeDef).Type.Type) ||
                        !Object.ReferenceEquals((type.DynamicType as TypeDef).Type.DynamicType, (m_typeReference.DynamicType as TypeDef).Type.DynamicType))
                    {
                        m_typeReference = type;
                    }
                }
                else
                {
                    m_typeReference = null;
                }
                return numUnresolved;
            }
            return 0;
        }
    }
}
