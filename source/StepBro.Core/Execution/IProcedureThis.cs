﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.Data.Report;

namespace StepBro.Core.Execution
{
    internal interface IProcedureThis
    {
        string Name { get; }
        string FileName { get; }
        string FileDirectory { get; }
        bool HasFails { get; }
        bool HasErrors { get; }
        bool HasFailsOrErrors { get; }
        ErrorID LastError { get; }
        void ReportFailure(string failureDescription, ErrorID id = null);
        void ReportError(string errorDescription, ErrorID id = null);
        ProcedureResult Result { get; }
        ProcedureResult LastCallResult { get; }
        bool SetResult(Verdict verdict, string description);
        string LastLoopExitReason { get; }
    }
}
