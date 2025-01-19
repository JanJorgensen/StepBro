using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Tasks;

namespace StepBro.Core
{
    public sealed class ServiceManager
    {
        public interface IServiceManagerAdministration
        {
            ServiceManager Manager { get; }
            void StartServices(ITaskContext context);
            void StopServices(ITaskContext context, bool reset = false);
        }

        public enum ServiceManagerState { Initial, Initialized, Starting, Started, InitOrStartFailed, Stopped, StopFailed }

        private List<IService> m_services = new List<IService>();
        private readonly object m_lock = new object();
        private static ServiceManager g_manager = null;

        private ServiceManager(out IServiceManagerAdministration admin)
        {
            admin = new ServiceManagerAdministration(this);
        }

        public static ServiceManager Global { get { return g_manager; } }

        public ServiceManagerState State { get; private set; } = ServiceManagerState.Initial;

        static public IServiceManagerAdministration Create()
        {
            IServiceManagerAdministration admin;
            g_manager = new ServiceManager(out admin);
            return admin;
        }

        internal static void Dispose()
        {
            g_manager = null;
        }

        private class ServiceManagerAdministration : IServiceManagerAdministration
        {
            public ServiceManager Manager { get; private set; }
            public ServiceManagerAdministration(ServiceManager manager) { this.Manager = manager; }

            public void StartServices(ITaskContext context)
            {
                this.Manager.StartServices(context);
            }

            public void StopServices(ITaskContext context, bool reset)
            {
                this.Manager.StopServices(context, reset);
                g_manager = null;
            }
        }

        internal IEnumerable<Type> ListServices()
        {
            foreach (var s in m_services) yield return s.ServiceType;
        }

        public IEnumerable<Type> ListUnregisteredDependencies()
        {
            lock (m_lock)
            {
                return m_services.SelectMany(s => s.Dependencies.Where(d => this.TryGetIndexOfService(d) < 0)).Distinct();
            }
        }

        public void Register(IService service, bool forceReplace = false)
        {
            lock (m_lock)
            {
                if (this.State != ServiceManagerState.Initial)
                {
                    throw new Exception("Service registration after other services has already been started (or stopped again).");
                }
                InsertServiceInList(m_services, service, forceReplace);
            }
        }

        internal static void InsertServiceInList(List<IService> list, IService service, bool forceReplace)
        {
            var allServices = new List<IService>(list);
            if (allServices.Select(s => s.ServiceType).Contains(service.ServiceType))
            {
                // Service type already registered. So what now?
                int index = allServices.FindIndex(s => s.ServiceType == service.ServiceType);
                if (Object.ReferenceEquals(allServices[index], service))
                {
                    throw new Exception("Service already registered.");
                }
                if (forceReplace)
                {
                    allServices[index] = service;
                }
                else
                {
                    throw new Exception($"Service of the same service type already registered ({allServices[index].ServiceObject.GetType().FullName}).");
                }
            }
            else
            {
                allServices.Add(service);
            }
            var servicePicker = new ListElementPicker<IService>(allServices);
            var newList = new List<IService>();

            int lastCount = -1;
            while (newList.Count != lastCount)
            {
                lastCount = newList.Count;
                foreach (var s in servicePicker.ListUnpicked())
                {
                    if (s.Dependencies.Count() == 0)
                    {
                        newList.Add(servicePicker.Pick());
                    }
                    else
                    {
                        var allDepsFound = true;
                        var deps = (s.OptionalDependencies != null) ? s.Dependencies.Concat(s.OptionalDependencies) : s.Dependencies;
                        foreach (var d in deps)
                        {
                            if (allServices.Exists(x => x.ServiceType == d) && !newList.Exists(x => x.ServiceType == d))
                            {
                                // If the dependency exists in the list but not already added to the new list, don't add this service yet.
                                allDepsFound = false;
                                break;
                            }
                        }
                        if (allDepsFound)
                        {
                            newList.Add(servicePicker.Pick());
                        }
                    }
                }
            }

            newList.AddRange(servicePicker.ListUnpicked());     // Insert those services with unresolved dependencies
            list.Clear();
            list.AddRange(newList);
        }

        public T Get<T>() where T : class
        {
            lock (m_lock)
            {
                if (this.State != ServiceManagerState.Started && this.State != ServiceManagerState.Starting)
                {
                    throw new NotSupportedException($"Service manager cannot return services; current state: {this.State}.");
                }
                var service = m_services.FirstOrDefault(s => s.ServiceType == typeof(T));
                if (service != null)
                {
                    if (service.State < ServiceState.Initialized)
                    {
                        throw new Exception("");
                    }
                }
                return service?.ServiceObject as T;
            }
        }

        private int TryGetIndexOfService(Type type)
        {
            return m_services.FindIndex(s => s.ServiceType == type);
        }

        private void StartServices(ITaskContext context)
        {
            if (this.State == ServiceManagerState.Initial)
            {
                this.State = ServiceManagerState.Starting;
                if (this.ListUnregisteredDependencies().Any())
                {
                    string failed = String.Join(", ", this.ListUnregisteredDependencies().Select(t => t.Name));
                    string message = $"Failed starting services. Some dependencies are missing ({failed}).";
                    context.UpdateStatus(message, 0);
                    this.State = ServiceManagerState.InitOrStartFailed;
                    throw new Exception(message);
                }

                context.ProgressSetup(0, m_services.Count * 2, null);
                long i = 0;
                foreach (var s in m_services)
                {
                    try
                    {
                        context.UpdateStatus("Initializing service " + s.Name, i);
                        s.Initialize(this, context);
                    }
                    catch (Exception ex)
                    {
                        context.UpdateStatus("Failed initializing service " + s.Name, i);
                        this.State = ServiceManagerState.InitOrStartFailed;
                        throw new Exception("Failed initializing service " + s.Name, ex);
                    }
                    i++;
                }
                foreach (var s in m_services)
                {
                    try
                    {
                        context.UpdateStatus("Starting service " + s.Name, i);
                        s.Start(this, context);
                    }
                    catch (Exception ex)
                    {
                        context.UpdateStatus("Failed starting service " + s.Name, i);
                        this.State = ServiceManagerState.InitOrStartFailed;
                        throw new Exception("Failed starting service " + s.Name, ex);
                    }
                    i++;
                }
                context.UpdateStatus("Started all services", m_services.Count * 2);
                this.State = ServiceManagerState.Started;
            }
            else
            {
                context.UpdateStatus($"Failed to start, because manager is in state '{this.State}'.", 0);
                throw new Exception("Failed starting services. Some dependencies are missing.");
            }
        }
        private void StopServices(ITaskContext context, bool reset)
        {
            if (this.State == ServiceManagerState.Started || this.State == ServiceManagerState.InitOrStartFailed)
            {
                context.ProgressSetup(0, m_services.Count, null);
                List<IService> reversed = new List<IService>(m_services);
                reversed.Reverse();
                long i = 0;
                bool fails = false;
                string exceptionMessage = "";
                foreach (var s in reversed)
                {
                    if (s.State == ServiceState.Started)
                    {
                        try
                        {
                            context.UpdateStatus("Stopping service " + s.Name, i);
                            s.Stop(this, context);
                        }
                        catch (Exception e)
                        {
                            exceptionMessage += e.Message + " ";
                            fails = true;
                        }
                    }
                    i++;
                }
                if (fails)
                {
                    context.UpdateStatus("Failed stopping all services", m_services.Count);
                    if (this.State != ServiceManagerState.InitOrStartFailed)      // If start failed, keep that state
                    {
                        this.State = ServiceManagerState.StopFailed;
                    }
                    throw new Exception("Failed starting services. Some dependencies are missing. " + exceptionMessage);
                }
                else
                {
                    context.UpdateStatus("Stopped all services", m_services.Count);
                    if (this.State != ServiceManagerState.InitOrStartFailed)      // If start failed, keep that state
                    {
                        this.State = ServiceManagerState.Stopped;
                    }
                }
            }
            else
            {
                context.UpdateStatus($"Failed to stop, because manager is in '{this.State}' state.", 0);
            }

            if (reset)
            {
                m_services = new List<IService>();
                g_manager = null;
                this.State = ServiceManagerState.Initial;
            }
        }
    }
}
