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
        public DateTime TimeStamp { get; }
        public ITimestampedData DataObject { get; }
        public string GetTextForSearchMatching(bool includeExtraFields);
    }
}
