using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
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
        IEnumerable<Type> OptionalDependencies { get; }
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
            private readonly List<Type> m_dependencies;
            private List<Type> m_optionalDependencies = null;

            public MyServiceInterface(TThis service, string name, List<Type> dependencies)
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

            public IEnumerable<Type> OptionalDependencies
            {
                get
                {
                    return m_optionalDependencies;
                }
            }

            public void AddOptionalDependency(Type type)
            {
                if (m_optionalDependencies == null)
                {
                    m_optionalDependencies = new List<Type>();
                }
                m_optionalDependencies.Add(type);
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
            m_myInterface = new MyServiceInterface(this as TThis, name, dependencies.ToList());
            serviceAccess = m_myInterface as IService;
        }
        //public ServiceBase(string name, out IService serviceAccess, List<Type> firstSetOfDependencies, params Type[] dependencies)
        //{
        //    m_myInterface = new MyServiceInterface(this as TThis, name, firstSetOfDependencies.Concat(dependencies).ToList());
        //    serviceAccess = m_myInterface as IService;
        //}

        /// <summary>
        /// Constructor for using when the derived object is not actually used as a service.
        /// </summary>
        protected ServiceBase()
        {
            m_myInterface = null;
        }

        protected void AddOptionalDependency(Type type)
        {
            m_myInterface.AddOptionalDependency(type);
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

    public class ObjectReferenceService<TData> : ServiceBase<ObjectReferenceService<TData>, ObjectReferenceService<TData>>
    {
        private ObjectReferenceService(string name, TData data, out IService serviceAccess, Type[] dependencies) : base(name, out serviceAccess, dependencies) 
        {
            this.Data = data;
        }

        public TData Data { get; private set; }

        public static IService Create(string name, TData data, params Type[] dependencies)
        {
            IService service;
            var obj = new ObjectReferenceService<TData>(name, data, out service, dependencies);
            return service;
        }
    }
}
