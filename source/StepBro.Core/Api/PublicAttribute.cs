using System;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate the type should be considered public for use from a StepBro script.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly |
        AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate |
        AttributeTargets.Event | AttributeTargets.Enum | AttributeTargets.Interface |
        AttributeTargets.Method | AttributeTargets.Property)]
    public class PublicAttribute : Attribute
    {
        public PublicAttribute() { }
    }
}
