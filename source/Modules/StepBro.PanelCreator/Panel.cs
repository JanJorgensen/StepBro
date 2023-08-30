using Newtonsoft.Json.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.PanelCreator.DummyUI;
using System;
using System.Threading;
using System.Xml.Linq;

namespace StepBro.PanelCreator
{
    public class Panel : StepBro.Core.Api.DynamicStepBroObject, INameable, ISettableFromPropertyBlock
    {
        public const string MainElementName = "panel";

        private string m_name = "PanelDefinition";
        private PropertyBlock m_mainPanelDefinition = null;
        private IPanelElement m_mainPanelElement = null;

        public Panel() { }

        public string Name { get { return m_name; } set { m_name = value; } }

        public void Setup(ILogger logger, PropertyBlock data)
        {
            var mainPanel = data.TryGetElement("Main");
            if (mainPanel != null)
            {
                if (mainPanel.BlockEntryType != PropertyBlockEntryType.Block || string.IsNullOrEmpty(mainPanel.SpecifiedTypeName))
                {
                    logger.LogError($"The '{MainElementName}' must be defines as a 'block'.");
                    return;
                }
                if (string.IsNullOrEmpty(mainPanel.SpecifiedTypeName))
                {
                    logger.LogError($"The '{MainElementName}' must have a type specified.");
                    return;
                }
                m_mainPanelDefinition = mainPanel as PropertyBlock;

                var panel = new DummyBaseUIElement(mainPanel.Name, mainPanel.Name, mainPanel.SpecifiedTypeName);
                m_mainPanelElement = panel;
                panel.Setup(m_mainPanelDefinition);
            }
            else
            {
                logger.LogError($"The '{MainElementName}' definition is missing.");
            }
        }

        public void SetPanelReference(IPanelElement panel)
        {
            m_mainPanelElement = panel;
        }

        public PropertyBlock MainPanelDefinition { get { return m_mainPanelDefinition; } }
        public IPanelElement MainPanelElement { get { return m_mainPanelElement; } }


        private static IPanelElement TryGetElement(IPanelElement element, string name, bool isRooted, out string field)
        {
            var nameparts = name.Split('.');
            var first = nameparts[0];
            foreach (var child in element.GetChilds())
            {
                if (String.Equals(child.ElementName, first, StringComparison.InvariantCulture) || String.Equals(child.PropertyName, first, StringComparison.InvariantCulture))
                {
                    if (nameparts.Length > 1)
                    {
                        var rest = name.Substring(first.Length + 1);
                        string subfield;
                        var found = TryGetElement(child, rest, true /* The root of the name is now found */, out subfield);
                        if (found != null)
                        {
                            field = subfield;
                            return found;
                        }
                        else
                        {
                            field = rest;
                            return child;
                        }
                    }
                    else
                    {
                        field = null;   // Nothing left in the element name.
                        return child;
                    }
                }
            }
            // The element was not found; now try to find the element in the childs.
            if (!isRooted)
            {
                foreach (var child in element.GetChilds())
                {
                    var found = TryGetElement(child, name, false, out field);
                    if (found != null) return found;
                }
            }
            field = "";
            return null;
        }

        #region Script Access

        public override DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            return base.HasProperty(name, out type, out isReadOnly);
        }

        public override object GetProperty([Implicit] ICallContext context, string name)
        {
            if (m_mainPanelElement != null)
            {
                string field;
                var element = TryGetElement(m_mainPanelElement, name, false, out field);
                if (element != null)
                {
                    if (field != null)
                    {
                        return element.GetProperty(context, field);
                    }
                    else
                    {
                        context.ReportError($"No property has been specified in the property-path '{name}'.");
                    }
                }
                else
                {
                    context.ReportError($"The specified GUI element for '{name}' was not found.");
                }
            }
            else
            {
                context.ReportError("No GUI panel has been created or registered.");
            }
            return null;
        }

        public override void SetProperty([Implicit] ICallContext context, string name, object value)
        {
            if (m_mainPanelElement != null)
            {
                string field;
                var element = TryGetElement(m_mainPanelElement, name, false, out field);
                if (element != null)
                {
                    if (field != null)
                    {
                        element.SetProperty(context, field, value);
                    }
                    else
                    {
                        context.ReportError($"No property has been specified in the property-path '{name}'.");
                    }
                }
                else
                {
                    context.ReportError($"The specified GUI element for '{name}' was not found.");
                }
            }
            else
            {
                context.ReportError("No GUI panel has been created or registered.");
            }
        }

        public void DumpPanelElements([Implicit] ICallContext context)
        {
            if (m_mainPanelElement != null)
            {
                this.DumpElement(context, m_mainPanelElement);
            }
            else
            {
                context.ReportError("No GUI panel has been created or registered.");
            }
        }

        private void DumpElement(ICallContext context, IPanelElement element, string root = "")
        {
            string pathString = (String.IsNullOrEmpty(root) ? String.Empty : root + ".") + element.ElementName;
            context.Logger.LogDetail(pathString);
            foreach (var child in element.GetChilds())
            {
                this.DumpElement(context, child, pathString);
            }
        }

        #endregion
    }
}