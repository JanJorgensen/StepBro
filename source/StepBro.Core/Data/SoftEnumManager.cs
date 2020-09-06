using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class SoftEnumManager : ISoftEnumManager
    {
        private static SoftEnumManager g_instance = new SoftEnumManager();
        public static SoftEnumManager Instance { get { return g_instance; } }

        private List<SoftEnumType> m_types = new List<SoftEnumType>();

        public bool TryRegisterNewType(SoftEnumType type)
        {
            if (m_types.Where(t => ReferenceEquals(t.GetType(), type.GetType())).Count() > 0)
            {
                return false;
            }
            else
            {
                m_types.Add(type);
                return true;
            }
        }

        public SoftEnumType TryGetType(string nameSpace, string name)
        {
            return null;
        }

        internal void Clear()
        {
            m_types = new List<SoftEnumType>();
        }
    }
}
