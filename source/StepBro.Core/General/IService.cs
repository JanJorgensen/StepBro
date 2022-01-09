using System;
using System.Collections.Generic;
using StepBro.Core.Tasks;

namespace StepBro.Core
{
    public enum ServiceState { Initial, Initialized, Started, Stopped, InitFailed, StartFailed, StopFailed }

    public interface IService
    {
        Type ServiceType { get; }
        ServiceState State { get; }
        object ServiceObject { get; }
        string Name { get; }
        IEnumerable<Type> Dependencies { get; }
        void Initialize(ServiceManager manager, ITaskContext context);
        void Start(ServiceManager manager, ITaskContext context);
        void Stop(ServiceManager manager, ITaskContext context);
    }

    /// <summary>
    /// Base class for a service. This class encapsulates the service housekeeping, to ensure users cannot directly start or stop the service.
    /// </summary>
    /// <typeparam name="TService">The public service type (typically interface) users will know the service by.</typeparam>
    /// <typeparam name="TThis">The actual type of the parent class implementing the service.</typeparam>
    public abstract class ServiceBase<TService, TThis> where TThis : ServiceBase<TService, TThis>, TService
    {
        private class MyServiceInterface : IService
        {
            private TThis m_service;
            private readonly Type[] m_dependencies;

            public MyServiceInterface(TThis service, string name, Type[] dependencies)
            {
                m_service = service;
                this.Name = name;
                m_dependencies = dependencies;
            }

            public IEnumerable<Type> Dependencies
            {
                get
                {
                    return m_dependencies;
                }
            }

            public string Name { get; }

            public object ServiceObject
            {
                get
                {
                    return m_service;
                }
            }

            public Type ServiceType
            {
                get
                {
                    return typeof(TService);
                }
            }

            public ServiceState State { get; protected set; } = ServiceState.Initial;

            public void Initialize(ServiceManager manager, ITaskContext context)
            {
                try
                {
                    m_service.Initialize(manager, context);
                    this.State = ServiceState.Initialized;
                }
                catch
                {
                    this.State = ServiceState.InitFailed;
                    throw;
                }
            }

            public void Start(ServiceManager manager, ITaskContext context)
            {
                try
                {
                    m_service.Start(manager, context);
                    this.State = ServiceState.Started;
                }
                catch
                {
                    this.State = ServiceState.StartFailed;
                    throw;
                }
            }

            public void Stop(ServiceManager manager, ITaskContext context)
            {
                try
                {
                    m_service.Stop(manager, context);
                    this.State = ServiceState.Stopped;
                }
                catch
                {
                    this.State = ServiceState.StopFailed;
                    throw;
                }
            }
        }

        private readonly MyServiceInterface m_myInterface;

        public ServiceBase(string name, out IService serviceAccess, params Type[] dependencies)
        {
            m_myInterface = new MyServiceInterface(this as TThis, name, dependencies);
            serviceAccess = m_myInterface as IService;
        }

        /// <summary>
        /// Constructor for using when the derived object is not actually used as a service.
        /// </summary>
        protected ServiceBase()
        {
            m_myInterface = null;
        }

        protected virtual void Initialize(ServiceManager manager, ITaskContext context) { }

        protected virtual void Start(ServiceManager manager, ITaskContext context) { }

        protected virtual void Stop(ServiceManager manager, ITaskContext context) { }

        protected void ExpectServiceStarted()
        {
            if (m_myInterface.State != ServiceState.Started)
            {
                throw new Exception("Service " + m_myInterface.Name + " is not started!");
            }
        }
    }
}
