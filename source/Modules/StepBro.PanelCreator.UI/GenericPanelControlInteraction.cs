using Newtonsoft.Json.Linq;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xaml;
using System.Xml.Linq;

namespace StepBro.PanelCreator.UI
{
    public class GenericPanelControlInteraction : IPanelControlInteraction, StepBro.PanelCreator.IPanelElement
    {
        private IPanelControl m_panelControl;
        private Control m_control;
        private string m_parentProperty = null;
        private Tuple<string, PropertyInfo>[] m_childProperties;
        private Tuple<string, PropertyInfo>[] m_valuedProperties;

        string IPanelElement.PropertyName { get { return m_parentProperty; } }

        string IPanelElement.ElementName { get { return m_control.Name; } }

        string IPanelElement.ElementType { get {  return m_control.GetType().Name; } }

        public GenericPanelControlInteraction(IPanelControl control)
        {
            m_panelControl = control;
            m_control = (Control)control;
            m_childProperties = this.ListChildProperties().ToArray();
            m_valuedProperties = this.ListValueProperties().ToArray();
        }

        protected virtual IEnumerable<Tuple<string, PropertyInfo>> ListChildProperties()
        {
            yield break;
        }

        protected virtual IEnumerable<Tuple<string, PropertyInfo>> ListValueProperties()
        {
            foreach (var property in m_control.GetType().GetProperties())
            {
                if (property.PropertyType.IsPrimitive || property.PropertyType.IsEnum || property.PropertyType == typeof(string))
                {
                    yield return new Tuple<string, PropertyInfo>(property.Name, property);
                }
            }
        }

        #region IPanelControlInteraction Interface

        public IEnumerable<string> GetChildProperties()
        {
            foreach (var property in m_childProperties)
            {
                yield return property.Item1;
            }
        }

        public void SetChildProperty(string name, IPanelControl childControl)
        {
            if (m_childProperties != null && m_childProperties.Length > 0)
            {
                var prop = m_childProperties.FirstOrDefault(p => String.Equals(p.Item1, name, StringComparison.CurrentCultureIgnoreCase));
                if (prop != null)
                {
                    prop.Item2.SetValue(this, childControl);
                }
            }
        }

        public IEnumerable<Tuple<string, Type>> GetValueProperties()
        {
            foreach (var property in m_valuedProperties)
            {
                yield return new Tuple<string, Type>(property.Item1, property.Item2.PropertyType);
            }
        }

        #endregion

        #region IPanelElement Interface

        bool IPanelElement.SetValue(ICallContext context, object value)
        {
            throw new NotImplementedException();
        }

        object IPanelElement.GetValue(ICallContext context)
        {
            throw new NotImplementedException();
        }

        void IPanelElement.SetProperty(ICallContext context, string property, object value)
        {
            try
            {
                this.GetType().GetProperty(property).SetValue(this, value);
            }
            catch
            {
                if (context != null)
                {
                    context.ReportError($"Error setting property '{property}' of panel element.");
                }
            }
        }

        object IPanelElement.GetProperty(ICallContext context, string property)
        {
            return this.GetType().GetProperty(property).GetValue(this, null);
        }

        IPanelElement IPanelElement.TryFindChildElement(ICallContext context, string name)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IPanelElement> IPanelElement.GetChilds()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
