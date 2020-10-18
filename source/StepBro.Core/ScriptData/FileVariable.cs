using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    internal class FileVariable : FileElement
    {
        private readonly IValueContainerOwnerAccess m_variableAccess;
        private readonly int m_id;
        public FileVariable(
            IScriptFile file,
            AccessModifier access,
            int line,
            IFileElement parentElement,
            string @namespace,
            string name,
            IValueContainerOwnerAccess variableAccess, int id) :
                base(file, line, parentElement, @namespace, name, access, FileElementType.FileVariable)
        {
            m_variableAccess = variableAccess;
            m_id = id;
        }

        public IValueContainerOwnerAccess VariableOwnerAccess { get { return m_variableAccess; } }

        public int ID { get { return m_id; } }

        public static FileVariable Create(IScriptFile file, AccessModifier access, string @namespace, string name, TypeReference type, bool readOnly,
            int line, int column, int codeHash,
            VariableContainerAction resetter,
            VariableContainerAction creator,
            VariableContainerAction initializer)
        {
            var vcOwnerAccess = VariableContainer.Create(@namespace, name, type, readOnly);
            vcOwnerAccess.FileLine = line;
            vcOwnerAccess.FileColumn = column;
            vcOwnerAccess.CodeHash = codeHash;
            vcOwnerAccess.Tag = line * 1000 + column;
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
            return new FileVariable(file, access, line, null, @namespace, name, vcOwnerAccess, vcOwnerAccess.Container.UniqueID);
        }

        protected override TypeReference GetDataType()
        {
            throw new System.NotImplementedException();
        }
    }
}
