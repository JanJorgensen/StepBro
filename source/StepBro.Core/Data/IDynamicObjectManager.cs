using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ReadOnlyObservableCollection<IObjectContainer> GetObjectCollection();
        IObjectContainer TryFindObject(string id);
    }
}
