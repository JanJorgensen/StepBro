using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public interface ISymbolLookupService
    {
        object TryResolveSymbol(string symbol);
        object TryResolveSymbol(IScriptFile fileScope, string symbol);
        object TryResolveSymbol(IFileProcedure procedureScope, string symbol);
    }
}
