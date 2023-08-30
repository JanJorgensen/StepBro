using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.PanelCreator
{
    public interface IPanelElement
    {
        /// <summary>
        /// The name of the parent panel's property that holds this element. 
        /// </summary>
        string PropertyName { get; }
        /// <summary>
        /// The logical name of this element.
        /// </summary>
        string ElementName { get; }
        /// <summary>
        /// The name of the element type.
        /// </summary>
        string ElementType { get; }
        bool SetValue([Implicit] ICallContext context, object value);
        object GetValue([Implicit] ICallContext context);
        void SetProperty([Implicit] ICallContext context, string property, object value);
        object GetProperty([Implicit] ICallContext context, string property);
        IPanelElement TryFindChildElement([Implicit] ICallContext context, string name);
        IEnumerable<IPanelElement> GetChilds();
    }
}
