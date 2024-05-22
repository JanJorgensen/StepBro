using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogFileCreator : IDisposable
    {
        void CloseFile(bool AwaitLogSilence = true);
    }
}
