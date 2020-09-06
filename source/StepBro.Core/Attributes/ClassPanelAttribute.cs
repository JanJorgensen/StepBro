using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Attributes
{
    public class ClassPanelAttribute : Attribute
    {
        public ClassPanelAttribute(bool allowMultipleInstances)
        {
            this.AllowMultipleInstances = allowMultipleInstances;
        }
        public bool AllowMultipleInstances { get; private set; }
    }
}
