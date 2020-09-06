using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Data
{
    internal class DynamicObjectManager : ServiceBase<IDynamicObjectManager, DynamicObjectManager>, IDynamicObjectManager
    {
        private List<IObjectHost> m_hosts = new List<IObjectHost>();

        public DynamicObjectManager(out IService serviceAccess) :
            base("DynamicObjectManager", out serviceAccess)
        {
        }

        public event EventHandler ObjectListChanged;

        public void RegisterObjectHost(IObjectHost host)
        {
            m_hosts.Add(host);
            host.ObjectContainerListChanged += this.Host_ObjectContainerListChanged;
        }

        public void DeRegisterObjectHost(IObjectHost host)
        {
            if (m_hosts.Contains(host))
            {
                m_hosts.Add(host);
                host.ObjectContainerListChanged -= this.Host_ObjectContainerListChanged;
            }
            else
            {
                throw new ArgumentException("Unknown object host.");
            }
        }

        private void Host_ObjectContainerListChanged(object sender, EventArgs e)
        {
            this.ObjectListChanged?.Invoke(this, e);
        }

        public IEnumerable<IObjectContainer> ListKnownObjects()
        {
            foreach (var host in m_hosts)
            {
                foreach (var c in host.ListObjectContainers())
                {
                    yield return c;
                }
            }
        }
        public IObjectContainer TryFindObject(string id)
        {
            return this.ListKnownObjects().FirstOrDefault(
                oc => string.Equals(oc.FullName, id, System.StringComparison.InvariantCulture));
        }
    }
}
