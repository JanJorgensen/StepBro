using StepBro.Core.Api;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    internal class SymbolLookupService : ServiceBase<ISymbolLookupService, SymbolLookupService>, ISymbolLookupService
    {
        private ServiceManager m_manager = null;

        public SymbolLookupService(out IService service) : base("", out service, typeof(ILoadedFilesManager), typeof(IAddonManager)) { }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            base.Start(manager, context);
            m_manager = manager;
        }

        public object TryResolveSymbol(string symbol)
        {
            return this.TryResolveSymbol(null, null, symbol);
        }

        public object TryResolveSymbol(IScriptFile fileScope, string symbol)
        {
            return this.TryResolveSymbol(fileScope, null, symbol);
        }

        public object TryResolveSymbol(IFileProcedure procedureScope, string symbol)
        {
            return this.TryResolveSymbol(null, procedureScope, symbol);
        }

        public object TryResolveSymbol(IScriptFile fileScope, IFileProcedure procedureScope, string symbol)
        {
            System.Diagnostics.Debug.WriteLine("Lookup symbol: " + symbol);

            var errors = new ErrorCollector(fileScope);
            var listener = new StepBroListener(errors, m_manager.Get<IAddonManager>(), fileScope as ScriptFile);

            return listener.TryResolveSymbol(procedureScope, symbol);
        }
    }
}
