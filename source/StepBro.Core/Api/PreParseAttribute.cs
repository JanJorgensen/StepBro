using System;

namespace StepBro.Core.Api
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class PreParseAttribute : Attribute
    {
        public PreParseAttribute() { }
    }
}
