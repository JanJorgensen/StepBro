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

    internal class ObjectPanelManager : ServiceBase<IObjectPanelManager, ObjectPanelManager>, IObjectPanelManager, IAddonTypeHandler
    {
        private class PanelState
        {
            public List<ObjectPanel> CreatedPanels = new List<ObjectPanel>();
        }

        private List<Type> m_creatorTypes = new List<Type>();
        private List<ObjectPanelCreator> m_creators = new List<ObjectPanelCreator>();
        private Dictionary<ObjectPanelInfo, PanelState> m_panelStates = new Dictionary<ObjectPanelInfo, PanelState>();

        public ObjectPanelManager(out IService serviceAccess) :
            base("ObjectPanelManager", out serviceAccess, typeof(IAddonManager))
        {
        }

        protected override void Initialize(ServiceManager manager, ITaskContext context)
        {
            manager.Get<IAddonManager>().AddTypeHandler(this);
        }

        public bool CheckForSpecialHandling(Type type)
        {
            if (type.IsClass && typeof(ObjectPanelCreator).IsAssignableFrom(type))
            {
                m_creatorTypes.Add(type);
                try
                {
                    var creator = Activator.CreateInstance(type) as ObjectPanelCreator;
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

        //private Controls.WinForms.ObjectPanel CreateTheWinFormsPanel(ObjectPanelInfo type)
        //{
        //    PanelState state;
        //    if (!m_panelStates.TryGetValue(type, out state))
        //    {
        //        state = new PanelState();
        //        m_panelStates[type] = state;
        //    }

        //    if (state.CreatedWinFormsPanels.Count == 0 || type.AllowMultipleInstances)
        //    {
        //        var panel = type.CreateWinFormsPanel();
        //        if (panel != null)
        //        {
        //            state.CreatedWinFormsPanels.Add(panel);
        //            panel.PanelType = type;
        //            panel.Disposed += Panel_Disposed;
        //        }
        //        return panel;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //private void Panel_Disposed(object sender, EventArgs e)
        //{
        //    var panel = sender as Controls.WinForms.ObjectPanel;
        //    panel.Disposed -= Panel_Disposed;
        //    var type = panel.PanelType;
        //    m_panelStates[type].CreatedWinFormsPanels.Remove(panel);
        //}

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

        //public IEnumerable<Controls.WinForms.ObjectPanel> ListCreatedPanelsWinForms()
        //{
        //    foreach (var ps in m_panelStates.Keys)
        //    {
        //        foreach (var p in m_panelStates[ps].CreatedWinFormsPanels) yield return p;
        //    }
        //}

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
