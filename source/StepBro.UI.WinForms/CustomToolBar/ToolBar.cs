using CommandLine;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.ToolBarCreator;
using System.ComponentModel;
using static StepBro.Core.Data.PropertyBlockDecoder;
using static StepBro.UI.WinForms.WinFormsPropertyBlockDecoder;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ToolBar : ToolStrip, IMenuItemHost, IToolBarElement
    {
        private static ICoreAccess m_coreAccess = null;
        bool m_colorSet = false;
        bool m_settingDefaultColor = false;
        int m_index = 1000;
        private static Scanner m_decoder = null;
        private Dictionary<string, object> m_commonChildFields = null;
        private static bool m_adjustingSizes = false;
        private static bool g_scannerIsSetup = false;


        public ToolBar() : base()
        {
            this.GripStyle = ToolStripGripStyle.Hidden;
            this.AutoSize = false;
            this.Height = 26;
        }

        public static void ToolBarSetup()
        {
            if (!g_scannerIsSetup)
            {
                m_decoder = new Scanner();
                StepBro.ToolBarCreator.ToolBar.PreScanner = m_decoder;
                g_scannerIsSetup = true;
            }
        }

        public ToolBar(ICoreAccess coreAccess) : this()
        {
            m_coreAccess = coreAccess;
        }

        public void SetCoreAccess(ICoreAccess coreAccess)
        {
            m_coreAccess = coreAccess;
        }

        public void Setup(ILogger logger, string name, PropertyBlock definition)
        {
            this.Text = name.Split(".").Last();
            this.Name = name;
            this.Tag = name;
            this.Setup(logger, definition);
        }

        private class Scanner : IPropertyBlockDataScanner
        {
            public void PreScanData(PropertyBlock data, List<Tuple<int, string>> errors)
            {
                var toolbarToDispose = new ToolBar();
                ((Block<object, ToolBar>)m_decoder.TryGetDecoder()).DecodeData(data, toolbarToDispose, errors);
            }

            private const string MenuInstanceHelp = "The default script object to be used by child menu elements.";

            public Element TryGetDecoder()
            {
                var buttonElements = new Element[] {
                    new ValueColor<Button>("Color", Doc("The button color."), (b, c) => { b.BackColor = c; return null; }),
                    new ValueString<Button>("Text", Doc("The text shown on the button."), (b, v) => { b.Text = v.ValueAsString(); return null; }),
                    new ValueInt<Button>("MaxWidth", Doc("The maximum width of the button."), (b, v) => { b.MaxWidth = (int)(long)v.Value; b.AutoSize = false; return null; }),
                    new ValueString<Button>("WidthGroup", Doc("The name of the group that synchronizes their button width."), (b, v) => { b.WidthGroup = v.ValueAsString(); return null; }),
                    new ValueString<Button>("Instance", "Object", Doc("The script object associated with the button."), (b, v) => { b.Instance = v.ValueAsString(); return null; }),
                    new ValueString<Button>("Procedure", "Element", Doc("The procedure or other script element to execute when activating the button."), (b, v) => { b.Procedure = v.ValueAsString(); return null; }),
                    new ValueString<Button>("Partner", "Model", Doc("The partner/model to use for executing the target procedure or script element."), (b, v) => { b.Partner = v.ValueAsString(); return null; }),
                    new Value<Button>("Arg", "Argument", Usage.Setting, Doc("An argument for the called procedure."), (b, a) => { b.AddToArguments(a.Value); return null; }),
                    new Array<Button>("Args", "Arguments", Usage.Setting, Doc("A list of arguments for the called procedure."), (b, a) => { b.AddToArguments(a); return null; }),
                    new Flag<Button>("Stoppable", Doc("Makes the execution stoppable by activating the button again."), (b, f) => { b.SetStoppable(); return null; }),
                    new Flag<Button>("StopOnButtonRelease", Doc("Makes the execution stop when the button is released."), (b, f) => { b.SetStopOnButtonRelease(); return null; }),
                    new ValueString<Button>("Command", Doc("Object command to execute when activating the button."), (b, v) => { b.ObjectCommand = v.ValueAsString(); return null; }),
                    new Flag<Button>("CheckOnClick", Doc("Makes the button toggle between checked and unchecked when activated."), (b, f) => { b.SetCheckOnClick(); return null; }),
                    new Flag<Button>("CheckArg", Doc("The 'Instance' object "), (b, f) => { /* TODO */ return null; }),
                    new ValueString<Button>("CheckedText", Doc("Blah blah blah"), (b, v) => { /* TODO */ return null; }),
                    new ValueString<Button>("UncheckedText", Doc("Blah blah blah"), (b, v) => { /* TODO */ return null; }),
                    new ValueString<Button>("EnabledSource", Doc("Blah blah blah"), (b, v) => { /* TODO */ return null; }),
                    new ValueString<Button>("DisabledSource", Doc("Blah blah blah"), (b, v) => { /* TODO */ return null; })
                };
                var button = new Block<IMenuItemHost, Button>(
                    "Button",
                    Doc("A push button."),
                    (m, n) =>
                    {
                        var button = new Button(m as IToolBarElement, m_coreAccess, n);
                        m.Add(button);
                        button.Size = new Size(23, 20);
                        button.AutoSize = true;
                        return button;
                    },
                    buttonElements);

                var textboxElements = new Element[] {
                    new ValueColor<TextBox>("Color", Doc("Blah blah blah"), (t, c) => { t.BackColor = c; return null; }),
                    new ValueString<TextBox>("Text", Doc("Blah blah blah"), (t, v) => { t.Text = v.ValueAsString(); return null; }),
                    new ValueInt<TextBox>("MaxWidth", Doc("Blah blah blah"), (t, v) => { t.MaxWidth = (int)(long)v.Value; t.AutoSize = false; return null; }),
                    new ValueString<TextBox>("WidthGroup", Doc("Blah blah blah"), (t, v) => { t.WidthGroup = v.ValueAsString(); return null; }),
                    new Flag<TextBox>("ReadOnly", Doc("Blah blah blah"), (t, f) => { t.ReadOnly = true; return null; }),
                    new Flag<TextBox>("RightAligned", Doc("Blah blah blah"), (t, f) => { t.TextBoxTextAlign = HorizontalAlignment.Right; return null; }),
                    new ValueString<Button>("Instance", "Object", Doc("The script object associated with the textbox."), (t, v) => { t.Instance = v.ValueAsString(); return null; }),
                    new ValueString<TextBox>("Property", Doc("Blah blah blah"), (t, v) => { /* TODO */ return null; }),
                    new ValueString<TextBox>("ProcedureOutput", Doc("Blah blah blah"), (t, v) => { /* TODO */ return null; }),
                    new ValueString<TextBox>("EnabledSource", Doc("Blah blah blah"), (t, v) => { /* TODO */ return null; }),
                    new ValueString<TextBox>("DisabledSource", Doc("Blah blah blah"), (t, v) => { /* TODO */ return null; })
                };

                var textbox = new Block<IMenuItemHost, CustomToolBar.TextBox>(
                    "TextBox",
                    Doc("A text box that is either read-only (just an indicator), or where the user can input some text."),
                    (m, n) =>
                    {
                        var tb = new CustomToolBar.TextBox(m as IToolBarElement, m_coreAccess, n);
                        m.Add(tb);
                        tb.Size = new Size(60, 22);
                        tb.AutoSize = true;
                        return tb;
                    },
                    textboxElements);

                var menuTitle = new ValueString<IMenu>("Text", "Title", Doc(""), (m, v) => { m.SetTitle(v.ValueAsString()); return null; });

                var menuSeparator = new Flag<IMenuItemHost>("Separator", Usage.Element, Doc("A separator line between the previous and the succeeding menu item."), (m, f) =>
                {
                    var separator = new Separator("Separator");
                    m.Add(separator);
                    return null;
                });

                var subMenu = new Block<ToolStripDropDownMenu, ToolStripMenuSubMenu>(
                    "SubMenu", "Menu",
                    Doc("A sub-menu for a drop-down menu or another sub-menu."),
                    (m, n) =>
                    {
                        var menu = new ToolStripMenuSubMenu(m, m_coreAccess, n);
                        m.DropDownItems.Add(menu);
                        menu.Size = new Size(30, 20);
                        menu.AutoSize = true;
                        return menu;
                    });
                subMenu.SetChilds(
                    menuTitle, subMenu, button, menuSeparator,
                    new ValueString<ToolStripMenuSubMenu>("Instance", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Instance", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripMenuSubMenu>("Procedure", "Element", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Element", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripMenuSubMenu>("Partner", "Model", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Partner", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripMenuSubMenu>("Command", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Command", v.ValueAsString()); return null; }));

                var toolbarMenu = new Block<ToolBar, ToolStripDropDownMenu>(
                    "Menu", "DropDownMenu",
                    Doc("A drop-down menu that can contain different kinds of entries."),
                    (t, n) =>
                    {
                        var menu = new ToolStripDropDownMenu(t, m_coreAccess, n);
                        t.Items.Add(menu);
                        menu.Size = new Size(30, 20);
                        menu.AutoSize = true;
                        return menu;
                    });
                toolbarMenu.SetChilds(
                    menuTitle, subMenu, button, textbox, menuSeparator,
                    new ValueString<ToolStripDropDownMenu>("Instance", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Instance", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripDropDownMenu>("Procedure", "Element", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Element", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripDropDownMenu>("Partner", "Model", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Partner", v.ValueAsString()); return null; }),
                    new ValueString<ToolStripDropDownMenu>("Command", Doc("Blah blah blah"), (m, v) => { m.SetChildProperty("Command", v.ValueAsString()); return null; }));

                return new Block<object, ToolBar>
                    (
                        nameof(ToolBar),
                        Doc("Defined properties and GUI element types for the created toolbar."),
                        new ValueString<ToolBar>("Label", Usage.Element, Doc("A simple text field."), (t, v) =>
                        {
                            var text = v.ValueAsString();
                            Label label;
                            if (v.HasTypeSpecified)
                            {
                                label = new Label(v.Name);
                            }
                            else
                            {
                                label = new Label("label" + text.Replace(" ", "_").Replace(".", "Dot"));
                            }
                            label.Text = text;
                            t.Items.Add(label);
                            return null;    // No errors
                        }),
                        new ValueColor<ToolBar>("Color", Doc("The color of the toolbar and the default color of all the elements."), (t, c) => { t.BackColor = c; return null; }),
                        new ValueInt<ToolBar>(
                            "Index", 
                            Doc("The relative display order index of the toolbar.<br/>The toolbar will be shown below toolbars with a lower index."),
                            (t, v) => { t.Index = Convert.ToInt32(v.Value); return null; }),
                        new Flag<ToolBar>("Separator", Usage.Element, Doc("A separator line between the previous and the succeeding elements of the toolbar."), (t, f) =>
                        {
                            var separator = new Separator("Separator");
                            t.Items.Add(separator);
                            return null;
                        }),
                        new Flag<ToolBar>("ColumnSeparator", Usage.Element, Doc("A separator line between the previous and the succeeding elements of the toolbar.<br/>All separators of this type and the same name will be graphically aligned with the rightmost one."), (t, f) =>
                        {
                            string name = f.Name;
                            if (!f.HasTypeSpecified)
                            {
                                int index = t.Items.Cast<ToolStripItem>().Count(i => i is ColumnSeparator);
                                name = "column" + index;
                            }
                            var separator = new ColumnSeparator(name);
                            t.Items.Add(separator); t.Items.Add(separator);
                            return null;
                        }),
                        new ValueString<ToolBar>("Instance", Doc("Blah blah blah"), (t, v) => { t.SetChildProperty("Instance", v.ValueAsString()); return null; }),
                        toolbarMenu, button, textbox
                    );
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!m_settingDefaultColor)
            {
                m_colorSet = true;
            }
        }

        public int Index { get { return m_index; } set { m_index = value; } }

        public new Color DefaultBackColor
        {
            set
            {
                if (!m_colorSet)
                {
                    m_settingDefaultColor = true;
                    this.BackColor = value;
                    m_settingDefaultColor = false;
                }
            }
        }

        private void SetChildProperty(string name, object value)
        {
            if (m_commonChildFields == null)
            {
                m_commonChildFields = new Dictionary<string, object>();
            }
            m_commonChildFields[name] = value;
        }

        public IEnumerable<ColumnSeparator> ListColumns()
        {
            foreach (var column in this.Items.Cast<ToolStripItem>().Where(item => item is ColumnSeparator).Cast<ColumnSeparator>())
            {
                yield return column;
            }
        }


        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.SizeChanged += Control_SizeChanged;
            System.Diagnostics.Debug.WriteLine("Added control: " + e.Control.GetType().FullName);
        }

        private void Control_SizeChanged(object sender, EventArgs e)
        {
            var item = sender as ToolStripItem;
            if (item is null)
            {
                var parent = (sender as Control).Parent;
                System.Diagnostics.Debug.WriteLine("Control Size Changed: " + parent.GetType().FullName);
                //item = (sender as ToolStripControlHost).Control as Tool
            }
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e)
        {
            base.OnItemAdded(e);
            //e.Item.Si
        }

        protected override void OnLayoutCompleted(EventArgs e)
        {
            base.OnLayoutCompleted(e);
            if (!m_adjustingSizes)
            {
                //if (sender is IResizeable sizeableControl)
                {
                    //if (!String.IsNullOrEmpty(sizeableControl.WidthGroup))
                    {
                        //this.AdjustSizesAndColumns();
                    }
                }
            }
        }

        public void AdjustSizesAndColumns()
        {
            if (this.Parent == null) return;
            m_adjustingSizes = true;
            var toolbars = this.Parent.Controls.Cast<Control>().Where(c => c is ToolBar).Cast<ToolBar>().ToList();

            #region Adjust items width 'WidthGroup'

            var widthGroupSizes = new Dictionary<string, int>();
            var widthGroupedItems = new List<IResizeable>();

            toolbars.ForEach(
                toolbar =>
                {
                    widthGroupedItems.AddRange(toolbar.Items.Cast<ToolStripItem>().Where(i => i is IResizeable && !String.IsNullOrEmpty((i as IResizeable).WidthGroup)).Cast<IResizeable>());
                });
            widthGroupedItems.ForEach(item =>
                {
                    var v = widthGroupSizes.ContainsKey(item.WidthGroup) ? widthGroupSizes[item.WidthGroup] : 0;
                    widthGroupSizes[item.WidthGroup] = Math.Max(v, (item as ToolStripItem).Width);
                });
            widthGroupedItems.ForEach(item =>
            {
                if ((item as ToolStripItem).Width < widthGroupSizes[item.WidthGroup])
                {
                    item.SetWidth(widthGroupSizes[item.WidthGroup]);
                }
            });

            #endregion

            var titleLabels = toolbars.Where(t => t.Items[0] is Label && ((Label)t.Items[0]).Name.Equals("title", StringComparison.InvariantCultureIgnoreCase)).Select(t => t.Items[0]).Cast<Label>().ToList();
            int widest = 0;
            foreach (var label in titleLabels)
            {
                widest = Math.Max(widest, label.Bounds.Left + label.Width);
            }
            foreach (var label in titleLabels)
            {
                label.Margin = new Padding(label.Margin.Left, label.Margin.Top, (widest + 2) - (label.Bounds.Left + label.Width), label.Margin.Bottom);
            }

            var columns = new List<ColumnSeparator>();
            foreach (var toolbar in toolbars)
            {
                columns.AddRange(toolbar.ListColumns());
            }
            var columnNames = columns.Select(c => c.Name).Distinct().ToList();
            var handledColumns = new List<string>();
            foreach (var column in columns)
            {
                if (!handledColumns.Contains(column.Name))
                {
                    var separators = columns.Where(c => c.Name == column.Name);
                    int maxWidth = separators.Select(s => s.Bounds.Left - s.Margin.Left).Max();
                    foreach (var col in separators)
                    {
                        col.Margin = new Padding(maxWidth - (col.Bounds.Left - col.Margin.Left) + 2, col.Margin.Top, col.Margin.Right, col.Margin.Bottom);
                    }
                    handledColumns.Add(column.Name);
                }
            }
            m_adjustingSizes = false;
        }

        public void Clear()
        {
            //foreach (IToolBarElementSetup item in this.Items)
            //{
            //    item.Clear();
            //}
            this.Items.Clear();
        }
        public ICoreAccess Core { get { return m_coreAccess; } }

        public void Setup(ILogger logger, PropertyBlock definition)
        {
            this.Items.Clear();
            var errors = new List<Tuple<int, string>>();
            ((Block<object, ToolBar>)m_decoder.TryGetDecoder()).DecodeData(definition, this, errors);

            foreach (var error in errors)
            {
                logger.LogError($"Toolbar '{this.Name}' line " + error.Item1 + ": " + error.Item2);
            }
        }

        public static void ReportTypeUnknown(ILogger logger, int line, string type)
        {
            if (logger != null)
            {
                logger.LogError("Toolbar definition line " + line + ", unknown type: \"" + type + "\".");
            }
        }

        public void Add(ToolStripItem item)
        {
            this.Items.Add(item);
        }

        public void Add(ToolStripTextBox item)
        {
            this.Items.Add(item);
        }

        private static System.Reflection.PropertyInfo GetElementProperty(object @object, ICallContext context, string property)
        {
            var propAccess = @object.GetType().GetProperty(property, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            if (propAccess == null)
            {
                context.ReportError("Property not found.");
            }
            return propAccess;
        }

        internal static object GetElementPropertyValue(object @object, ICallContext context, string property)
        {
            object returnValue = null;
            var propAccess = GetElementProperty(@object, context, property);

            var action = () =>
            {
                if (propAccess != null)
                {
                    returnValue = propAccess.GetValue(@object);
                }
                else returnValue = null;
            };

            @object.InvokeAction(action);

            return returnValue;
        }

        internal static void SetElementPropertyValue(object @object, ICallContext context, string property, object value)
        {
            var propAccess = GetElementProperty(@object, context, property);

            var action = () =>
            {
                try
                {
                    propAccess.SetValue(@object, value.TryConvert(propAccess.PropertyType), null);
                }
                catch (Exception ex)
                {
                    context.ReportError("Error setting property.", exception: ex);
                }
            };

            @object.InvokeAction(action);
        }

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement { get { return null; } }

        public string ElementName { get { return this.Name; } }

        public string ElementType { get { return "ToolBar"; } }

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        public IEnumerable<IToolBarElement> GetChilds()
        {
            foreach (ToolStripItem item in this.Items)
            {
                if (item is IToolBarElement) yield return item as IToolBarElement;
            }
        }

        [Public]
        public object GetProperty([Implicit] ICallContext context, string property)
        {
            return GetElementPropertyValue(this, context, property);
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        [Public]
        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            SetElementPropertyValue(this, context, property, value);
        }

        public bool SetValue([Implicit] ICallContext context, object value)
        {
            throw new NotImplementedException();
        }

        public IToolBarElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            throw new NotImplementedException();
        }

        public object TryGetChildProperty(string name)
        {
            if (m_commonChildFields != null && m_commonChildFields.ContainsKey(name))
            {
                return m_commonChildFields[name];
            }
            return null;
        }

        #endregion
    }
}
