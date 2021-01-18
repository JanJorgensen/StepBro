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
            public List<Controls.ObjectPanel> CreatedPanels = new List<Controls.ObjectPanel>();
            public List<Controls.WinForms.ObjectPanel> CreatedWinFormsPanels = new List<Controls.WinForms.ObjectPanel>();
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
                    context.Logger.LogError(GetType().Name, $"Error updating panel list for {creator.GetType().FullName}. {ex}");
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
            foreach (var c in ListPanelCreators())
            {
                foreach (var p in c.ListPanels()) yield return p;
            }
        }

        public ObjectPanel CreateStaticPanel(ObjectPanelInfo type)
        {
            throw new NotImplementedException();
        }

        public ObjectPanel CreateObjectPanel(ObjectPanelInfo type, IObjectContainer container)
        {
            throw new NotImplementedException();
        }

        public ObjectPanel CreateObjectPanel(ObjectPanelInfo type, string objectReference)
        {
            throw new NotImplementedException();
        }

        public Controls.WinForms.ObjectPanel CreateStaticWinFormsPanel(ObjectPanelInfo type)
        {
            System.Diagnostics.Debug.Assert(!type.IsObjectPanel);
            return CreateTheWinFormsPanel(type);
        }

        public Controls.WinForms.ObjectPanel CreateObjectWinFormsPanel(ObjectPanelInfo type, IObjectContainer container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            System.Diagnostics.Debug.Assert(type.IsObjectPanel);
            var panel = CreateTheWinFormsPanel(type);
            if (panel != null)
            {
                panel.Bind(container);
            }
            return panel;
        }

        public Controls.WinForms.ObjectPanel CreateObjectWinFormsPanel(ObjectPanelInfo type, string objectReference)
        {
            System.Diagnostics.Debug.Assert(type.IsObjectPanel);
            var panel = CreateTheWinFormsPanel(type);
            if (panel != null)
            {
                panel.SetObjectReference(objectReference);
            }
            return panel;
        }

        private Controls.WinForms.ObjectPanel CreateTheWinFormsPanel(ObjectPanelInfo type)
        {
            PanelState state;
            if (!m_panelStates.TryGetValue(type, out state))
            {
                state = new PanelState();
                m_panelStates[type] = state;
            }

            if (state.CreatedWinFormsPanels.Count == 0 || type.AllowMultipleInstances)
            {
                var panel = type.CreateWinFormsPanel();
                if (panel != null)
                {
                    state.CreatedWinFormsPanels.Add(panel);
                    panel.PanelType = type;
                    panel.Disposed += Panel_Disposed;
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
            var panel = sender as Controls.WinForms.ObjectPanel;
            panel.Disposed -= Panel_Disposed;
            var type = panel.PanelType;
            m_panelStates[type].CreatedWinFormsPanels.Remove(panel);
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

        public IEnumerable<Controls.WinForms.ObjectPanel> ListCreatedPanelsWinForms()
        {
            foreach (var ps in m_panelStates.Keys)
            {
                foreach (var p in m_panelStates[ps].CreatedWinFormsPanels) yield return p;
            }
        }

        public ObjectPanelInfo FindPanel(string name)
        {
            foreach (var p in ListPanelTypes())
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
