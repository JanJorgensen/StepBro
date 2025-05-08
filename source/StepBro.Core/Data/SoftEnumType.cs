using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace StepBro.Core.Data;

public class SoftEnumType
{
    private string m_nameSpace;
    private string m_name;
    private SoftEnum m_template = null;
    private SoftEnum[] m_values = null;
    private ICreator m_creator = null;
    private ISoftEnumManager m_manager = SoftEnumManager.Instance;  // Default manager

    protected SoftEnumType()
    {
        m_name = this.GetType().Name;
        m_nameSpace = this.GetType().Namespace;
    }

    public string Namespace { get { return m_nameSpace; } }

    public string Name { get { return m_name; } }

    internal void Initialize(ISoftEnumManager manager, SoftEnum template)
    {
        m_manager = manager;
        m_template = template;
    }

    internal static SoftEnumType CreateType(string @namespace, string name)
    {
        var an = new AssemblyName("StepBro.Dynamic." + @namespace);
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        TypeBuilder tb = moduleBuilder.DefineType(@namespace + "." + name,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                typeof(SoftEnumType));

        ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

        Type objectType = tb.CreateType();
        return (SoftEnumType)Activator.CreateInstance(objectType); 
    }

    public SoftEnum FromString(string name)
    {
        foreach (var val in m_values)
        {
            if (val.Name == name) return val;
        }
        return null;
    }
    public SoftEnum<TType> FromString<TType>(string name) where TType : SoftEnumType, new()
    {
        foreach (var val in m_values)
        {
            if (val.Name == name) return val as SoftEnum<TType>;
        }
        return null;
    }

    public SoftEnum FromValue(int value)
    {
        foreach (var val in m_values)
        {
            if (val.Value == value) return val;
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

    public SoftEnum FromIndex(int index)
    {
        foreach (var val in m_values)
        {
            if (val.Index == index) return val;
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

    public int ValueCount { get { return (m_values != null) ? m_values.Length : 0; } }

    //public IEnumerable<SoftEnum<TType>> Values<TType>() where TType : SoftEnumType, new()
    //{
    //    SoftEnum[] values = new SoftEnum[m_values.Length];
    //    Array.Copy(m_values, values, m_values.Length);
    //    foreach (var val in values)
    //    {
    //        yield return val as SoftEnum<TType>;
    //    }
    //}

    //public static IEnumerable<SoftEnum> ListValues(SoftEnumType type)
    //{
    //    SoftEnum[] values = new SoftEnum[type.m_values.Length];
    //    Array.Copy(type.m_values, values, type.m_values.Length);
    //    foreach (var val in values)
    //    {
    //        yield return val;
    //    }
    //}

    //public static SoftEnum GetValue(SoftEnumType type, string name = "", int value = -1)
    //{
    //    if (String.IsNullOrEmpty(name))
    //    {
    //        foreach (var val in type.m_values)
    //        {
    //            if (val.Value == value) return val;
    //        }
    //    }
    //    else
    //    {
    //        foreach (var val in type.m_values)
    //        {
    //            if (val.Name == name) return val;
    //        }
    //    }
    //    return null;
    //}

    #region Creation and Populating

    public ICreator Setup()
    {
        if (m_creator == null)
        {
            m_creator = new Creator(this);
            return m_creator;
        }
        else
        {
            throw new General.OperationNotAllowedException("The soft-enum type has already been created.");
        }
    }

    private void Update(SoftEnum[] values)
    {
        m_values = values;
        if (this.Populated != null) this.Populated(this, EventArgs.Empty);
    }

    public interface ICreator : IDisposable
    {
        SoftEnumType Type { get; }
        IValuePopulator Populate(StepBro.Core.Logging.ILogger logger);
    }

    public interface IValuePopulator : IDisposable
    {
        SoftEnum AddEntry(string name, int value);
    }

    public event EventHandler Populating;
    public event EventHandler Populated;

    private void NotifyPopulatorCreated()
    {
        if (this.Populating != null) this.Populating(this, EventArgs.Empty);
    }

    private class Creator : ICreator
    {
        SoftEnumType m_type;
        private IValuePopulator m_populator = null;

        public Creator(SoftEnumType type)
        {
            m_type = type;
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

        public IValuePopulator Populate(StepBro.Core.Logging.ILogger logger)
        {
            if (m_populator != null)
            {
                throw new General.OperationNotAllowedException("The soft-enum is already being populated.");
            }

            m_populator = new ValuePopulator(this, logger);
            m_type.NotifyPopulatorCreated();
            return m_populator;
        }


        private class ValuePopulator : IValuePopulator
        {
            Creator m_creator;
            StepBro.Core.Logging.ILogger m_logger;
            List<SoftEnum> m_values = new List<SoftEnum>();

            public ValuePopulator(Creator creator, StepBro.Core.Logging.ILogger logger)
            {
                m_creator = creator;
                m_logger = logger;
            }

            public SoftEnum AddEntry(string name, int value)
            {
                var v = m_creator.Type.m_template.CreateNew(name, value, m_values.Count);
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