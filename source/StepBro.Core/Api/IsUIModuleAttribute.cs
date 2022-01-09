using System;
using System.Reflection;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate that the assembly should only be loaded if the host application is a WPF application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class IsUIModuleAttribute : Attribute
    {
        public IsUIModuleAttribute() { }

        public static bool HasAttribute(Assembly assembly)
        {
            var attrib = assembly.GetCustomAttribute<IsUIModuleAttribute>();
            if (attrib != null) return true;
            else return false;
        }
    }
}
