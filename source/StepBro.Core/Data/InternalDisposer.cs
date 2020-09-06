using System;
using StepBro.Core.General;

namespace StepBro.Core.Data
{
    internal static class InternalDisposer
    {
        public static InternalDisposer<T> Create<T>(T obj) where T : class, IInternalDispose
        {
            return new InternalDisposer<T>(obj);
        }

        public static InternalDisposer<T> Disposer<T>(this T obj) where T : class, IInternalDispose
        {
            return new InternalDisposer<T>(obj);
        }
    }

    internal class InternalDisposer<T> : IDisposable where T : class, IInternalDispose
    {
        private T m_obj;

        public InternalDisposer(T obj)
        {
            m_obj = obj;
        }

        public void Dispose()
        {
            if (m_obj != null)
            {
                m_obj.InternalDispose();
                m_obj = null;
            }
        }

        public T Value { get { return m_obj; } }
    }
}
