using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.ScriptData
{
    public interface IPartner : IIdentifierInfo
    {
        IFileElement ParentElement { get; }
        string ProcedureName { get; }
        IFileProcedure ProcedureReference { get; }
        bool IsModel { get; }
    }
}
