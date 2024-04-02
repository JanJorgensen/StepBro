using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.PanelCreator.DummyUI;
using System;
using System.Collections.Generic;

namespace StepBro.ToolBarCreator
{
    [Public]
    public class ToolBar : StepBro.Core.Api.DynamicStepBroObject, INameable, ISettableFromPropertyBlock
    {
        private string m_name = null;
        private string m_title = null;
        private PropertyBlock m_definition = null;
        private IToolBarElement m_toolbarElement = null;

        public ToolBar() { }

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
            }
        }
        public string Title
        {
            get { return m_title; }
            set
            {
                m_title = value;
            }
        }

        public static string[] ToolbarElementTypes()
        {
            return new string[] { "ProcedureActivationButton", "ObjectCommandButton", "Menu", "ColumnSeparator", "Label" };
        }

        public static string[] ToolbarPropertiesAndFlags()
        {
            return new string[] { "Color" };
        }

        public void PreScanData(PropertyBlock data, List<Tuple<int, string>> errors)
        {
            var color = new PropertyBlockDecoder.ValueString<object>("Color");
            var text = new PropertyBlockDecoder.ValueString<object>("Text");
            var instance = new PropertyBlockDecoder.ValueString<object>("Instance");
            var procedure = new PropertyBlockDecoder.ValueString<object>("Procedure", "Element");

            var procButton = new PropertyBlockDecoder.Block<object, object>("ProcedureActivationButton",
                color, text, instance,
                new PropertyBlockDecoder.ValueString<object>("Procedure", "Element"),
                new PropertyBlockDecoder.ValueString<object>("Partner"),
                new PropertyBlockDecoder.Value<object>("Arg", "Argument"),
                new PropertyBlockDecoder.Array<object>("Args", "Arguments"),
                new PropertyBlockDecoder.Flag<object>("Stoppable"),
                new PropertyBlockDecoder.Flag<object>("StopOnButtonRelease")
                );
            var objCmdButton = new PropertyBlockDecoder.Block<object, object>("ObjectCommandButton",
                color, text, instance,
                new PropertyBlockDecoder.ValueString<object>("Command")
                );

            var menu = new PropertyBlockDecoder.Block<object, object>("Menu");
            menu.SetChilds(menu, procButton, objCmdButton, instance);

            var root = new PropertyBlockDecoder.Block<object, object>
                (
                    new PropertyBlockDecoder.ValueString<object>("Label"),
                    color,
                    new PropertyBlockDecoder.ValueInt<object>("Priority"),
                    new PropertyBlockDecoder.Flag<object>("Separator"),
                    new PropertyBlockDecoder.Flag<object>("ColumnSeparator"),
                    menu, procButton, objCmdButton, instance
                );
            root.DecodeData(data, null, errors);
        }

        public void Setup(ILogger logger, PropertyBlock data)
        {
            m_definition = data;
        }

        public void SetToolBarReference(IToolBarElement panel)
        {
            //m_mainPanelElement = panel;
        }

        public PropertyBlock Definition { get { return m_definition; } }
        public IToolBarElement MainPanelElement { get { return m_toolbarElement; } }


        private static IToolBarElement TryGetElement(IToolBarElement element, string name, bool isRooted, out string field)
        {
            var nameparts = name.Split('.');
            var first = nameparts[0];
            foreach (var child in element.GetChilds())
            {
                if (String.Equals(child.ElementName, first, StringComparison.InvariantCulture))
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
            //if (m_mainPanelElement != null)
            //{
            //    string field;
            //    var element = TryGetElement(m_mainPanelElement, name, false, out field);
            //    if (element != null)
            //    {
            //        if (field != null)
            //        {
            //            return element.GetProperty(context, field);
            //        }
            //        else
            //        {
            //            context.ReportError($"No property has been specified in the property-path '{name}'.");
            //        }
            //    }
            //    else
            //    {
            //        context.ReportError($"The specified GUI element for '{name}' was not found.");
            //    }
            //}
            //else
            //{
            //    context.ReportError("No GUI panel has been created or registered.");
            //}
            return null;
        }

        public override void SetProperty([Implicit] ICallContext context, string name, object value)
        {
            //if (m_mainPanelElement != null)
            //{
            //    string field;
            //    var element = TryGetElement(m_mainPanelElement, name, false, out field);
            //    if (element != null)
            //    {
            //        if (field != null)
            //        {
            //            element.SetProperty(context, field, value);
            //        }
            //        else
            //        {
            //            context.ReportError($"No property has been specified in the property-path '{name}'.");
            //        }
            //    }
            //    else
            //    {
            //        context.ReportError($"The specified GUI element for '{name}' was not found.");
            //    }
            //}
            //else
            //{
            //    context.ReportError("No GUI panel has been created or registered.");
            //}
        }

        public void DumpPanelElements([Implicit] ICallContext context)
        {
            if (m_toolbarElement != null)
            {
                this.DumpElement(context, m_toolbarElement);
            }
            else
            {
                context.ReportError("No GUI ToolBar has been created or registered.");
            }
        }

        private void DumpElement(ICallContext context, IToolBarElement element, string root = "")
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