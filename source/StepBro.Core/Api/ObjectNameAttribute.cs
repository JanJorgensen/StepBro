using System;
using System.Linq;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate the type should be considered public for use from a StepBro script.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class ObjectNameAttribute : Attribute
    {
        public ObjectNameAttribute() { }

        static public bool IsObjectName(System.Reflection.ParameterInfo parameter)
        {
            var attrib = parameter.CustomAttributes.Where(a => a.AttributeType == typeof(ObjectNameAttribute)).FirstOrDefault();
            if (attrib != null)
            {
                return true;
            }
            return false;
        }
    }
}
