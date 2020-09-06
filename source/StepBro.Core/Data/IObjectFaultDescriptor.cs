using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface IObjectFaultDescriptor
    {
        bool IsFaulted { get; }
        string FaultDescription { get; }
    }
}
