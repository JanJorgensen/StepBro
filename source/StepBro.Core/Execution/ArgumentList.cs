﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;

namespace StepBro.Core.Execution
{
    [Public]
    public class NamedArgument
    {
        private readonly string m_name;
        private readonly object m_value;
        public NamedArgument(string name, object value)
        {
            m_name = name;
            m_value = value;
        }
        public string Name { get { return m_name; } }
        public object Value { get { return m_value; } }
    }

    [Public]
    public class ArgumentList : IEnumerable<NamedArgument>
    {
        private ArgumentList m_parent = null;
        private List<NamedArgument> m_arguments = new List<NamedArgument>();

        public ArgumentList(ArgumentList parent)
        {
            m_parent = parent;
        }

        public ArgumentList Clone(params NamedArgument[] arguments)
        {
            var list = new ArgumentList(this);
            foreach (var a in arguments)
            {
                list.Add(a.Name, a.Value);
            }
            return list;
        }

        public void Add(string name, object value)
        {
            m_arguments.Add(new NamedArgument(name, value));
        }

        public IEnumerator<NamedArgument> GetEnumerator()
        {
            return this.List().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.List().GetEnumerator();
        }

        private IEnumerable<NamedArgument> List()
        {
            foreach (var a in m_arguments)
            {
                yield return a;
            }
            if ( m_parent != null)
            {
                foreach (var a in m_parent.List())
                {
                    if (m_arguments.FirstOrDefault(s => (s.Name == a.Name)).Name != null)
                    {
                        yield return a;
                    }
                }
            }
        }
    }
}
