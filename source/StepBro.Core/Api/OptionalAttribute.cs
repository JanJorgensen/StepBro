using System;

namespace StepBro.Core.Api
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class OptionalAttribute : Attribute
    {
        public OptionalAttribute() { }
    }
}
