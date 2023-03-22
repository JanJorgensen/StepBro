using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class TypeDef
    {
        private readonly string m_name;
        private readonly TypeReference m_type;

        internal TypeDef(string name, TypeReference type)
        {
            m_name = name;
            m_type = type;
        }

        public string Name { get { return m_name; } }
        public TypeReference Type { get { return m_type; } }

        public override string ToString()
        {
            return "TD-" + m_name;
        }

    }
}
