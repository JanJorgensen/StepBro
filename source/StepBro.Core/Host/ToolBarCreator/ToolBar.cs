using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Drawing;
using static StepBro.Core.Data.PropertyBlockDecoder;
using static StepBro.Core.Parser.StepBroTypeScanListener;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.ToolBarCreator
{
    [Public]
    public class ToolBar : StepBro.Core.Api.DynamicStepBroObject, INameable, ISettableFromPropertyBlock
    {
        private string m_name = null;
        private string m_title = null;
        private PropertyBlock m_definition = null;
        private IToolBarElement m_toolbarElement = null;

        internal class DefaultScanner : IPropertyBlockDataScanner
        {
            private static Block<object, object> m_decoder;

            static DefaultScanner()
            {
                m_decoder = CreateDecoder();
            }

            public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
            {
                ToolBar.DefaultPreScanData(file, data, errors);
            }
            public PropertyBlockDecoder.Element TryGetDecoder()
            {
                return m_decoder;
            }

            public static readonly ValueString<object> FieldColor = new ValueString<object>("Color", Doc(""));
            public static readonly ValueInt<object> FieldIndex = new ValueInt<object>("Index", Doc(""));
            public static readonly ValueString<object> FieldText = new ValueString<object>("Text", Doc(""));
            public static readonly ValueString<object> FieldInstance = new ValueString<object>("Instance", "Object", Doc(""));
            public static readonly ValueString<object> FieldFileElement = new ValueString<object>("Procedure", "Element", Doc(""));
            public static readonly ValueInt<object> FieldMaxWidth = new ValueInt<object>("MaxWidth", Doc(""));
            public static readonly ValueString<object> FieldWidthGroup = new ValueString<object>("WidthGroup", Doc(""));
            public static readonly Flag<object> FieldSeparatorFlag = new Flag<object>("Separator", Usage.Element, Doc(""));
            public static readonly Block<object, object> FieldSeparatorBlock = new Block<object, object>("Separator", Doc(""), FieldIndex);

            private static Block<object, object> s_button = null;
            public static Block<object, object> GetButton()
            {
                if (s_button == null)
                {
                    s_button = new Block<object, object>("Button", Doc(""),
                        FieldColor, FieldIndex, FieldText, FieldInstance, FieldFileElement, FieldMaxWidth, FieldWidthGroup,
                        new ValueString<object>("Partner", Doc("")),
                        new Value<object>("Arg", "Argument", Usage.Setting, Doc("")),
                        new Array<object>("Args", "Arguments", Usage.Setting, Doc("")),
                        new Flag<object>("Stoppable", Doc("")),
                        new Flag<object>("StopOnButtonRelease", Doc("")),
                        new ValueString<object>("Command", Doc("")),
                        new Flag<object>("CheckOnClick", Doc("")),
                        new Flag<object>("CheckArg", Doc("")),
                        new ValueString<object>("CheckedText", Doc("")),
                        new ValueString<object>("UncheckedText", Doc("")),
                        new ValueString<object>("EnabledSource", Doc("")),
                        new ValueString<object>("DisabledSource", Doc(""))
                        );
                }
                return s_button;
            }
            private static Block<object, object> s_textbox = null;
            public static Block<object, object> GetTextBox()
            {
                if (s_textbox == null)
                {
                    s_textbox = new Block<object, object>("TextBox", Doc(""),
                        FieldColor, FieldIndex, FieldText, FieldInstance, FieldMaxWidth, FieldWidthGroup,
                        new Flag<object>("ReadOnly", Doc("")),
                        new Flag<object>("RightAligned", Doc("")),
                        new ValueString<object>("Property", Doc("")),
                        new ValueString<object>("ProcedureOutput", Doc("")),
                        new ValueString<object>("EnabledSource", Doc("")),
                        new ValueString<object>("DisabledSource", Doc(""))
                        );
                }
                return s_textbox;
            }

            private static Block<object, object> s_menu = null;
            public static Block<object, object> GetMenu()
            {
                if (s_menu == null)
                {
                    s_menu = new Block<object, object>("Menu", Doc(""));
                    s_menu.SetChilds(FieldIndex, FieldInstance, s_menu, GetButton());
                }
                return s_menu;
            }


            private static Block<object, object> CreateDecoder()
            {
                return new Block<object, object>
                    (
                        nameof(ToolBar),
                        Doc(""),
                        new ValueString<object>("Label", Doc("")),
                        FieldColor,
                        FieldIndex,
                        FieldSeparatorFlag,
                        FieldSeparatorBlock,
                        new Flag<object>("ColumnSeparator", Usage.Element, Doc("")),
                        FieldInstance,
                        GetMenu(), GetButton(), GetTextBox()
                    );
            }
        }

        private static IPropertyBlockDataScanner g_scanner = new DefaultScanner();

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

        public static IPropertyBlockDataScanner PreScanner { get { return g_scanner; } set { g_scanner = value; } }

        public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            g_scanner.PreScanData(file, data, errors);
        }

        public PropertyBlockDecoder.Element TryGetDecoder()
        {
            return g_scanner.TryGetDecoder();
        }

        private static void DefaultPreScanData(object context, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            ((Block<object, object>)g_scanner.TryGetDecoder()).DecodeData(context, data, null, errors);
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

        public void DumpElements([Implicit] ICallContext context)
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