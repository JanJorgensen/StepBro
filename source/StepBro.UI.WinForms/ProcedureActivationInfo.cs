using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms
{
    internal class ProcedureActivationInfo
    {
        public string Name { get; set; } = null;
        public string Partner { get; set; } = null;
        public string TargetObject { get; set; } = null;
        public List<object> Arguments { get; set; } = null;
        public bool IsUsed { get { return !string.IsNullOrEmpty(Name); } }
    }
}
