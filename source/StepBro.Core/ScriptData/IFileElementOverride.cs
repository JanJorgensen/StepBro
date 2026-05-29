using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    public interface IFileElementOverride : IFileElement
    {
        public bool HasTypeOverride { get; }

        public TypeReference OverrideType { get; }
    }
}
