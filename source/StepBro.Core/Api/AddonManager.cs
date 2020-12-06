using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StepBro.Core.Api
{
    internal class AddonManager : ServiceBase<IAddonManager, AddonManager>, IAddonManager
    {
        private readonly Action<IAddonManager> m_basicModulesLoader;
        private List<Assembly> m_assemblies = new List<Assembly>();
        private List<NamespaceList> m_rootNamespaces = new List<NamespaceList>();
        private Dictionary<string, NamespaceList> m_namespaceLookup = new Dictionary<string, NamespaceList>();
        private Dictionary<string, Type> m_typeLookup = new Dictionary<string, Type>();
        private List<Tuple<string, Type>> m_types = new List<Tuple<string, Type>>();
        private Dictionary<string, IIdentifierInfo> m_lookup = new Dictionary<string, IIdentifierInfo>();
        private List<Type> m_panelCreatorTypes = new List<Type>();
        private List<ObjectPanelCreator> m_panelCreators = new List<ObjectPanelCreator>();

        public AddonManager(Action<IAddonManager> basicModulesLoader, out IService serviceAccess) :
            base("AddonManager", out serviceAccess, typeof(Logging.IMainLogger))
        {
            m_basicModulesLoader = basicModulesLoader;
        }

        public static IAddonManager Create(Action<IAddonManager> basicModulesLoader = null)
        {
            IService s;
            return new AddonManager(basicModulesLoader, out s);
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            if (m_basicModulesLoader != null)
            {
                m_basicModulesLoader(this);
            }

            foreach (Type t in m_panelCreatorTypes)
            {
                try
                {
                    var creator = Activator.CreateInstance(t) as ObjectPanelCreator;
                    if (creator != null)
                    {
                        m_panelCreators.Add(creator);
                    }
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void LoadAssembly(string path, bool loadOnlyTypesWithPublicAttribute)
        {
            var fileAssembly = Assembly.LoadFrom(path);
            this.AddAssembly(fileAssembly, loadOnlyTypesWithPublicAttribute);
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
            foreach (Type type in assembly.GetExportedTypes())
            {
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
                if (type.IsClass || type.IsInterface || type.IsEnum)
                {
                    if (type.IsClass && typeof(ObjectPanelCreator).IsAssignableFrom(type))
                    {
                        m_panelCreatorTypes.Add(type);
                    }
                    else
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

        public IEnumerable<MethodInfo> ListExtensionMethods(Type thistype, Func<MethodInfo, bool> filter = null)
        {
            // TODO: Optimize this; takes most of the parsing time!!!
            var f = (filter != null) ? filter : (a => true);
            List<MethodInfo> returned = new List<MethodInfo>();
            foreach (var tt in thistype.SelfAndInterfaces())
            {
                foreach (var t in m_types)
                {
                    foreach (var method in t.Item2.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.IsExtension()).Where(f))
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
                                    genericArgs[0] == pars[0].ParameterType.GetGenericArguments()[0])
                                {
                                    m = m.MakeGenericMethod(thistype.GetGenericArguments()[0]);
                                }
                                else
                                {
                                    // Not implemented/supported, just skip !
                                }
                            }
                        }

                        if (m.GetParameters()[0].IsAssignableFrom(tt))
                        {
                            if (!returned.Contains(m))
                            {
                                yield return m;
                                returned.Add(m);
                            }
                        }
                    }
                }
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

                NamespaceList nsList = this.GetOrCreateNamespaceList(type.Namespace);
                nsList.AddType(type);

                m_types.Add(new Tuple<string, Type>(fullName, type));
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

        public IEnumerable<ObjectPanelCreator> GetPanelCreators()
        {
            foreach (var c in m_panelCreators) yield return c;
        }
    }
}
