using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
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

        public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            var color = new PropertyBlockDecoder.ValueString<object>("Color");
            var text = new PropertyBlockDecoder.ValueString<object>("Text");
            var maxWidth = new PropertyBlockDecoder.ValueInt<object>("MaxWidth");
            var widthGroup = new PropertyBlockDecoder.ValueInt<object>("WidthGroup");
            var instance = new PropertyBlockDecoder.ValueString<object>("Instance", "Object");
            var fileElement = new PropertyBlockDecoder.ValueString<object>("Procedure", "Element");

            var button = new PropertyBlockDecoder.Block<object, object>("Button",
                color, text, instance, fileElement, maxWidth, widthGroup,
                new PropertyBlockDecoder.ValueString<object>("Partner"),
                new PropertyBlockDecoder.Value<object>("Arg", "Argument"),
                new PropertyBlockDecoder.Array<object>("Args", "Arguments"),
                new PropertyBlockDecoder.Flag<object>("Stoppable"),
                new PropertyBlockDecoder.Flag<object>("StopOnButtonRelease"),
                new PropertyBlockDecoder.ValueString<object>("Command"),
                new PropertyBlockDecoder.Flag<object>("CheckOnClick"),
                new PropertyBlockDecoder.Flag<object>("CheckArg"),
                new PropertyBlockDecoder.ValueString<object>("CheckedText"),
                new PropertyBlockDecoder.ValueString<object>("UncheckedText"),
                new PropertyBlockDecoder.ValueString<object>("EnabledSource"),
                new PropertyBlockDecoder.ValueString<object>("DisabledSource")
                );

            var textbox = new PropertyBlockDecoder.Block<object, object>("TextBox",
                color, text, instance, maxWidth, widthGroup,
                new PropertyBlockDecoder.Flag<object>("ReadOnly"),
                new PropertyBlockDecoder.Flag<object>("RightAligned"),
                new PropertyBlockDecoder.ValueString<object>("Property"),
                new PropertyBlockDecoder.ValueString<object>("ProcedureOutput"),
                new PropertyBlockDecoder.ValueString<object>("EnabledSource"),
                new PropertyBlockDecoder.ValueString<object>("DisabledSource")
                );

            var menu = new PropertyBlockDecoder.Block<object, object>("Menu");
            menu.SetChilds(menu, button, instance);

            var root = new PropertyBlockDecoder.Block<object, object>
                (
                    new PropertyBlockDecoder.ValueString<object>("Label"),
                    color,
                    new PropertyBlockDecoder.ValueInt<object>("Index"),
                    new PropertyBlockDecoder.Flag<object>("Separator"),
                    new PropertyBlockDecoder.Flag<object>("ColumnSeparator"),
                    menu, button, textbox, instance
                );
            root.DecodeData(data, null, errors);
        }

        public void Setup(IScriptFile file, ILogger logger, PropertyBlock data)
        {
            m_definition = data;
        }

        public void SetToolBarReference(IToolBarElement panel)
        {
            m_toolbarElement = panel;
        }

        public PropertyBlock Definition { get { return m_definition; } }
        public IToolBarElement UI { get { return m_toolbarElement; } }


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
            if (m_toolbarElement != null)
            {
                string field;
                var element = TryGetElement(m_toolbarElement, name, false, out field);
                if (element != null)
                {
                    if (field != null)
                    {
                        return element.GetProperty(context, field);
                    }
                    else
                    {
                        return element;
                    }
                }
                else
                {
                    // Then try the toolbar itself.
                    return m_toolbarElement.GetProperty(context, name);
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
            if (m_toolbarElement != null)
            {
                string field;
                var element = TryGetElement(m_toolbarElement, name, false, out field);
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
                    // Then try the toolbar itself.
                    m_toolbarElement.SetProperty(context, name, value);
                }
            }
            // else, just ignore.
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