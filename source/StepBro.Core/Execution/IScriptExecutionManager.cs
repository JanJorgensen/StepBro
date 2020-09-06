using StepBro.Core.ScriptData;

namespace StepBro.Core.Execution
{
    public interface IScriptExecutionManager
    {
        IExecutionResult ExecuteFileElement(
            //ScriptExecutionConfiguration configuration,
            IFileElement element,
            IPartner partner,
            params object[] arguments);

        IScriptExecution CreateFileElementExecution(
            //ScriptExecutionConfiguration configuration,
            IFileElement element,
            IPartner partner,
            params object[] arguments);
    }
}
