using CommandLine;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.PanelCreator;
using StepBro.ToolBarCreator;
using StepBro.UI.WinForms.PanelElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ToolBar : ToolStrip, StepBro.ToolBarCreator.IToolBarElement, IToolBarElementSetup
    {
        private ICoreAccess m_coreAccess = null;
        bool m_colorSet = false;
        bool m_settingDefaultColor = false;
        //private static List<ColumnSeparator> g_columns = new List<ColumnSeparator>();

        public ToolBar() : base()
        {
            this.GripStyle = ToolStripGripStyle.Hidden;
            this.AutoSize = false;
            this.Height = 26;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!m_settingDefaultColor)
            {
                m_colorSet = true;
            }
        }

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

        public ToolBar(ICoreAccess coreAccess) : this()
        {
            m_coreAccess = coreAccess;
        }

        public void SetCoreAccess(ICoreAccess coreAccess)
        {
            m_coreAccess = coreAccess;
        }

        public void Setup(string name, PropertyBlock definition)
        {
            this.Text = name.Split(".").Last();
            this.Name = name.Replace(' ', '_').Replace(".", "Dot");
            this.Tag = name;
            this.Setup(definition);
        }

        public IEnumerable<ColumnSeparator> ListColumns()
        {
            foreach (var column in this.Items.Cast<ToolStripItem>().Where(item => item is ColumnSeparator).Cast<ColumnSeparator>())
            {
                yield return column;
            }
        }

        public void AdjustColumns()
        {
            var toolbars = this.Parent.Controls.Cast<Control>().Where(c => c is ToolBar).Cast<ToolBar>().ToList();

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
        }

        #region IToolBarElementSetup

        public void Clear()
        {
            foreach (IToolBarElementSetup item in this.Items)
            {
                item.Clear();
            }
            this.Items.Clear();
        }
        public ICoreAccess Core { get { return m_coreAccess; } }

        public void Setup(PropertyBlock definition)
        {
            this.Items.Clear();
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = element as PropertyBlockValue;
                    if (valueField.Name == "Color")
                    {
                        try
                        {
                            Color color = (Color)(typeof(Color).GetProperty(valueField.ValueAsString()).GetValue(null));
                            this.BackColor = color;
                        }
                        finally { }
                    }
                    else if (valueField.TypeOrName == "Label")
                    {
                        if (valueField.IsStringOrIdentifier)
                        {
                            var text = valueField.ValueAsString();
                            Label label;
                            if (valueField.HasTypeSpecified)
                            {
                                label = new Label(valueField.Name);
                            }
                            else
                            {
                                label = new Label("label" + text.Replace(" ", "_").Replace(".", "Dot"));
                            }
                            label.Text = text;
                            this.Items.Add(label);
                        }
                    }
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = element as PropertyBlockFlag;
                    if (element.Name == nameof(Separator))
                    {
                        var separator = new Separator("Separator");
                        this.Items.Add(separator);
                    }
                    else if (element.TypeOrName == nameof(ColumnSeparator))
                    {
                        string name = element.Name;
                        if (!element.HasTypeSpecified)
                        {
                            int index = this.Items.Cast<ToolStripItem>().Count(i => i is ColumnSeparator);
                            name = "column" + index;
                        }
                        var separator = new ColumnSeparator(element.Name);
                        this.Items.Add(separator);
                    }
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Block)
                {
                    var elementBlock = element as PropertyBlock;
                    var type = element.SpecifiedTypeName;
                    if (type != null)
                    {
                        if (type == "Menu")
                        {
                            var menu = new ToolStripDropDownMenu(m_coreAccess);
                            this.Items.Add(menu);
                            menu.Size = new Size(30, 20);
                            menu.AutoSize = true;
                            menu.Setup(elementBlock);
                        }
                        else if (type == nameof(ProcedureActivationButton))
                        {
                            var button = new ProcedureActivationButton(m_coreAccess);
                            this.Items.Add(button);
                            button.Size = new Size(23, 20);
                            button.AutoSize = true;
                            button.Setup(elementBlock);
                        }
                        else if (type == nameof(ObjectCommandButton))
                        {
                            var button = new ObjectCommandButton(m_coreAccess);
                            this.Items.Add(button);
                            button.Size = new Size(23, 20);
                            button.AutoSize = true;
                            button.Setup(elementBlock);
                        }
                        else if (type == nameof(Separator))
                        {
                            var separator = new Separator(element.Name);
                            this.Items.Add(separator);
                            separator.Setup(elementBlock);
                        }
                        else if (type == nameof(ColumnSeparator))
                        {
                            var separator = new ColumnSeparator(element.Name);
                            this.Items.Add(separator);
                            separator.Setup(elementBlock);
                        }
                    }
                    else
                    {
                        if (element.Name == nameof(Separator))
                        {
                            var separator = new Separator("Separator");
                            this.Items.Add(separator);
                            separator.Setup(elementBlock);
                        }
                        else if (element.Name == nameof(ColumnSeparator))
                        {
                            int index = this.Items.Cast<ToolStripItem>().Count(i => i is ColumnSeparator);
                            var separator = new ColumnSeparator("column" + index);
                            this.Items.Add(separator);
                            separator.Setup(elementBlock);
                        }
                    }
                }
            }
        }

        #endregion

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement => throw new NotImplementedException();

        public string PropertyName => throw new NotImplementedException();

        public string ElementName => throw new NotImplementedException();

        public string ElementType => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<IToolBarElement> GetChilds()
        {
            throw new NotImplementedException();
        }

        public object GetProperty([Implicit] ICallContext context, string property)
        {
            throw new NotImplementedException();
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            throw new NotImplementedException();
        }

        public bool SetValue([Implicit] ICallContext context, object value)
        {
            throw new NotImplementedException();
        }

        public IToolBarElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
