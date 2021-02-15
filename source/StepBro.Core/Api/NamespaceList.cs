using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using StepBro.Core.Data;

namespace StepBro.Core.Api
{
    public class NamespaceList : IIdentifierInfo
    {
        private string m_name;
        private string m_fullName;
        private List<NamespaceList> m_SubNamespaces = new List<NamespaceList>();
        private List<Type> m_types = new List<Type>();

        public string Name { get { return m_name; } }
        public string FullName { get { return m_fullName; } }

        public IdentifierType Type { get { return IdentifierType.DotNetNamespace; } }

        public TypeReference DataType { get { return null; } }

        public object Reference { get { return this; } }

        public override string ToString()
        {
            return String.Format(
                "NamespaceList: \"{0}\", {1} subs, {2} types",
                m_fullName,
                m_SubNamespaces.Count,
                m_types.Count);
        }

        internal void AddType(Type type)
        {
            m_types.Add(type);
        }

        internal NamespaceList(string name, string fullname)
        {
            m_name = name;
            m_fullName = fullname;
        }

        internal NamespaceList AddSubNamespace(string name, string fullname)
        {
            var list = new NamespaceList(name, fullname);
            m_SubNamespaces.Add(list);
            return list;
        }
        internal NamespaceList GetOrCreateSubNamespace(string name)
        {
            NamespaceList sub = null;
            if (this.TryGetSubList(name, ref sub))
            {
                return sub;
            }
            var list = new NamespaceList(name, this.FullName + '.' + name);
            m_SubNamespaces.Add(list);
            return list;
        }

        public bool TryGetSubList(string name, ref NamespaceList list)
        {
            foreach (var s in m_SubNamespaces)
            {
                if (s.Name == name)
                {
                    list = s;
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<NamespaceList> ListSubNamespaces(bool recursively)
        {
            foreach (var s in m_SubNamespaces)
            {
                yield return s;
                if (recursively)
                {
                    foreach (var sub in s.ListSubNamespaces(true))
                    {
                        yield return sub;
                    }
                }
            }
        }

        public IEnumerable<Type> ListTypes(bool recursively)
        {
            foreach (var t in m_types)
            {
                yield return t;
                if (recursively)
                {
                    foreach (var sub in m_SubNamespaces)
                    {
                        foreach (var subt in sub.ListTypes(true))
                        {
                            yield return subt;
                        }
                    }
                }
            }
        }
    }
}
