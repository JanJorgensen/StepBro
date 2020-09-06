using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Attributes
{
    /// <summary>
    /// Makes TSharp scripts use the tagged method as if it was a property (syntax). 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AsPropertyAttribute : Attribute
    {
        public AsPropertyAttribute() { }
    }
}
