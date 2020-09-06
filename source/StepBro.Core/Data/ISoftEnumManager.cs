using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface ISoftEnumManager
    {
        bool TryRegisterNewType(SoftEnumType type);
        SoftEnumType TryGetType(string nameSpace, string name);
    }
}
