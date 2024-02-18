using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;

namespace StepBro.Core.Host
{
    public enum HostType
    {
        Console,
        WinForms,
        WPF
    }

    public interface IHost
    {
        HostType Type { get; }
        IEnumerable<Type> ListHostCodeModuleTypes();
        IEnumerable<NamedData<object>> ListHostCodeModuleInstances();
    }
}
