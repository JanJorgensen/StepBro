using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public interface IDynamicAsyncStepBroObject
    {
        DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly);
        IAsyncResult<object> TryGetProperty(string name);
        IAsyncResult<bool> TrySetProperty(string name, object value);
        DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType);
        IAsyncResult<object> TryInvokeMethod(string name, object[] args);
    }
}
