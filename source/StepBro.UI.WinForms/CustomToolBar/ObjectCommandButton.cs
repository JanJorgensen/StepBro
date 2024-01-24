using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.ProcedureActivationButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ObjectCommandButton : ToolStripMenuItem, IToolBarElement, IToolBarElementSetup
    {
        private ICoreAccess m_coreAccess = null;
        private string m_object = null;
        private string m_command = null;

        public ObjectCommandButton(ICoreAccess coreAccess) : base()
        {
            m_coreAccess = coreAccess;
        }

        #region IToolBarElementSetup

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public ICoreAccess Core { get { return m_coreAccess; } }

        public void Setup(PropertyBlock definition)
        {
            this.Name = definition.Name;
            this.Text = definition.Name;   // Just the default text.
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = element as PropertyBlockValue;
                    if (valueField.Name.Equals("Instance", StringComparison.InvariantCultureIgnoreCase) || valueField.Name.Equals("Object", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_object = valueField.ValueAsString();
                    }
                    else if (valueField.Name.Equals("Command", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_command = valueField.ValueAsString();
                    }
                    else if (valueField.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Text = valueField.ValueAsString();
                    }
                    else if (valueField.Name.Equals("Color", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            Color color = (Color)(typeof(Color).GetProperty(valueField.ValueAsString()).GetValue(null));
                            this.BackColor = color;
                        }
                        finally { }
                    }
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = element as PropertyBlockFlag;
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Block)
                {
                    //var type = element.SpecifiedTypeName;
                    //if (type != null)
                    //{
                    //    var elementBlock = element as PropertyBlock;
                    //    if (type == nameof(Menu))
                    //    {
                    //        var menu = new Menu(m_coreAccess);
                    //        this.DropDownItems.Add(menu);
                    //        menu.Setup(element.Name, elementBlock);
                    //    }
                    //}
                }
            }
        }

        #endregion

        protected override void OnClick(EventArgs e)
        {
            m_coreAccess.ExecuteObjectCommand(m_object, m_command);
        }

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
