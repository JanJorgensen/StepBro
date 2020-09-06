using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using System;

namespace StepBro.Core.Execution
{
    public delegate void RuntimeErrorListener(IFileProcedure procedure, int line, ErrorID error, string description, Exception exception);
}
