using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Data
{
    public interface IValueContainerOwnerAccess : IAvailability
    {
        IValueContainer Container { get; }
        void SetUniqueID(int id);
        void SetValue(object value, ILogger logger);
        bool DataCreated { get; }
        bool InitNeeded { get; set; }
        void SetAccessModifier(AccessModifier access);
        VariableContainerAction DataResetter { get; set; }
        VariableContainerAction DataCreator { get; set; }
        VariableContainerAction DataInitializer { get; set; }
        IScriptFile File { get; set; }
        int FileLine { get; set; }
        int FileColumn { get; set; }
        int CodeHash { get; set; }
        Dictionary<string,object> Tags { get; set; }
    }

    public delegate bool VariableContainerAction(IScriptFile file, IValueContainerOwnerAccess container, ILogger logger);

    public class VariableContainer<T> : AvailabilityBase, IValueContainer<T>, IValueContainerRich, IObjectContainer, INameable
    {
        #region Owner Access
        private class OwnerAccessor : IValueContainerOwnerAccess
        {
            private VariableContainer<T> m_container;
            private bool m_dataIsSet = false;

            public event EventHandler Disposed;
            public event EventHandler Disposing;

            public OwnerAccessor(VariableContainer<T> container)
            {
                m_container = container;
                m_container.m_owner = this;
            }

            public IValueContainer Container
            {
                get
                {
                    return m_container;
                }
            }

            public void SetUniqueID(int id)
            {
                m_container.SetID(id);
            }

            public void SetValue(object value, ILogger logger = null)
            {
                m_container.SetValue(value, logger, true);
                m_dataIsSet = (value!= null);
            }

            public bool DataCreated { get { return m_dataIsSet; } }
            public bool InitNeeded { get; set; } = true;

            public void SetAccessModifier(AccessModifier access)
            {
                m_container.SetAccessModifier(access);
            }

            public VariableContainerAction DataResetter { get; set; } = null;
            public VariableContainerAction DataCreator { get; set; } = null;
            public VariableContainerAction DataInitializer { get; set; } = null;

            public IScriptFile File { get; set; }
            public int FileLine { get; set; }
            public int FileColumn { get; set; }
            public int CodeHash { get; set; }
            public Dictionary<string,object> Tags { get; set; } = null;

            public bool IsStillValid { get { return disposedValue == false; } }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        this.Disposing?.Invoke(this, EventArgs.Empty);
                        m_container.Dispose();
                        m_container = null;
                    }
                    // TODO: set large fields to null.

                    this.Disposed?.Invoke(this, EventArgs.Empty);
                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(true);
            }
            #endregion
        }
        #endregion

        private OwnerAccessor m_owner;
        private readonly string m_namespace;
        private readonly string m_name;
        private readonly TypeReference m_declaredType;
        private T m_value;
        private readonly bool m_readonly;
        private AccessModifier m_access = AccessModifier.None;
        private int m_valueChangeIndex = 0;
        private readonly object m_sync = new object();
        private int m_uniqueID;

        public event EventHandler ValueChanged;
        public event EventHandler ObjectReplaced;

        public VariableContainer(string @namespace, string name, TypeReference declaredType, T value, bool readOnly)
        {
            m_namespace = @namespace;
            m_name = name;
            m_declaredType = declaredType;
            m_value = value;
            m_readonly = readOnly;
            m_uniqueID = VariableContainer.GetIdentifier();
        }

        protected override void DoDispose(bool disposing)
        {
            base.DoDispose(disposing);

            if (m_value is IDisposable v)
            {
                v.Dispose();
            }
        }

        public static IValueContainerOwnerAccess Create(string @namespace, string name, TypeReference declaredType, T value, bool readOnly)
        {
            var container = new VariableContainer<T>(@namespace, name, declaredType, value, readOnly);
            return new OwnerAccessor(container);
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public TypeReference DataType
        {
            get
            {
                return m_declaredType;
            }
        }

        public bool IsReadonly { get { return m_readonly; } }

        public AccessModifier AccessProtection { get { return m_access; } }

        public int ValueChangeIndex
        {
            get
            {
                return m_valueChangeIndex;
            }
        }

        public object Sync
        {
            get
            {
                return m_sync;
            }
        }

        public int UniqueID
        {
            get
            {
                return m_uniqueID;
            }
        }

        private void SetID(int id)
        {
            m_uniqueID = id;
        }

        private void SetAccessModifier(AccessModifier access)
        {
            m_access = access;
        }

        public string FullName
        {
            get
            {
                return String.IsNullOrEmpty(m_namespace) ? m_name : (m_namespace + "." + m_name);
            }
        }

        public IdentifierType Type
        {
            get
            {
                return IdentifierType.VariableContainer;
            }
        }

        public object Reference
        {
            get
            {
                return this;
            }
        }

        public string SourceFile { get; internal set; } = null;

        public int SourceLine { get; internal set; } = -1;

        object IObjectContainer.Object { get { return m_value; } }

        public object GetValue(ILogger logger = null)
        {
            return m_value;
        }

        public void SetValue(object value, ILogger logger = null)
        {
            this.SetValue(value, logger, false);
        }

        internal void SetValue(object value, ILogger logger, bool force)
        {
            lock (m_sync)
            {
                if (!force && m_readonly)
                {
                    throw new NotSupportedException("Setting the variable is not allowed, because it is read-only.");
                }
                if (value == null)
                {
                    if (typeof(T).IsValueType)
                    {
                        throw new ArgumentException("The value is not a compatible type for the container.");
                    }
                }
                else
                {
                    if (typeof(T).IsAssignableFrom(value.GetType()) == false)
                    {
                        throw new ArgumentException("The value is not a compatible type for the container.");
                    }
                }
                this.SetValueDirect((T)value, logger);
            }
        }

        internal void SetValueDirect(T value, ILogger logger = null)
        {
            if (typeof(T).IsPrimitive)
            {
                if (m_value != null && m_value.Equals(value)) return;
            }
            else
            {
                if (m_value == null)
                {
                    if (value == null) return;
                }
                else if (value == null || Object.ReferenceEquals(m_value, value))
                {
                    return;
                }
            }

            m_value = value;
            m_valueChangeIndex++;
            this.ValueChanged?.Invoke(this, EventArgs.Empty);
            this.ObjectReplaced?.Invoke(this, EventArgs.Empty);
        }

        public T GetTypedValue(ILogger logger = null)
        {
            return m_value;
        }

        public T Modify(ValueContainerModifier<T> modifier, ILogger logger = null)
        {
            lock (m_sync)
            {
                if (m_readonly)
                {
                    throw new NotSupportedException("Setting the variable is not allowed, because it is read-only.");
                }
                T vNew, vRet;
                vRet = modifier(m_value, out vNew);
                this.SetValueDirect(vNew);
                return vRet;
            }
        }
    }

    public static class VariableContainer
    {
        private static int m_nextIdentifier = 100;
        internal static int GetIdentifier()
        {
            return m_nextIdentifier++;
        }

        private static IValueContainerOwnerAccess CreateContainer(string @namespace, string name, TypeReference type, object defaultValue, bool readOnly)
        {
            if (type.Equals(typeof(bool))) return VariableContainer<Boolean>.Create(@namespace, name, TypeReference.TypeBool, (bool)defaultValue, readOnly);
            else if (type.Equals(typeof(long))) return VariableContainer<Int64>.Create(@namespace, name, TypeReference.TypeInt64, (long)defaultValue, readOnly);
            else if (type.Equals(typeof(int))) return VariableContainer<Int32>.Create(@namespace, name, TypeReference.TypeInt32, (int)defaultValue, readOnly);
            else if (type.Equals(typeof(double))) return VariableContainer<Double>.Create(@namespace, name, TypeReference.TypeDouble, (double)defaultValue, readOnly);
            else if (type.Equals(typeof(string))) return VariableContainer<String>.Create(@namespace, name, TypeReference.TypeString, (string)defaultValue, readOnly);
            else if (type.Equals(typeof(DateTime))) return VariableContainer<DateTime>.Create(@namespace, name, TypeReference.TypeDateTime, (DateTime)defaultValue, readOnly);
            else if (type.Equals(typeof(TimeSpan))) return VariableContainer<TimeSpan>.Create(@namespace, name, TypeReference.TypeTimeSpan, (TimeSpan)defaultValue, readOnly);
            else
            {
                Type containertype = typeof(VariableContainer<>).MakeGenericType(type.Type);
                var method = containertype.GetMethod("Create");
                var result = method.Invoke(null, new object[] { @namespace, name, type, defaultValue, readOnly }) as IValueContainerOwnerAccess;
                if (result == null) throw new NullReferenceException("Created Container");
                return result;
            }
        }

        public static IValueContainerOwnerAccess Create(string @namespace, string name, TypeReference type, bool readOnly)
        {
            object defaultValue = null;
            if (type.Type.IsValueType)
            {
                defaultValue = Activator.CreateInstance(type.Type);
            }
            return CreateContainer(@namespace, name, type, defaultValue, readOnly);
        }
    }
}
