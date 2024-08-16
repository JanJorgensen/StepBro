using StepBro.Core.Data;
using System;
using System.Collections.Generic;

namespace StepBro.Core.ScriptData
{
    internal class FileConfigValue : FileElement
    {
        private readonly IValueContainerOwnerAccess m_variableAccess;
        private readonly int m_id;
        private readonly object m_defaultValue;

        public FileConfigValue(
            IScriptFile file,
            AccessModifier access,
            int line,
            string @namespace,
            string name,
            IValueContainerOwnerAccess variableAccess, int id, object defaultValue) :
                base(file, line, null, @namespace, name, access, FileElementType.Config)
        {
            m_variableAccess = variableAccess;
            m_id = id;
            m_defaultValue = defaultValue;
        }

        public IValueContainerOwnerAccess VariableOwnerAccess { get { return m_variableAccess; } }

        public int ID { get { return m_id; } }

        public static FileConfigValue Create(
            IScriptFile file, AccessModifier access, string @namespace, string name, 
            int line, int column, 
            TypeReference type, object value)
        {
            IValueContainerOwnerAccess vcOwnerAccess = null;
            if (type.Type != null)
            {
                vcOwnerAccess = VariableContainer.Create(@namespace, name, type, readOnly: true);
                System.Diagnostics.Debug.WriteLine($"Creating config value \"{name}\" (in {file.FileName}), with ID {vcOwnerAccess.Container.UniqueID}");
                vcOwnerAccess.FileLine = line;
                vcOwnerAccess.FileColumn = column;
                //vcOwnerAccess.CodeHash = codeHash;
                vcOwnerAccess.Tags = new Dictionary<string, Object>();
            }
            return new FileConfigValue(file, access, line, @namespace, name, vcOwnerAccess, (vcOwnerAccess != null) ? vcOwnerAccess.Container.UniqueID : 0, value);
        }

        protected override TypeReference GetDataType()
        {
            return m_variableAccess.Container.DataType;
        }
    }
}
