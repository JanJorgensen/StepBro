using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms
{
    internal class ActivationInfo
    {
        public string FileElementName { get; set; } = null;
        public string Partner { get; set; } = null;
        public string TargetObject { get; set; } = null;
        public string ObjectCommand { get; set; } = null;
        public List<object> Arguments { get; set; } = null;

        public bool IsUsed { get { return !string.IsNullOrEmpty(FileElementName) || !string.IsNullOrEmpty(ObjectCommand); } }
        public bool IsFileElementUsed { get { return !string.IsNullOrEmpty(FileElementName); } }
        public bool IsObjectCommandUsed { get { return !string.IsNullOrEmpty(ObjectCommand); } }
    }
}
