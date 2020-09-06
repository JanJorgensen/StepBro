using StepBro.Core.Tasks;
using System.Collections.Generic;

namespace StepBro.TestInterface
{
    public interface IRemoteProcedures
    {
        IEnumerable<RemoteProcedureInfo> Procedures { get; }
        IAsyncResult<object> Invoke(RemoteProcedureInfo procedure, params object[] arguments);
        IAsyncResult<object> Invoke(string procedure, params object[] arguments);
    }
}
