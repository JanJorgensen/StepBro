﻿using StepBro.Core.Data;
using StepBro.Core.Host;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace StepBro.Core.Api
{
    internal class AddonManager : ServiceBase<IAddonManager, AddonManager>, IAddonManager
    {
        /// <summary>
        /// Helper class made from the guide here: https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
        /// </summary>
        class PluginLoadContext : AssemblyLoadContext
        {
            private AssemblyDependencyResolver _resolver;
            static Dictionary<string, Assembly> _assemblyCache = new Dictionary<string, Assembly>();

            public PluginLoadContext(string pluginPath)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                {
                    var assembly = LoadFromAssemblyPath(assemblyPath);
                    if (!_assemblyCache.ContainsKey(assemblyName.Name))
                    {
                        _assemblyCache[assemblyName.Name] = assembly;
                    }
                    return assembly;
                }

                if (_assemblyCache.ContainsKey(assemblyName.Name))
                {
                    return _assemblyCache[assemblyName.Name];
                }

                return null;
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (libraryPath != null)
                {
                    return LoadUnmanagedDllFromPath(libraryPath);
                }

                return IntPtr.Zero;
            }
        }

        private bool m_hostIsWPF = false;
        private readonly Action<IAddonManager> m_basicModulesLoader;
        private List<Assembly> m_assemblies = new List<Assembly>();
        private List<Tuple<string, bool, Exception>> m_scannedFiles = new List<Tuple<string, bool, Exception>>();
        private List<NamespaceList> m_rootNamespaces = new List<NamespaceList>();
        private Dictionary<string, NamespaceList> m_namespaceLookup = new Dictionary<string, NamespaceList>();
        private Dictionary<string, Type> m_typeLookup = new Dictionary<string, Type>();
        private List<Tuple<string, Type>> m_types = new List<Tuple<string, Type>>();
        private List<Tuple<string, Type>> m_extensionTypes = new List<Tuple<string, Type>>();
        private Dictionary<string, IIdentifierInfo> m_lookup = new Dictionary<string, IIdentifierInfo>();
        private List<IAddonTypeHandler> m_specialTypeHandlers = new List<IAddonTypeHandler>();
        private List<IAddon> m_addons = new List<IAddon>();
        private Dictionary<Type, Dictionary<string, List<MethodInfo>>> m_extensionMethods = new Dictionary<Type, Dictionary<string, List<MethodInfo>>>();

        public AddonManager(Action<IAddonManager> basicModulesLoader, out IService serviceAccess) :
            base("AddonManager", out serviceAccess, typeof(Logging.ILogger))
        {
            m_basicModulesLoader = basicModulesLoader;
            this.AddOptionalDependency(typeof(IHost));
        }

        public void AddTypeHandler(IAddonTypeHandler handler)
        {
            m_specialTypeHandlers.Add(handler);
        }

        public static IAddonManager Create(Action<IAddonManager> basicModulesLoader = null)
        {
            IService s;
            return new AddonManager(basicModulesLoader, out s);
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            var host = manager.Get<IHost>();
            m_hostIsWPF = (host != null) ? (host.Type == HostType.WPF) : false;

            if (host != null)
            {
                foreach (var t in host.ListHostCodeModuleTypes())
                {
                    this.AddTypeLookup(t);
                }
            }

            if (m_basicModulesLoader != null)
            {
                m_basicModulesLoader(this);
            }
        }

        public void LoadAssembly(string path, bool loadOnlyTypesWithPublicAttribute)
        {
            bool skipped = false;
            Exception loadException = null;
            try
            {
                var loadContext = new PluginLoadContext(path);
                var fileAssembly = loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(path));
                //if (fileAssembly.GetName().Name == "StepBro.Streams")
                //{
                //    var portType = fileAssembly.ExportedTypes.Where(t => t.Name == "SerialPort").FirstOrDefault();
                //    if (portType != null)
                //    {
                //        object obj = Activator.CreateInstance(portType, new object[] { null });
                //        PropertyInfo prop = portType.GetProperty("BaudRate");
                //        prop.SetValue(obj, 115200L, null);
                //    }
                //}
                if (!IsUIModuleAttribute.HasAttribute(fileAssembly) || m_hostIsWPF)
                {
                    this.AddAssembly(fileAssembly, loadOnlyTypesWithPublicAttribute);
                }
                else
                {
                    skipped = true;
                }
            }
            catch (BadImageFormatException) { skipped = true; }
            catch (Exception ex)
            {
                loadException = ex;
            }
            m_scannedFiles.Add(new Tuple<string, bool, Exception>(path, skipped, loadException));
        }

        public IEnumerable<Tuple<string, bool, Exception>> ScannedFiles
        {
            get
            {
                foreach (var f in m_scannedFiles)
                {
                    yield return new Tuple<string, bool, Exception>(f.Item1, f.Item2, f.Item3);
                }
            }
        }

        public void AddAssembly(Assembly assembly, bool loadOnlyTypesWithPublicAttribute)
        {
            if (!m_assemblies.Contains(assembly))
            {
                System.Diagnostics.Debug.WriteLine("MODULE ASSEMBLY: " + assembly.FullName);
                Type loadType = null;
                this.AddAssemblyData(assembly, loadOnlyTypesWithPublicAttribute, ref loadType);
                if (loadType != null)
                {
                    object obj = Activator.CreateInstance(loadType);
                }
                m_assemblies.Add(assembly);
            }
        }

        public static Assembly StepBroCoreAssembly
        {
            get { return typeof(AddonManager).Assembly; }
        }

        private void AddAssemblyData(Assembly assembly, bool loadOnlyTypesWithPublicAttribute, ref Type loadType)
        {
            System.Diagnostics.Debug.WriteLine("MODULE ASSEMBLY UPDATE: " + assembly.FullName);
            var attribs = assembly.GetCustomAttributes();
            foreach (Type type in assembly.GetExportedTypes())
            {
                string name = type.TypeName();
                if (loadOnlyTypesWithPublicAttribute)
                {
                    PublicAttribute pubAttrib = type.GetCustomAttribute<PublicAttribute>(true);
                    if (pubAttrib == null) continue;
                }
                if (ModuleLoadAttribute.HasAttribute(type))
                {
                    loadType = type;
                    continue;   // Don't load this type
                }
                if (typeof(IAddon).IsAssignableFrom(type))
                {
                    try
                    {
                        IAddon addon = (IAddon)Activator.CreateInstance(type);
                        m_addons.Add(addon);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Error creating addon instance of '" + type.Name + "'. Error: " + ex.ToString());
                        // TODO: Register error somewhere.
                    }
                    continue;
                }
                if (type.IsTypeDefinition)
                {
                    bool handled = false;
                    foreach (var th in m_specialTypeHandlers)
                    {
                        if (th.CheckForSpecialHandling(type))
                        {
                            handled = true;
                            break;
                        }
                    }
                    if (!handled)
                    {
                        this.AddTypeLookup(type);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Non-loaded type: " + type.FullName);
                }
            }
        }

        public IIdentifierInfo Lookup(IEnumerable<UsingData> usings, string name)
        {
            IIdentifierInfo data;
            if (m_lookup.TryGetValue(name, out data)) return data;
            else if (usings != null)
            {
                foreach (UsingData u in usings)
                {
                    var ui = u.Identifier;
                    switch (ui.Type)
                    {
                        case IdentifierType.DotNetNamespace:
                            if (m_lookup.TryGetValue(((NamespaceList)ui.Reference).FullName + "." + name, out data)) return data;
                            break;

                        case IdentifierType.DotNetType:
                            foreach (var nt in ((Type)ui.DataType.Type).GetNestedTypes())
                            {
                                if (nt.Name.Equals(name, StringComparison.InvariantCulture))
                                {
                                    return new IdentifierInfo(name, ui.FullName + "." + name, IdentifierType.DotNetType, (TypeReference)nt);
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            return null;
        }

        public IEnumerable<IIdentifierInfo> List(IEnumerable<UsingData> usings)
        {
            yield break;
            //foreach (IIdentifierInfo u in usings)
            //{
            //    switch (u.Type)
            //    {
            //        case IdentifierType.DotNetNamespace:
            //            if (m_lookup.TryGetValue(((NamespaceList)u.Reference).FullName + "." + name, out data)) return data;
            //            break;

            //        case IdentifierType.DotNetType:
            //            foreach (var nt in ((Type)u.DataType.Type).GetNestedTypes())
            //            {
            //                if (nt.Name.Equals(name, StringComparison.InvariantCulture))
            //                {
            //                    return new IdentifierInfo(name, u.FullName + "." + name, IdentifierType.DotNetType, (TypeReference)nt);
            //                }
            //            }
            //            break;

            //        default:
            //            break;
            //    }
            //}
        }

        public IEnumerable<MethodInfo> ListExtensionMethods(Type thistype, string name)
        {
            // TODO: Optimize this; takes most of the parsing time!!!
            Dictionary<string, List<MethodInfo>> typeMethods = null;
            if (!m_extensionMethods.ContainsKey(thistype))
            {
                typeMethods = new Dictionary<string, List<MethodInfo>>();
                m_extensionMethods.Add(thistype, typeMethods);
            }
            else
            {
                typeMethods = m_extensionMethods[thistype];
            }

            List<MethodInfo> extensionMethods = null;
            if (!typeMethods.ContainsKey(name))
            {
                extensionMethods = new List<MethodInfo>();
                typeMethods.Add(name, extensionMethods);

                foreach (var tt in thistype.SelfAndInterfaces())
                {
                    foreach (var t in m_extensionTypes)
                    {
                        foreach (var method in t.Item2.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.IsExtension() && m.Name == name))
                        {
                            var m = method;
                            if (method.IsGenericMethodDefinition)
                            {
                                var genericArgs = method.GetGenericArguments();
                                if (genericArgs.Length == 1)
                                {
                                    var pars = method.GetParameters();
                                    System.Diagnostics.Debug.Assert(pars.Length >= 1);
                                    // Is the extension-type the same as the generic argument?
                                    if (genericArgs[0] == pars[0].ParameterType)
                                    {
                                        m = m.MakeGenericMethod(thistype);
                                    }
                                    else if (
                                        thistype.IsGenericType &&
                                        pars[0].ParameterType.IsGenericType &&
                                        //pars[0].ParameterType.GetGenericArguments().Length == 1 &&
                                        genericArgs[0] == pars[0].ParameterType.GetGenericArguments()[0])
                                    {
                                        try
                                        {
                                            m = m.MakeGenericMethod(thistype.GetGenericArguments()[0]);
                                        }
                                        catch (ArgumentException)
                                        {
                                            continue;   // Didn't work; skip it.
                                        }
                                    }
                                    else
                                    {
                                        // Not implemented/supported, just skip !
                                        continue;
                                    }
                                }
                            }

                            if (m.GetParameters()[0].IsAssignableFrom(tt))
                            {
                                if (!extensionMethods.Contains(m))
                                {
                                    extensionMethods.Add(m);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                extensionMethods = typeMethods[name];
            }

            foreach (var mi in extensionMethods)
            {
                yield return mi;
            }
        }


        public Type TryGetType(IEnumerable<UsingData> usings, string name)
        {
            var found = this.Lookup(usings, name);
            if (found != null && found.Type == IdentifierType.DotNetType)
            {
                return found.DataType.Type;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<IAddon> Addons
        {
            get
            {
                foreach (var a in m_addons) yield return a;
            }
        }

        public T TryGetAddon<T>(string name) where T : class, IAddon
        {
            foreach (var a in m_addons)
            {
                if (String.Equals(a.ShortName, name, StringComparison.InvariantCulture) && a is T)
                {
                    return a as T;
                }
            }
            return null;
        }

        public void AddTypeLookup(Type type)
        {
            var fullName = type.FullName;
            Type declaringType = null;
            Type dt = type.DeclaringType;
            if (dt != null)
            {
                string typeName = type.Name;
                while (dt != null)
                {
                    typeName = dt.Name + "." + typeName;
                    declaringType = dt;
                    dt = dt.DeclaringType;
                }
                fullName = declaringType.Namespace + "." + typeName;
            }

            if (!m_typeLookup.ContainsKey(fullName))    // Add only if not covered as a sub-type of another type.
            {
                m_typeLookup.Add(fullName, type);
                m_lookup.Add(fullName, new IdentifierInfo(fullName, fullName, IdentifierType.DotNetType, new TypeReference(type), null));

                if (!String.IsNullOrEmpty(type.Namespace))
                {
                    NamespaceList nsList = this.GetOrCreateNamespaceList(type.Namespace);
                    nsList.AddType(type);
                }

                m_types.Add(new Tuple<string, Type>(fullName, type));
                if (type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.IsExtension()).Any())
                {
                    m_extensionTypes.Add(new Tuple<string, Type>(fullName, type));
                }
            }
        }

        public NamespaceList GetOrCreateNamespaceList(string name)
        {
            NamespaceList found;
            if (m_namespaceLookup.TryGetValue(name, out found))
            {
                return found;
            }
            else
            {
                int i = name.IndexOf('.');
                if (i >= 0)
                {
                    var listName = name.Substring(0, i);
                    NamespaceList list = null;
                    foreach (var l in m_rootNamespaces)
                    {
                        if (l.Name == listName)
                        {
                            list = l;
                            break;
                        }
                    }
                    if (list == null)
                    {
                        list = new NamespaceList(listName, listName);
                        m_rootNamespaces.Add(list);
                        m_namespaceLookup.Add(listName, list);
                        m_lookup.Add(listName, new IdentifierInfo(listName, listName, IdentifierType.DotNetNamespace, null, list));
                    }
                    return this.GetOrCreateNamespaceListInList(list, name);
                }
                else
                {
                    foreach (var l in m_rootNamespaces)
                    {
                        if (l.Name == name)
                        {
                            return l;
                        }
                    }
                    var list = new NamespaceList(name, name);
                    m_rootNamespaces.Add(list);
                    m_namespaceLookup.Add(name, list);
                    m_lookup.Add(name, new IdentifierInfo(name, name, IdentifierType.DotNetNamespace, null, list));
                    return list;
                }
            }
        }

        private NamespaceList GetOrCreateNamespaceListInList(NamespaceList list, string name)
        {
            foreach (var sub in list.ListSubNamespaces(false))
            {
                if (sub.FullName == name) return sub;
                if (name.StartsWith(sub.FullName + "."))
                {
                    return this.GetOrCreateNamespaceListInList(sub, name);
                }
            }


            int i = name.IndexOf('.', list.FullName.Length + 1);
            if (i >= 0)
            {
                var subFull = name.Substring(0, i);
                var subName = subFull.Substring(list.FullName.Length + 1);
                var subList = list.GetOrCreateSubNamespace(subName);
                m_namespaceLookup.Add(subFull, subList);
                m_lookup.Add(subFull, new IdentifierInfo(subList.Name, subFull, IdentifierType.DotNetNamespace, null, subList));
                return this.GetOrCreateNamespaceListInList(subList, name);
            }
            else
            {
                var subName = name.Substring(list.FullName.Length + 1);
                var subList = list.GetOrCreateSubNamespace(subName);
                m_namespaceLookup.Add(name, subList);
                m_lookup.Add(name, new IdentifierInfo(subName, name, IdentifierType.DotNetNamespace, null, subList));
                return subList;
            }
        }
    }
}
