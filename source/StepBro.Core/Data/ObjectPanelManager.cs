using StepBro.Core.Controls;
using StepBro.Core.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Data
{
    public enum PanelCreationOption { NotPossible, NoMoreCanBeCreated, Possible}

    internal class ObjectPanelManager : ServiceBase<IObjectPanelManager, ObjectPanelManager>, IObjectPanelManager
    {
        private class PanelState
        {
            public List<ObjectPanel> CreatedPanels = new List<ObjectPanel>();
        }

        private List<ObjectPanelCreator> m_creators = null;
        private Dictionary<ObjectPanelInfo, PanelState> m_panelStates = new Dictionary<ObjectPanelInfo, PanelState>();

        public ObjectPanelManager(out IService serviceAccess) :
            base("ObjectPanelManager", out serviceAccess, typeof(Api.IAddonManager))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_creators = manager.Get<Api.IAddonManager>().GetPanelCreators().ToList();
            foreach (var creator in m_creators)
            {
                try
                {
                    creator.UpdatePanelsList();
                }
                catch { }
            }
        }

        private IEnumerable<ObjectPanelCreator> ListPanelCreators()
        {
            if (m_creators == null) yield break;
            else
            {
                foreach (var c in m_creators) yield return c;
            }
        }

        public IEnumerable<ObjectPanelInfo> ListPanelTypes()
        {
            foreach (var c in this.ListPanelCreators())
            {
                foreach (var p in c.ListPanels()) yield return p;
            }
        }

        public ObjectPanel CreatePanel(ObjectPanelInfo type, IObjectContainer container = null)
        {
            System.Diagnostics.Debug.Assert(container == null || type.IsObjectPanel);
            PanelState state;
            if (!m_panelStates.TryGetValue(type, out state))
            {
                state = new PanelState();
                m_panelStates[type] = state;
            }

            if (state.CreatedPanels.Count == 0 || type.AllowMultipleInstances)
            {
                var panel = type.CreatePanel(container);
                state.CreatedPanels.Add(panel);
                return panel;
            }
            else
            {
                return null;
            }
        }

        public PanelCreationOption GetPanelCreationOption(ObjectPanelInfo type, object @object = null)
        {
            return PanelCreationOption.Possible;
        }

        public IEnumerable<ObjectPanel> ListCreatedPanels()
        {
            throw new System.NotImplementedException();
        }

        public ObjectPanelInfo FindPanel(string name)
        {
            foreach (var p in this.ListPanelTypes())
            {
                if (string.Equals(p.TypeIdentification, name, System.StringComparison.InvariantCulture))
                {
                    return p;
                }
            }
            return null;
        }
    }
}
