using System;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate the type should be considered public for use from a TSharp script.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class PublicAttribute : Attribute
    {
        public PublicAttribute() { }
    }
}
