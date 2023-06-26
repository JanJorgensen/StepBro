using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementUsingAlias : FileElement
    {
        private StepBroTypeScanListener.ScannedTypeDescriptor m_declaration = null;
        private TypeReference m_typeReference = null;

        public FileElementUsingAlias(IScriptFile file, int line, string @namespace, string name)
            : base(file, line, null, @namespace, name, AccessModifier.None, FileElementType.UsingAlias)
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
                    m_typeReference = new TypeReference(new UsingAlias(this.Name, m_declaration.ResolvedType));
                }
                return numUnresolved;
            }
            return 0;
        }
    }
}
