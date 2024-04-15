using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.ExecutionHelper.Messages
{
    internal class ExecutionHelperMessages
    {
    }

    public enum ShortCommand
    {
        None,
        Close,
        IncrementTestCounter,
        GetTestCounter
    }

    public class SendTestCounter
    {
        public SendTestCounter(int testCounter)
        {
            TestCounter = testCounter;
        }

        public int TestCounter { get; set; }
    }
}
