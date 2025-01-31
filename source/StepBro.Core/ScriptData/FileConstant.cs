using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileConstant : FileElement, IFileConstant
    {
        private object m_defaultValue;
        private object m_overrideValue;

        public FileConstant(
            IScriptFile file,
            AccessModifier access,
            int line,
            string @namespace,
            string name,
            bool @override,
            object value) :
                base(file, line, null, @namespace, name, access, (@override) ? FileElementType.ConstOverride : FileElementType.Const)
        {
            m_defaultValue = value;
        }

        public object Value { get { return (m_overrideValue != null) ? m_overrideValue : m_defaultValue; } }

        public object DefaultValue { get { return m_defaultValue; } }

        public object OverrideValue
        {
            get { return m_overrideValue; }
            set { m_overrideValue = value; }
        }

        protected override TypeReference GetDataType()
        {
            if (m_defaultValue != null && m_defaultValue is IProcedureReference proc)
            {
                return proc.ProcedureData.DataType;
            }
            return TypeReference.GetSimpleTypeReference(m_defaultValue.GetType());
        }
    }
}
