using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace StepBro.Core.Data
{
    public interface ISettableFromPropertyBlock
    {
        void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors);
        void Setup(IScriptFile file, ILogger logger, PropertyBlock data);
    }
}
