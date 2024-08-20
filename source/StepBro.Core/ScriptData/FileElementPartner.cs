using System;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.ScriptData
{
    internal class FileElementPartner : IPartner
    {
        public FileElementPartner(IFileElement parent, string name, string referenceName, IFileProcedure procedure, int line)
        {
            this.ParentElement = parent;
            this.Name = name;
            this.ProcedureName = referenceName;
            this.ProcedureReference = procedure;
            this.SourceLine = line;
        }

        public IFileElement ParentElement { get; private set; }

        public string Name { get; private set; }

        public string ProcedureName { get; internal set; }

        public string FullName
        {
            get { return this.Name; }
        }

        public IdentifierType Type
        {
            get { return IdentifierType.ElementPartner; }
        }

        public TypeReference DataType { get; internal set; }

        public object Reference
        {
            get { return this.ProcedureReference; }
        }

        public string SourceFile { get { return this.ParentElement.ParentFile.FilePath; } }

        public int SourceLine { get; private set; }

        public IFileProcedure ProcedureReference { get; internal set; }

        public bool IsModel
        {
            get
            {
                if (this.IsModelDirect) return true;
                var basePartner = (this.ParentElement as FileElement)?.GetRootBaseElement().ListPartners().FirstOrDefault(p => p.Name == this.Name);
                if (basePartner != null && Object.ReferenceEquals(basePartner, this))
                {
                    basePartner = null;
                }
                return (basePartner != null && basePartner.IsModel);
            }
        }

        internal bool IsModelDirect { get; set; } = false;

        private string ReferencedProcedure
        {
            get
            {
                if (this.ProcedureReference == null) return "<unresolved " + ProcedureName + ">";
                else return ProcedureReference.FullName;
            }
        }

        public override string ToString()
        {
            return $"Partner \"{Name}\" on \"{ParentElement.FullName}\" => {ReferencedProcedure}";
        }
    }
}
