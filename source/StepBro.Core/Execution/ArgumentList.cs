using System.Collections;
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

        public override string ToString()
        {
            if (string.IsNullOrEmpty(m_name))
            {
                return $"<unnamed>: {StringUtils.ObjectToString(m_value)}";
            }
            else
            {
                return $"{m_name}: {StringUtils.ObjectToString(m_value)}";
            }
        }
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

        public ArgumentList(ArgumentList parent, ArgumentList arguments)
        {
            m_parent = parent;
            m_arguments.AddRange(arguments.Select(a => new NamedArgument(a.Name, a.Value)));
        }

        public ArgumentList(object[] firstUnnamed, NamedArgument[] named)
        {
            m_parent = null;
            if (firstUnnamed != null)
            {
                foreach (object arg in firstUnnamed) this.Add(null, arg);
            }
            if (named != null)
            {
                foreach (var arg in named) this.Add(arg.Name, arg.Value);
            }
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
