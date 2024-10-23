using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.SimpleWorkbench
{
    internal class Shortcuts
    {
        public static string ScripExecutionButtonTitle(bool showFullName, string element, string partner, bool partnerIsModel, string objectVariable, object[] args)
        {
            var elementName = (showFullName && String.IsNullOrEmpty(objectVariable)) ? element : element.Split('.').Last();
            if (!String.IsNullOrEmpty(objectVariable))
            {
                if (showFullName)
                {
                    return objectVariable + "." + elementName;
                }
                else
                {
                    return objectVariable.Split('.').Last() + "." + elementName;
                }
            }
            else if (!String.IsNullOrEmpty(partner))
            {
                if (partnerIsModel)
                {
                    return elementName + " using '" + partner + "'";
                }
                else
                {
                    return elementName + "." + partner;
                }
            }
            else
            {
                return elementName;
            }
        }

        internal class ScriptExecutionToolStripMenuItem : ToolStripMenuItem
        {
            public ScriptExecutionToolStripMenuItem() { }

            public ScriptExecutionToolStripMenuItem(IFileElement element, string partner, string instanceObject)
            {
                this.FileElement = element.FullName;
                Partner = partner;
                InstanceObject = instanceObject;
            }

            public bool ShowFullName { get; set; } = false;
            public string FileElement { get; set; } = null;
            public string Partner { get; set; } = null;
            public bool PartnerIsModel { get; set; } = false;
            public string InstanceObject { get; set; } = null;

            public void SetText()
            {
                this.Text = ScripExecutionButtonTitle(this.ShowFullName, this.FileElement, this.Partner, this.PartnerIsModel, this.InstanceObject, null);
            }

            public bool Equals(string element, string partner, string instanceObject)
            {
                if (!String.Equals(element, this.FileElement, StringComparison.InvariantCulture)) return false;
                if (String.IsNullOrEmpty(partner) != String.IsNullOrEmpty(this.Partner)) return false;
                if (!String.Equals(partner, this.Partner)) return false;
                if (!String.Equals(instanceObject, this.InstanceObject)) return false;
                return true;
            }
        }

        internal class ObjectCommandToolStripMenuItem : ToolStripMenuItem
        {
            public ObjectCommandToolStripMenuItem() { }

            public ObjectCommandToolStripMenuItem(string text, string instance, string command)
            {
                this.Text = text;
                this.Instance = instance;
                this.Command = command;
            }

            public string Instance { get; set; } = null;
            public new string Command { get; set; } = null;

            public bool Equals(string text, string instance, string command)
            {
                if (!String.Equals(text, this.Text, StringComparison.InvariantCulture)) return false;
                if (!String.Equals(instance, this.Instance, StringComparison.InvariantCulture)) return false;
                if (!String.Equals(command, this.Command, StringComparison.InvariantCulture)) return false;
                return true;
            }
        }
    }
}
