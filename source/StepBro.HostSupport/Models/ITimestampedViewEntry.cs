using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public interface ITimestampedViewEntry
    {
        public abstract ITimestampedData DataObject { get; }
    }
}
