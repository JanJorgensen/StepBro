using StepBro.Core.Data;
using System;
using System.Collections.Generic;

namespace StepBro.Core.ScriptData
{
    internal class FileVariable : FileElement, IFileVariable
    {
        private readonly IValueContainerOwnerAccess m_variableAccess;
        private readonly int m_id;
        private TypeReference m_declaredType = null;

        public FileVariable(
            IScriptFile file,
            AccessModifier access,
            int line,
            IFileElement parentElement,
            string @namespace,
            string name,
            TypeReference type,
            IValueContainerOwnerAccess variableAccess, int id) :
                base(file, line, parentElement, @namespace, name, access, FileElementType.FileVariable)
        {
            m_declaredType = type;
            m_variableAccess = variableAccess;
            m_id = id;
        }

        public IValueContainerOwnerAccess VariableOwnerAccess { get { return m_variableAccess; } }

        public IValueContainer Value { get { return m_variableAccess.Container; } }

        public int ID { get { return m_id; } }

        public static FileVariable Create(IScriptFile file, AccessModifier access, string @namespace, string name, TypeReference type, bool readOnly,
            int line, int column, int codeHash,
            VariableContainerAction resetter,
            VariableContainerAction creator,
            VariableContainerAction initializer)
        {
            IValueContainerOwnerAccess vcOwnerAccess = null;
            if (type.Type != null)
            {
                vcOwnerAccess = VariableContainer.Create(@namespace, name, type, readOnly);
                vcOwnerAccess.SetAccessModifier(access);
                System.Diagnostics.Debug.WriteLine($"Creating variable \"{name}\" (in {file.FileName}), with ID {vcOwnerAccess.Container.UniqueID}");
                vcOwnerAccess.FileLine = line;
                vcOwnerAccess.FileColumn = column;
                vcOwnerAccess.CodeHash = codeHash;
                vcOwnerAccess.Tags = new Dictionary<string, Object>();
                if (resetter != null)
                {
                    vcOwnerAccess.DataResetter = resetter;
                }
                if (creator != null)
                {
                    vcOwnerAccess.DataCreator = creator;
                }
                if (initializer != null)
                {
                    vcOwnerAccess.DataInitializer = initializer;
                }
            }
            return new FileVariable(file, access, line, null, @namespace, name, type, vcOwnerAccess, (vcOwnerAccess != null) ? vcOwnerAccess.Container.UniqueID : 0);
        }

        protected override TypeReference GetDataType()
        {
            return m_declaredType;
        }
    }
}
