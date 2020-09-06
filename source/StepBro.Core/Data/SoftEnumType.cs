using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class SoftEnumType
    {
        private string m_nameSpace;
        private string m_name;
        private SoftEnumBase[] m_values = null;
        private IDisposable m_creator = null;
        private ISoftEnumManager m_manager = SoftEnumManager.Instance;  // Default manager

        protected SoftEnumType()
        {
            m_name = this.GetType().Name;
            m_nameSpace = this.GetType().Namespace;
        }

        internal void SetManager(ISoftEnumManager manager)
        {
            m_manager = manager;
        }

        public SoftEnum<TType> FromString<TType>(string name) where TType : SoftEnumType, new()
        {
            foreach (var val in m_values)
            {
                if (val.Name == name) return val as SoftEnum<TType>;
            }
            return null;
        }

        public SoftEnum<TType> FromValue<TType>(int value) where TType : SoftEnumType, new()
        {
            foreach (var val in m_values)
            {
                if (val.Value == value) return val as SoftEnum<TType>;
            }
            return null;
        }

        public SoftEnum<TType> FromIndex<TType>(int index) where TType : SoftEnumType, new()
        {
            foreach (var val in m_values)
            {
                if (val.Index == index) return val as SoftEnum<TType>;
            }
            return null;
        }

        public int ValueCount { get { return m_values.Length; } }

        public IEnumerable<SoftEnum<TType>> Values<TType>() where TType : SoftEnumType, new()
        {
            SoftEnumBase[] values = new SoftEnumBase[m_values.Length];
            Array.Copy(m_values, values, m_values.Length);
            foreach (var val in values)
            {
                yield return val as SoftEnum<TType>;
            }
        }

        public static IEnumerable<SoftEnumBase> ListValues(SoftEnumType type)
        {
            SoftEnumBase[] values = new SoftEnumBase[type.m_values.Length];
            Array.Copy(type.m_values, values, type.m_values.Length);
            foreach (var val in values)
            {
                yield return val;
            }
        }

        public static SoftEnumBase GetValue(SoftEnumType type, string name = "", int value = -1)
        {
            if (String.IsNullOrEmpty(name))
            {
                foreach (var val in type.m_values)
                {
                    if (val.Value == value) return val;
                }
            }
            else
            {
                foreach (var val in type.m_values)
                {
                    if (val.Name == name) return val;
                }
            }
            return null;
        }

        #region Creation and Populating

        internal ITypeCreator<T> SetupSoftEnumType<T>(string nameSpace, string name) where T : SoftEnumType, new()
        {
            if (m_creator == null)
            {
                var creator = Creator<T>.Create(this);
                m_creator = creator;
                return (ITypeCreator<T>)m_creator;
            }
            else
            {
                throw new General.OperationNotAllowedException("The soft-enum type has already been created.");
            }
        }

        private void Update(SoftEnumBase[] values)
        {
            m_values = values;
            if (this.Populated != null) this.Populated(this, EventArgs.Empty);
        }

        public interface ITypeCreator<TType> : IDisposable where TType : SoftEnumType, new()
        {
            SoftEnumType Type { get; }
            IValuePopulator<TType> Populate(StepBro.Core.Logging.ILogger logger);
        }

        public interface IValuePopulator<TType> : IDisposable where TType : SoftEnumType, new()
        {
            SoftEnum<TType> AddEntry(string name, int value);
        }

        public event EventHandler Populating;
        public event EventHandler Populated;

        private void NotifyPopulatorCreated()
        {
            if (this.Populating != null) this.Populating(this, EventArgs.Empty);
        }

        private class Creator<TType> : ITypeCreator<TType> where TType : SoftEnumType, new()
        {
            TType m_type;
            private ValuePopulator m_populator = null;

            private Creator(TType type)
            {
                m_type = type;
            }

            public static Creator<TType> Create(SoftEnumType type)
            {
                var creator = new Creator<TType>(type as TType);
                return creator;
            }

            public SoftEnumType Type { get { return m_type; } }

            public void NotifyPopylatorDispose()
            {
                m_populator = null;
            }

            public void Dispose()
            {
                m_type = null;
            }

            public IValuePopulator<TType> Populate(StepBro.Core.Logging.ILogger logger)
            {
                if (m_populator != null)
                {
                    throw new General.OperationNotAllowedException("The soft-enum is already being populated.");
                }

                m_populator = new ValuePopulator(this, logger);
                m_type.NotifyPopulatorCreated();
                return m_populator;
            }


            private class ValuePopulator : IValuePopulator<TType>
            {
                Creator<TType> m_creator;
                StepBro.Core.Logging.ILogger m_logger;
                List<SoftEnum<TType>> m_values = new List<SoftEnum<TType>>();

                public ValuePopulator(Creator<TType> creator, StepBro.Core.Logging.ILogger logger)
                {
                    m_creator = creator;
                    m_logger = logger;
                }

                public SoftEnum<TType> AddEntry(string name, int value)
                {
                    var v = new SoftEnum<TType>((TType)m_creator.Type, name, value, m_values.Count);
                    m_values.Add(v);
                    return v;
                }

                public void Dispose()
                {
                    if (m_creator != null)
                    {
                        m_creator.Type.Update(m_values.ToArray());
                        m_creator.NotifyPopylatorDispose();
                        m_creator = null;
                    }
                }
            }
        }

        #endregion
    }
}