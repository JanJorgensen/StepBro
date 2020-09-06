using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.TestInterface
{
    public interface IParametersAccess : IEnumerable<IParameter>
    {
        IAsyncResult UpdateParameterList();
        int ParameterCount { get; }
        IParameter GetParameterInfo(string name);
        IAsyncResult SetParameterValue(int ID, object value);
        IAsyncResult<bool> SetParameterValue(string name, object value);
        IAsyncResult<object> GetParameterValue(int ID);
        IAsyncResult<object> GetParameterValue(string name);
    }
}
