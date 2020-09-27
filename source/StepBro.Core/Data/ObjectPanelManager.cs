using StepBro.Core.Controls;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Data
{
    public enum PanelCreationOption { NotPossible, NoMoreCanBeCreated, Possible }

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
            if (m_creators == null)
            {
                m_creators = manager.Get<Api.IAddonManager>().GetPanelCreators().ToList();
            }
            var deadCreators = new List<ObjectPanelCreator>();
            foreach (var creator in m_creators)
            {
                try
                {
                    creator.UpdatePanelsList();
                }
                catch (Exception ex)
                {
                    context.Logger.LogError(this.GetType().Name, $"Error updating panel list for {creator.GetType().FullName}. {ex}");
                    deadCreators.Add(creator);
                }
            }
            // Only continue with the ones that succeeded.
            foreach (var pc in deadCreators)
            {
                m_creators.Remove(pc);
            }
        }

        internal void AddPanelCreator(ObjectPanelCreator creator)
        {
            if (m_creators == null) m_creators = new List<ObjectPanelCreator>();
            m_creators.Add(creator);
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

        public Controls.ObjectPanel CreateStaticPanel(ObjectPanelInfo type)
        {
            System.Diagnostics.Debug.Assert(!type.IsObjectPanel);
            return this.CreateThePanel(type);
        }

        public ObjectPanel CreateObjectPanel(ObjectPanelInfo type, IObjectContainer container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            System.Diagnostics.Debug.Assert(type.IsObjectPanel);
            var panel = this.CreateThePanel(type);
            if (panel != null)
            {
                panel.Bind(container);
            }
            return panel;
        }

        public ObjectPanel CreateObjectPanel(ObjectPanelInfo type, string objectReference)
        {
            System.Diagnostics.Debug.Assert(type.IsObjectPanel);
            var panel = this.CreateThePanel(type);
            if (panel != null)
            {
                panel.SetObjectReference(objectReference);
            }
            return panel;
        }

        private ObjectPanel CreateThePanel(ObjectPanelInfo type)
        {
            PanelState state;
            if (!m_panelStates.TryGetValue(type, out state))
            {
                state = new PanelState();
                m_panelStates[type] = state;
            }

            if (state.CreatedPanels.Count == 0 || type.AllowMultipleInstances)
            {
                var panel = type.CreatePanel();
                if (panel != null)
                {
                    state.CreatedPanels.Add(panel);
                    panel.PanelType = type;
                    panel.Disposed += this.Panel_Disposed;
                }
                return panel;
            }
            else
            {
                return null;
            }
        }

        private void Panel_Disposed(object sender, EventArgs e)
        {
            var panel = sender as ObjectPanel;
            panel.Disposed -= this.Panel_Disposed;
            var type = panel.PanelType;
            m_panelStates[type].CreatedPanels.Remove(panel);
        }

        public PanelCreationOption GetPanelCreationOption(ObjectPanelInfo type, object @object = null)
        {
            return PanelCreationOption.Possible;
        }

        public IEnumerable<ObjectPanel> ListCreatedPanels()
        {
            foreach (var ps in m_panelStates.Keys)
            {
                foreach (var p in m_panelStates[ps].CreatedPanels) yield return p;
            }
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
