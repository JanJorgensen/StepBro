using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace StepBro.Core.Data
{

    public interface IPropertyBlockDataScanner
    {
        void PreScanData(PropertyBlock data, List<Tuple<int, string>> errors);
        PropertyBlockDecoder.Element TryGetDecoder();
    }
    public interface ISettableFromPropertyBlock : IPropertyBlockDataScanner
    {
        void Setup(IScriptFile file, ILogger logger, PropertyBlock data);
    }
}
