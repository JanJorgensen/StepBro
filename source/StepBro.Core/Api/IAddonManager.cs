using System;
using System.Collections.Generic;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.ScriptData;

namespace StepBro.Core.Api
{
    public interface IAddonManager
    {
        void LoadAssembly(string path, bool loadOnlyTypesWithPublicAttribute);

        void AddAssembly(Assembly assembly, bool loadOnlyTypesWithPublicAttribute);

        IIdentifierInfo Lookup(IEnumerable<IIdentifierInfo> usings, string name);

        IEnumerable<IIdentifierInfo> List(IEnumerable<IIdentifierInfo> usings);

        IEnumerable<MethodInfo> ListExtensionMethods(Type type, Func<MethodInfo, bool> filter = null);

        Type TryGetType(IEnumerable<IIdentifierInfo> usings, string name);

        IEnumerable<ObjectPanelCreator> GetPanelCreators();

    }
}
