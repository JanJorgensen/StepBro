using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface IDynamicObjectManager
    {
        event EventHandler ObjectListChanged;
        void RegisterObjectHost(IObjectHost host);
        void DeRegisterObjectHost(IObjectHost host);
        IEnumerable<IObjectContainer> ListKnownObjects();
        IObjectContainer TryFindObject(string id);
    }
}
