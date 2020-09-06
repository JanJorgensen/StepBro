using System;
using System.Reflection;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Attribute used to indicate that the type or static method that should be used for loading the assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ModuleLoadAttribute : Attribute
    {
        public ModuleLoadAttribute() { }

        public static bool HasAttribute(Type type)
        {
            var attrib = type.GetCustomAttribute<ModuleLoadAttribute>();
            if (attrib != null) return true;
            else return false;
        }
    }
}
