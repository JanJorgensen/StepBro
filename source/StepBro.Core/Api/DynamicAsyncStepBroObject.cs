using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.Tasks;

namespace StepBro.Core.Api
{
    public class DynamicAsyncStepBroObject : IDynamicAsyncStepBroObject
    {
        public virtual DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            type = null;
            isReadOnly = false;
            return DynamicSupport.KnownAtRuntimeOnly;
        }

        public virtual IAsyncResult<object> TryGetProperty(string name)
        {
            throw new NotImplementedException();
        }

        public virtual IAsyncResult<bool> TrySetProperty(string name, object value)
        {
            throw new NotImplementedException();
        }

        public virtual DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType)
        {
            parameters = null;
            returnType = null;
            return DynamicSupport.KnownAtRuntimeOnly;
        }

        public virtual IAsyncResult<object> TryInvokeMethod(string name, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
