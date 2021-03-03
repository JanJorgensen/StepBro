using StepBro.Core.Controls;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Api;

namespace StepBro.UI.Panels
{
    public enum PanelCreationOption { NotPossible, NoMoreCanBeCreated, Possible }

    internal class CustomPanelManager : ServiceBase<ICustomPanelManager, CustomPanelManager>, ICustomPanelManager, IAddonTypeHandler
    {
        private class PanelState
        {
            public List<CustomPanelInstanceData> CreatedPanels = new List<CustomPanelInstanceData>();
        }

        private List<Type> m_creatorTypes = new List<Type>();
        private List<CustomPanelCreator> m_creators = new List<CustomPanelCreator>();
        private Dictionary<CustomPanelType, PanelState> m_panelStates = new Dictionary<CustomPanelType, PanelState>();
        private IDynamicObjectManager m_dynamicObjectManager = null;

        public CustomPanelManager(out IService serviceAccess) :
            base("ObjectPanelManager", out serviceAccess, typeof(IAddonManager), typeof(IDynamicObjectManager))
        {
        }

        #region Initialization

        protected override void Initialize(ServiceManager manager, ITaskContext context)
        {
            manager.Get<IAddonManager>().AddTypeHandler(this);
            m_dynamicObjectManager = manager.Get<IDynamicObjectManager>();
        }

        public bool CheckForSpecialHandling(Type type)
        {
            if (type.IsClass && typeof(CustomPanelCreator).IsAssignableFrom(type))
            {
                m_creatorTypes.Add(type);
                try
                {
                    var creator = Activator.CreateInstance(type) as CustomPanelCreator;
                    if (creator != null)
                    {
                        m_creators.Add(creator);
                    }
                }
                catch
                {
                    // TODO: log or register this
                }
                return true;    
            }
            else
            {
                return false;
            }
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            var deadCreators = new List<CustomPanelCreator>();
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

        internal void AddPanelCreator(CustomPanelCreator creator)
        {
            if (m_creators == null) m_creators = new List<CustomPanelCreator>();
            m_creators.Add(creator);
        }

        private IEnumerable<CustomPanelCreator> ListPanelCreators()
        {
            if (m_creators == null) yield break;
            else
            {
                foreach (var c in m_creators) yield return c;
            }
        }

        #endregion

        public IEnumerable<CustomPanelType> ListPanelTypes()
        {
            foreach (var c in ListPanelCreators())
            {
                foreach (var p in c.ListPanels()) yield return p;
            }
        }

        public CustomPanelInstanceData CreateStaticPanel(string type)
        {
            throw new NotImplementedException();
        }

        public CustomPanelInstanceData CreateStaticPanel(CustomPanelType type)
        {
            throw new NotImplementedException();
        }

        public CustomPanelInstanceData CreateObjectPanel(string type, string objectReference)
        {
            var panelType = this.FindPanelType(type);
            if (panelType == null)
            {
                return new CustomPanelInstanceData(type, objectReference);
            }
            else return this.CreateObjectPanel(panelType, objectReference);
        }

        public CustomPanelInstanceData CreateObjectPanel(CustomPanelType type, string objectReference)
        {
            var foundObject = m_dynamicObjectManager.TryFindObject(objectReference);

            if (foundObject == null)
            {
                PanelState state;
                if (!m_panelStates.TryGetValue(type, out state))
                {
                    state = new PanelState();
                    m_panelStates[type] = state;
                }

                var createdCount = state.CreatedPanels.Where(p => p.TargetObjectReference.Equals(objectReference)).Count();
                if (createdCount == 0 || type.AllowMultipleInstances)
                {
                    var panel = new CustomPanelInstanceData(type);
                    panel.SetObjectReference(objectReference);      // Who's resolving this, then?
                    if (panel != null)
                    {
                        state.CreatedPanels.Add(panel);
                        panel.Disposed += Panel_Disposed;
                    }
                    return panel;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return CreateObjectPanel(type, foundObject);
            }
        }

        public CustomPanelInstanceData CreateObjectPanel(CustomPanelType type, IObjectContainer container)
        {
            PanelState state;
            if (!m_panelStates.TryGetValue(type, out state))
            {
                state = new PanelState();
                m_panelStates[type] = state;
            }
            var createdCount = state.CreatedPanels.Where(p => Object.ReferenceEquals(p.BoundObjectContainer, container)).Count();
            if (createdCount == 0 || type.AllowMultipleInstances)
            {
                var panel = new CustomPanelInstanceData(type, container);
                if (panel != null)
                {
                    state.CreatedPanels.Add(panel);
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
            var panel = sender as CustomPanelInstanceData;
            panel.Disposed -= Panel_Disposed;
            var type = panel.PanelType;
            m_panelStates[type].CreatedPanels.Remove(panel);
        }

        public PanelCreationOption GetPanelCreationOption(CustomPanelType type, object @object = null)
        {
            return PanelCreationOption.Possible;
        }

        public IEnumerable<CustomPanelInstanceData> ListCreatedPanels()
        {
            foreach (var ps in m_panelStates.Keys)
            {
                foreach (var p in m_panelStates[ps].CreatedPanels) yield return p;
            }
        }

        public CustomPanelType FindPanelType(string name)
        {
            foreach (var p in ListPanelTypes())
            {
                if (string.Equals(p.TypeIdentification, name))
                {
                    return p;
                }
            }
            return null;
        }
    }
}
