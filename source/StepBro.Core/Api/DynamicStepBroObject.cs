using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Tasks;
using System;

namespace StepBro.Core.Api
{
    public class DynamicStepBroObject : IDynamicStepBroObject
    {
        public virtual DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            type = null;
            isReadOnly = false;
            return DynamicSupport.KnownAtRuntimeOnly;
        }

        public virtual object GetProperty([Implicit] ICallContext context, string name)
        {
            throw new NotSupportedException();
        }

        public virtual void SetProperty([Implicit] ICallContext context, string name, object value)
        {
        }

        public virtual DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType)
        {
            parameters = null;
            returnType = null;
            return DynamicSupport.KnownAtRuntimeOnly;
        }

        public virtual object InvokeMethod([Implicit] ICallContext context, string name, object[] args)
        {
            throw new NotSupportedException();
        }
    }
}
