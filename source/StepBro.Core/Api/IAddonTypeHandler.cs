using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public interface IAddonTypeHandler
    {
        bool CheckForSpecialHandling(Type type);
    }
}
