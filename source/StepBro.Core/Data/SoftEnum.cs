using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Base class for SoftEnum, used to allow anonymous access to soft-enum values.
    /// </summary>
    public class SoftEnumBase
    {
        SoftEnumType m_type;
        private string m_name;
        private int m_value;
        private int m_index;

        internal SoftEnumBase(SoftEnumType type, string name, int value, int index)
        {
            m_type = type;
            m_name = name;
            m_value = value;
            m_index = index;
        }

        public SoftEnumType Type
        {
            get
            {
                return m_type;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public int Value
        {
            get
            {
                return m_value;
            }
        }

        public int Index
        {
            get
            {
                return m_index;
            }
        }
    }

    public class SoftEnum<TType> : SoftEnumBase where TType : SoftEnumType, new()
    {
        internal SoftEnum(TType type, string name, int value, int index) : base(type, name, value, index) { }

        private static TType g_type = null;
        public static SoftEnumType.ITypeCreator<TType> CreateType(string nameSpace, string name)
        {
            var type = CreateSoftEnumType();
            return type.SetupSoftEnumType<TType>(nameSpace, name);
        }

        public static SoftEnumType.ITypeCreator<TType> CreateType()
        {
            return CreateType(typeof(TType).Namespace, typeof(TType).Name);
        }

        #region Conversion

        public static SoftEnum<TType> FromString(string name)
        {
            var type = GetSoftEnumType();
            return type.FromString<TType>(name);
        }

        public static implicit operator SoftEnum<TType>(string name)
        {
            return FromString(name);
        }

        public static SoftEnum<TType> FromValue(int value)
        {
            var type = GetSoftEnumType();
            return type.FromValue<TType>(value);
        }

        public static SoftEnum<TType> FromValue(long value)
        {
            var type = GetSoftEnumType();
            return type.FromValue<TType>((int)value);
        }

        public static implicit operator SoftEnum<TType>(int value)
        {
            return FromValue(value);
        }

        public static implicit operator SoftEnum<TType>(long value)
        {
            return FromValue(value);
        }

        #endregion

        private static TType CreateSoftEnumType()
        {
            if (g_type == null)
            {
                g_type = new TType();
            }
            else
            {
                throw new General.OperationNotAllowedException("The soft-enum type (" + typeof(TType).FullName + ") has already been created.");
            }
            return g_type;
        }

        private static TType GetSoftEnumType()
        {
            if (g_type == null)
            {
                throw new General.OperationNotAllowedException("The soft-enum type (" + typeof(TType).FullName + ") has not been created/initialized yet.");
            }
            return g_type;
        }
    }
}
