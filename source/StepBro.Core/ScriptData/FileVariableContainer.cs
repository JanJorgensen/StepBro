using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    internal class FileVariableContainer
    {
        private readonly IValueContainerOwnerAccess m_variableAccess;
        private readonly int m_id;
        private FileVariableContainer(IValueContainerOwnerAccess variableAccess, int id)
        {
            m_variableAccess = variableAccess;
            m_id = id;
        }

        public IValueContainerOwnerAccess Access { get { return m_variableAccess; } }
        public int ID { get { return m_id; } }

        public static FileVariableContainer Create(string @namespace, string name, TypeReference type, bool readOnly,
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
            return new FileVariableContainer(vcOwnerAccess, vcOwnerAccess.Container.UniqueID);
        }
    }
}
