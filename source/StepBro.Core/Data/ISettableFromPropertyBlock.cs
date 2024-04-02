using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace StepBro.Core.Data
{
    public interface ISettableFromPropertyBlock
    {
        void PreScanData(PropertyBlock data, List<Tuple<int, string>> errors);
        void Setup(ILogger logger, PropertyBlock data);
    }
}
