using System;
using System.Collections.Generic;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.ScriptData;

namespace StepBro.Core.Api
{
    public interface IAddonManager
    {
        void AddTypeHandler(IAddonTypeHandler handler);
        void LoadAssembly(string path, bool loadOnlyTypesWithPublicAttribute);

        void AddAssembly(Assembly assembly, bool loadOnlyTypesWithPublicAttribute);

        IIdentifierInfo Lookup(IEnumerable<UsingData> usings, string name);

        IEnumerable<IIdentifierInfo> List(IEnumerable<UsingData> usings);

        IEnumerable<MethodInfo> ListExtensionMethods(Type type, string name);

        Type TryGetType(IEnumerable<UsingData> usings, string name);

        IEnumerable<IAddon> Addons { get; }

        T TryGetAddon<T>(string name) where T : class, IAddon;

        IEnumerable<Tuple<string, bool, Exception>> ScannedFiles { get; }
    }
}
