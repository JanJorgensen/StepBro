using StepBro.Core.Api;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class Label : ToolStripLabel, IToolBarElementSetup
    {
        public Label(string name) : base()
        {
            this.Name = name;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.Margin = new Padding(0, 1, 0, 2);
        }

        #region IToolBarElementSetup

        public void Clear()
        {
        }

        public ICoreAccess Core { get { return null; } }

        public void Setup(PropertyBlock definition)
        {
            this.Name = definition.Name;
            this.Text = definition.Name;   // Just the default text.
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = element as PropertyBlockValue;
                    //else if (valueField.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    this.Text = valueField.ValueAsString();
                    //}
                    //else if (valueField.Name.Equals("Color", StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    try
                    //    {
                    //        Color color = (Color)(typeof(Color).GetProperty(valueField.ValueAsString()).GetValue(null));
                    //        this.BackColor = color;
                    //    }
                    //    finally { }
                    //}
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
    }
}
