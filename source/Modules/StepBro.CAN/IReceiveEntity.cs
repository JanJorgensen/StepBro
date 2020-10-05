using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.CAN
{
    public interface IReceiveEntity
    {
        string Name { get; }
        long TotalCount { get; }
        bool TryAddHandover(IMessage message);
        string GetStatusText();
        void Flush(bool clearStats);
    }
}
