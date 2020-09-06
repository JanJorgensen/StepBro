using System;

namespace StepBro.Core.Api
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ImplicitAttribute : Attribute
    {
        public ImplicitAttribute() { }
        public static bool IsImplicit(System.Reflection.ParameterInfo parameter)
        {
            return Attribute.IsDefined(parameter, typeof(ImplicitAttribute));
        }
    }
}
