using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.Execution
{
    internal interface IProcedureThis
    {
        bool HasFails { get; }
        bool HasErrors { get; }
        bool HasFailsOrErrors { get; }
        ErrorID LastError { get; }
        string Name { get; }
        void ReportFailure(string failureDescription, ErrorID id = null);
        void ReportError(string errorDescription, ErrorID id = null);
        void AddPartResult(IProcedureReference procedure, ProcedureResult result);
        ProcedureResult Result { get; }
        ProcedureResult LastCallResult { get; }
        bool SetResult(Verdict verdict, string description); 
        string FileName { get; }
        string FileDirectory { get; }
    }
}
