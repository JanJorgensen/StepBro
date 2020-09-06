using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StepBro.TestInterface
{
    public interface IConnection
    {
        bool Connect([Implicit] ICallContext context);
        bool Disconnect([Implicit] ICallContext context);

        IAsyncResult<bool> UpdateInterfaceData([Implicit] ICallContext context = null);     // TODO: Replace ICallContext with ILogger.

        IParametersAccess Parameters { get; }
        IRemoteProcedures RemoteProcedures { get; }
        
        #region User Profile
        IEnumerable<string> ListAvailableProfiles();
        bool SetProfile(string profile, string accesskey = "");
        #endregion
    }
}
