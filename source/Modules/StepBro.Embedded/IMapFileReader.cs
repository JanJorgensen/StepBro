using StepBro.Core.Api;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Embedded
{
    public interface IMapFileReader : IDisposable
    {
        public Tuple<uint, uint> GetDataAddressAndSize([Implicit] StepBro.Core.Execution.ICallContext context, string name);
    }
}
