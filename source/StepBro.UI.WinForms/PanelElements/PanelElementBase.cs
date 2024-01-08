using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.PanelCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.PanelElements
{
    public partial class PanelElementBase : UserControl, IPanelElement
    {
        protected class ProcedureInfo
        {
            public string Name { get; set; } = null;
            public string Partner { get; set; } = null;
            public string TargetObject { get; set; } = null;
            //public bool FirstParameterIsSelf { get; set; } = false;
            public bool IsUsed { get { return !string.IsNullOrEmpty(Name); } }
        }


        private static uint g_nextID = 100;
        private uint m_id;
        private IPanelElement m_parent = null;
        private List<IPanelElement> m_children;

        public PanelElementBase()
        {
            m_id = g_nextID++;
        }

        public ICoreAccess CoreAccess { get; set; } = null;

        public Color SetForeColor { get; set; } = Color.Empty;
        public Color SetBackColor { get; set; } = Color.Empty;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.DoResizeToContents();
        }

        public bool ResizeToContents { get; set; } = false;

        protected void DoResizeToContents()
        {
            if (this.ResizeToContents)
            {
                int width = 0, height = 0;
                foreach (Control c in this.Controls)
                {
                    width = Math.Max(width, c.Left + c.Width);
                    height = Math.Max(height, c.Top + c.Height);
                }
                this.Size = new Size(width, height);
            }
        }

        #region IPanelElement interface

        public uint Id
        {
            get { return m_id; }
        }

        IPanelElement IPanelElement.Parent { get { return m_parent; } }

        public string PropertyName => throw new NotImplementedException();

        public string ElementName { get {  return this.Name; } }

        public string ElementType => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<IPanelElement> GetChilds()
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

        public IPanelElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            if (name.Contains('.'))
            {
                var i = name.IndexOf('.');
                var first = name.Substring(0, i);
                var child = m_children.FirstOrDefault(c => c.ElementName == first);
                if (child != null)
                {
                    child = child.TryFindChildElement(context, name.Substring(i + 1));
                }
                return child;
            }
            else
            {
                return m_children.FirstOrDefault(c => c.ElementName == name);
            }
        }

        #endregion

        protected virtual bool Setup(PanelElementBase parent, PropertyBlock definition)
        {
            m_parent = parent;
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var propValue = element as PropertyBlockValue;
                    if (propValue.Name == "Name" || propValue.Name == "Title")
                    {
                        this.Name = propValue.ValueAsString();
                    }
                    else if (propValue.Name == "Width")
                    {
                        this.Size = new Size(Convert.ToInt32(propValue.Value), this.Size.Height);
                    }
                    else if (propValue.Name == "Height")
                    {
                        this.Size = new Size(this.Size.Width, Convert.ToInt32(propValue.Value));
                    }
                    else if (propValue.Name == "Color")
                    {
                        try
                        {
                            Color color = (Color)(typeof(Color).GetProperty(propValue.ValueAsString()).GetValue(null));
                            this.SetBackColor = color;
                        }
                        finally { }
                    }
                    else if (propValue.Name == "ForeColor")
                    {
                        try
                        {
                            Color color = (Color)(typeof(Color).GetProperty(propValue.ValueAsString()).GetValue(null));
                            this.SetForeColor = color;
                        }
                        finally { }
                    }
                }
            }
            return true;
        }

        static public PanelElementBase Create(PanelElementBase elementParent, PropertyBlock definition, ICoreAccess coreAccess)
        {
            PanelElementBase createdElement = null;
            var type = definition.SpecifiedTypeName;
            var name = definition.Name;
            if (type != null)
            {
                if (type == nameof(FlowTopDown))
                {
                    createdElement = new FlowTopDown();
                }
                else if (type == nameof(FlowLeftRight))
                {
                    createdElement = new FlowLeftRight();
                }
                else if (type == nameof(ProcedureActivationButton))
                {
                    createdElement = new ProcedureActivationButton();
                }
                else if (type == nameof(ProcedureActivationCheckbox))
                {
                    createdElement = new ProcedureActivationCheckbox();
                }
            }
            if (createdElement != null)
            {
                createdElement.CoreAccess = coreAccess;
                createdElement.Name = name;     // Set name before Setup, because the name may be overridden.
                createdElement.Setup(elementParent, definition);
            }
            return createdElement;
        }
    }
}
