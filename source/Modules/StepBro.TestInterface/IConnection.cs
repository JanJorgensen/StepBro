using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StepBro.TestInterface
{
    public interface IConnection
    {
        [Obsolete]
        bool IsConnected { get; }
        bool IsOpen { get; }

        [Obsolete]
        bool Connect([Implicit] ICallContext context);
        [Obsolete]
        bool Disconnect([Implicit] ICallContext context);
        bool Open([Implicit] ICallContext context);
        bool Close([Implicit] ICallContext context);

        IAsyncResult<bool> UpdateInterfaceData([Implicit] ICallContext context = null);     // TODO: Replace ICallContext with ILogger.

        IParametersAccess Parameters { get; }
        IRemoteProcedures RemoteProcedures { get; }
        
        #region User Profile
        IEnumerable<string> ListAvailableProfiles();
        bool SetProfile(string profile, string accesskey = "");
        #endregion
    }
}
