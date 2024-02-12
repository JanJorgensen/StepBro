using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class MenuLogic
    {
        private ToolStripDropDownItem m_parent;
        ICoreAccess m_coreAccess;

        public MenuLogic(ToolStripDropDownItem parent, ICoreAccess coreAccess)
        {
            m_parent = parent;
            m_coreAccess = coreAccess;
        }


        public void Setup(ILogger logger, PropertyBlock definition)
        {
            m_parent.Name = definition.Name;
            m_parent.Text = definition.Name;
            m_parent.ToolTipText = null;
            m_parent.DropDownItems.Clear();
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = element as PropertyBlockValue;
                    if (valueField.Name == "Text"|| valueField.Name == "Title")
                    {
                        m_parent.Text = valueField.ValueAsString();
                    }
                    else
                    {
                        ToolBar.ReportTypeUnknown(logger, element.Line, valueField.TypeOrName);
                    }
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = element as PropertyBlockFlag;
                    if (element.Name == nameof(Separator))
                    {
                        var separator = new Separator("Separator");
                        m_parent.DropDownItems.Add(separator);
                    }
                    else
                    {
                        ToolBar.ReportTypeUnknown(logger, element.Line, element.TypeOrName);
                    }
                    //if (flagField.Name == nameof(StretchChilds))
                    //{
                    //    StretchChilds = true;
                    //    SizeToChilds = false;
                    //}
                    //else if (flagField.Name == nameof(SizeToChilds))
                    //{
                    //    SizeToChilds = true;
                    //    StretchChilds = false;
                    //}
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Block)
                {
                    var type = element.SpecifiedTypeName;
                    var elementBlock = element as PropertyBlock;
                    if (type != null)
                    {
                        if (type == "Menu")
                        {
                            var menu = new ToolStripMenuSubMenu(m_coreAccess);
                            m_parent.DropDownItems.Add(menu);
                            menu.Setup(logger, elementBlock);
                        }
                        else if (type == nameof(ProcedureActivationButton))
                        {
                            var button = new ProcedureActivationButton(m_coreAccess);
                            m_parent.DropDownItems.Add(button);
                            button.Setup(logger, elementBlock);
                        }
                        else if (type == nameof(ObjectCommandButton))
                        {
                            var button = new ObjectCommandButton(m_coreAccess);
                            m_parent.DropDownItems.Add(button);
                            button.Size = new Size(23, 20);
                            button.AutoSize = true;
                            button.Setup(logger, elementBlock);
                        }
                        else if (type == nameof(Separator))
                        {
                            var separator = new Separator(element.Name);
                            m_parent.DropDownItems.Add(separator);
                            separator.Setup(elementBlock);
                        }
                        else
                        {
                            ToolBar.ReportTypeUnknown(logger, element.Line, type);
                        }
                    }
                    else
                    {
                        if (element.Name == nameof(Separator))
                        {
                            var separator = new Separator(element.Name);
                            m_parent.DropDownItems.Add(separator);
                            separator.Setup(elementBlock);
                        }
                        else
                        {
                            ToolBar.ReportTypeUnknown(logger, element.Line, element.Name);
                        }
                    }
                }
            }
        }
    }
}
