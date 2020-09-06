using System;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.ScriptData
{
    internal class FileElementPartner : IPartner
    {
        public FileElementPartner(IFileElement parent, string name, string referenceName, IFileProcedure procedure)
        {
            this.ParentElement = parent;
            this.Name = name;
            this.ProcedureName = referenceName;
            this.ProcedureReference = procedure;
        }

        public IFileElement ParentElement
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string ProcedureName
        {
            get;
            private set;
        }

        public string FullName
        {
            get { return this.Name; }
        }

        public IdentifierType Type
        {
            get { return IdentifierType.ElementPartner; }
        }

        public TypeReference DataType
        {
            get;
            internal set;
        }

        public object Reference
        {
            get { return this.ProcedureReference; }
        }

        public IFileProcedure ProcedureReference
        {
            get;
            private set;
        }
    }
}
