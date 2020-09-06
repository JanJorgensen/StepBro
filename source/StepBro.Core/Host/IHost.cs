using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;

namespace StepBro.Core.Host
{
    public interface IHost
    {
        //System.Windows.Forms.Form GetMainForm();
        IEnumerable<Type> ListHostCodeModuleTypes();
        IEnumerable<NamedData<object>> ListHostCodeModuleInstances();
    }
}
