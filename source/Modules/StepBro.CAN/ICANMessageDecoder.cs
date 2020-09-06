using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.CAN
{
    public interface ICANMessageDecoder
    {
        string Name { get; }
        string Description { get; }
        string DecodeMessage(ICANMessage message, List<string> keywordsOut = null);
    }
}
