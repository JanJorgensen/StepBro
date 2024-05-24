using StepBro.Core.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogFileCreationManager
    {
        ILogFileCreator AddLogFileCreator(IOutputFormatter formatter, bool includePast);
    }
}
