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

        public SoftEnumType CreateOrGetType(string nameSpace, string name)
        {
            var type = this.ListTypes().FirstOrDefault(t => t.Namespace == nameSpace && t.Name == name);
            if (type == null)
            {
                var createdType = SoftEnumType.CreateType(nameSpace, name);
                var typeOfCreated = createdType.GetType();
                var typeOfValue = typeof(SoftEnum<>).MakeGenericType(typeOfCreated);

                var createMethod = typeOfValue.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, new Type[] { typeof(ISoftEnumManager) });

                type = (SoftEnumType)createMethod.Invoke(null, new object[] { (ISoftEnumManager)this });
            }

            return type;
        }

        public IEnumerable<SoftEnumType> ListTypes()
        {
            foreach (var t in m_types) {  yield return t; }
        }

        internal void Clear()
        {
            m_types = new List<SoftEnumType>();
        }
    }
}
