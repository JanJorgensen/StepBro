using System;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate the type should be considered private (non-public) for use from a StepBro script.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class PrivateAttribute : Attribute
    {
        public PrivateAttribute() { }
    }
}
