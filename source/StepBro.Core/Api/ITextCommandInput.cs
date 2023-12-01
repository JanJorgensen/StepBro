using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public interface ITextCommandInput
    {
        bool AcceptingCommands();
        void ExecuteCommand(string command);
    }
}
