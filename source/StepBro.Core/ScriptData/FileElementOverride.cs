using Antlr4.Runtime;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementOverride : FileElement, IFileElementOverride
    {
        private TypeReference m_datatype = null;
        private Tuple<string, IToken> m_asTypeData = null;
        private TypeReference m_asType = null;

        public FileElementOverride(IScriptFile file, int line, IFileElement parentElement, string name) 
            : base(file, line, parentElement, file.Namespace, name, AccessModifier.None, FileElementType.Override)
        {
            this.BaseElementName = name;
        }

        protected override TypeReference GetDataType()
        {
            if (m_datatype == null)
            {
                m_datatype = new TypeReference(typeof(IFileElement), this);
            }
            return m_datatype;
        }

        public void SetAsType(Tuple<string, IToken> type)
        {
            m_asTypeData = type;
        }

        public bool HasTypeOverride { get {  return m_asTypeData != null; } }

        public TypeReference OverrideType {  get { return m_asType; } }

        internal override int ParseSignature(StepBroListener listener, bool reportErrors)
        {
            base.ParseSignature(listener, reportErrors);

            if (m_asType == null && m_asTypeData != null)
            {
                m_asType = listener.ParseTypeString(m_asTypeData.Item1, reportErrors: reportErrors, token: m_asTypeData.Item2);
                if (m_asType == null) return 1;
                m_datatype = m_asType;
            }
            return 0;
        }
    }
}
