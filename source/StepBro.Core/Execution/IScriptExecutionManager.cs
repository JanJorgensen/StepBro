using StepBro.Core.ScriptData;
using System.Collections.ObjectModel;

namespace StepBro.Core.Execution
{
    public interface IScriptExecutionManager
    {
        IScriptExecution ExecuteFileElement(
            //ScriptExecutionConfiguration configuration,
            IFileElement element,
            IPartner partner,
            params object[] arguments);

        IScriptExecution CreateFileElementExecution(
            //ScriptExecutionConfiguration configuration,
            IFileElement element,
            IPartner partner,
            params object[] arguments);

        ReadOnlyObservableCollection<IScriptExecution> Executions { get; }
    }
}
