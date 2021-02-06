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
    }
}
