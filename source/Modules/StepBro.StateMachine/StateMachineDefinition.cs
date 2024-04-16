using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.StateMachine
{
    public class StateMachineDefinition : INameable, INamedObject, ISettableFromPropertyBlock
    {
        private string m_name = "StateMachine";

        private class MachineType
        {
            private string m_typeName;
            private List<NamedData<Type>> m_variables = new List<NamedData<Type>>();

            public MachineType(string name) { m_typeName = name; }

            public void AddVariable(string name, Type type)
            {
                m_variables.Add(new NamedData<Type>(name, type));
            }

            public IEnumerable<NamedData<Type>> ListVariables()
            {
                foreach(var v in m_variables) yield return v;
            }
        }

        private PropertyBlockDecoder.Block<object, StateMachineDefinition> m_decoder = null;
        private List<MachineType> m_types = new List<MachineType>();

        public StateMachineDefinition() { }

        public string Name { get { return m_name; } set { m_name = value; } }

        public string ShortName { get { return this.Name; } }

        public string FullName { get { return this.Name; } }

        public void Create([Implicit] ICallContext context, Identifier type, Identifier name, ArgumentList arguments)
        {

        }

        #region Parsing and Setup

        public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            var type = new PropertyBlockDecoder.Block<object, object>(
                new PropertyBlockDecoder.Array<object>("states"),
                new PropertyBlockDecoder.Value<object>()
                );

            //var instance = new PropertyBlockDecoder.Block<object, object>(
            //    new PropertyBlockDecoder.ValueString<object>("state"),
            //    new PropertyBlockDecoder.Value<object>()
            //    );

            var root = new PropertyBlockDecoder.Block<object, object>(type /*, instance*/);
            root.DecodeData(data, null, errors);
        }

        public void Setup(IScriptFile file, ILogger logger, PropertyBlock data)
        {
            //m_decoder = new Block<object, StateMachineDefinition>
            //    (
            //        new ValueString<ToolBar>("Label", (t, v) =>
            //        {
            //            var text = v.ValueAsString();
            //            Label label;
            //            if (v.HasTypeSpecified)
            //            {
            //                label = new Label(v.Name);
            //            }
            //            else
            //            {
            //                label = new Label("label" + text.Replace(" ", "_").Replace(".", "Dot"));
            //            }
            //            label.Text = text;
            //            t.Items.Add(label);
            //            return null;    // No errors
            //        }),
            //        new ValueColor<ToolBar>("Color", (t, c) => { t.BackColor = c; return null; }),
            //        new ValueInt<ToolBar>("Priority", (t, v) => { t.Priority = Convert.ToInt32(v.Value); return null; }),
            //        new Flag<ToolBar>("Separator", (t, f) =>
            //        {
            //            var separator = new Separator("Separator");
            //            t.Items.Add(separator);
            //            return null;
            //        }),
            //        new Flag<ToolBar>("ColumnSeparator", (t, f) =>
            //        {
            //            string name = f.Name;
            //            if (!f.HasTypeSpecified)
            //            {
            //                int index = t.Items.Cast<ToolStripItem>().Count(i => i is ColumnSeparator);
            //                name = "column" + index;
            //            }
            //            var separator = new ColumnSeparator(name);
            //            t.Items.Add(separator); t.Items.Add(separator);
            //            return null;
            //        }),
            //        new ValueString<ToolBar>("Instance", (t, v) => { t.SetChildProperty("Instance", v.ValueAsString()); return null; }),
            //        toolbarMenu, procButton, objCmdButton
            //    );




            //var procButtonElements = new Element[] {
            //    new ValueColor<ProcedureActivationButton>("Color", (b, c) => { b.BackColor = c; return null; }),
            //    new ValueString<ProcedureActivationButton>("Text", (b, v) => { b.Text = v.ValueAsString(); return null; }),
            //    new ValueString<ProcedureActivationButton>("Instance", "Object", (b, v) => { b.Instance = v.ValueAsString(); return null; }),
            //    new ValueString<ProcedureActivationButton>("Procedure", "Element", (b, v) => { b.Procedure = v.ValueAsString(); return null; }),
            //    new ValueString<ProcedureActivationButton>("Partner", "Model", (b, v) => { b.Partner = v.ValueAsString(); return null; }),
            //    new Value<ProcedureActivationButton>("Arg", "Argument", (b, a) => { b.AddToArguments(a.Value); return null; }),
            //    new PropertyBlockDecoder.Array<ProcedureActivationButton>("Args", "Arguments", (b, a) => { b.AddToArguments(a); return null; }),
            //    new Flag <ProcedureActivationButton>("Stoppable", (b, f) => { b.SetStoppable(); return null; }),
            //    new Flag<ProcedureActivationButton>("StopOnButtonRelease", (b, f) => { b.SetStopOnButtonRelease(); return null; })
            //};
            //var procButton = new Block<IMenuItemHost, ProcedureActivationButton>(
            //    "ProcedureActivationButton",
            //    (m, n) =>
            //    {
            //        var button = new ProcedureActivationButton(m as IToolBarElement, m_coreAccess, n);
            //        m.Add(button);
            //        button.Size = new Size(23, 20);
            //        button.AutoSize = true;
            //        return button;
            //    },
            //    procButtonElements);

            //var objCmdButtonElements = new Element[] {
            //    new ValueColor<ObjectCommandButton>("Color", (b, c) => { b.BackColor = c; return null; }),
            //    new ValueString<ObjectCommandButton>("Text", (b, v) => { b.Text = v.ValueAsString(); return null; }),
            //    new ValueString<ObjectCommandButton>("Instance", (b, v) => { b.ObjectInstance = v.ValueAsString(); return null; }),
            //    new ValueString<ObjectCommandButton>("Command", (b, v) => { b.ObjectCommand = v.ValueAsString(); return null; })
            //};
            //var objCmdButton = new Block<IMenuItemHost, ObjectCommandButton>(
            //    "ObjectCommandButton",
            //    (m, n) =>
            //    {
            //        var button = new ObjectCommandButton(m as IToolBarElement, m_coreAccess, n);
            //        m.Add(button);
            //        button.Size = new Size(23, 20);
            //        button.AutoSize = true;
            //        return button;
            //    },
            //    objCmdButtonElements);

            //var menuTitle = new ValueString<IMenu>("Text", "Title", (m, v) => { m.SetTitle(v.ValueAsString()); return null; });

            //var subMenu = new Block<ToolStripDropDownMenu, ToolStripMenuSubMenu>("Menu", "SubMenu",
            //    (m, n) =>
            //    {
            //        var menu = new ToolStripMenuSubMenu(m, m_coreAccess, n);
            //        m.DropDownItems.Add(menu);
            //        menu.Size = new Size(30, 20);
            //        menu.AutoSize = true;
            //        return menu;
            //    });
            //subMenu.SetChilds(
            //    menuTitle, subMenu, procButton, objCmdButton,
            //    new ValueString<ToolStripMenuSubMenu>("Instance", (m, v) => { m.SetChildProperty("Instance", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripMenuSubMenu>("Procedure", "Element", (m, v) => { m.SetChildProperty("Element", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripMenuSubMenu>("Partner", "Model", (m, v) => { m.SetChildProperty("Partner", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripMenuSubMenu>("Command", (m, v) => { m.SetChildProperty("Command", v.ValueAsString()); return null; }));

            //var toolbarMenu = new Block<ToolBar, ToolStripDropDownMenu>("Menu", "DropDownMenu",
            //    (t, n) =>
            //    {
            //        var menu = new ToolStripDropDownMenu(t, m_coreAccess, n);
            //        t.Items.Add(menu);
            //        menu.Size = new Size(30, 20);
            //        menu.AutoSize = true;
            //        return menu;
            //    });
            //toolbarMenu.SetChilds(
            //    menuTitle, subMenu, procButton, objCmdButton,
            //    new ValueString<ToolStripDropDownMenu>("Instance", (m, v) => { m.SetChildProperty("Instance", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripDropDownMenu>("Procedure", "Element", (m, v) => { m.SetChildProperty("Element", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripDropDownMenu>("Partner", "Model", (m, v) => { m.SetChildProperty("Partner", v.ValueAsString()); return null; }),
            //    new ValueString<ToolStripDropDownMenu>("Command", (m, v) => { m.SetChildProperty("Command", v.ValueAsString()); return null; }));

            //m_decoder = new Block<object, ToolBar>
            //    (
            //        new ValueString<ToolBar>("Label", (t, v) =>
            //        {
            //            var text = v.ValueAsString();
            //            Label label;
            //            if (v.HasTypeSpecified)
            //            {
            //                label = new Label(v.Name);
            //            }
            //            else
            //            {
            //                label = new Label("label" + text.Replace(" ", "_").Replace(".", "Dot"));
            //            }
            //            label.Text = text;
            //            t.Items.Add(label);
            //            return null;    // No errors
            //        }),
            //        new ValueColor<ToolBar>("Color", (t, c) => { t.BackColor = c; return null; }),
            //        new ValueInt<ToolBar>("Priority", (t, v) => { t.Priority = Convert.ToInt32(v.Value); return null; }),
            //        new Flag<ToolBar>("Separator", (t, f) =>
            //        {
            //            var separator = new Separator("Separator");
            //            t.Items.Add(separator);
            //            return null;
            //        }),
            //        new Flag<ToolBar>("ColumnSeparator", (t, f) =>
            //        {
            //            string name = f.Name;
            //            if (!f.HasTypeSpecified)
            //            {
            //                int index = t.Items.Cast<ToolStripItem>().Count(i => i is ColumnSeparator);
            //                name = "column" + index;
            //            }
            //            var separator = new ColumnSeparator(name);
            //            t.Items.Add(separator); t.Items.Add(separator);
            //            return null;
            //        }),
            //        new ValueString<ToolBar>("Instance", (t, v) => { t.SetChildProperty("Instance", v.ValueAsString()); return null; }),
            //        toolbarMenu, procButton, objCmdButton
            //    );

        }

        #endregion
    }
}
