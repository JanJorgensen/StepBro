using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.PanelCreator
{
    [Public]
    public interface IPanelElement : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the unique id of the panel element.
        /// </summary>
        uint Id { get; }
        /// <summary>
        /// Reference to the parent element;
        /// </summary>
        IPanelElement Parent { get; }
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

    //[Public]
    //public interface IPanelInput
    //{
    //    void NotifyObjectPropertyValue(string @object, string @property, object value);
    //    void NotifyProcedureResult(string name, string partner, object result);
    //    void RequestSetPanelElementProperty(string element, object value);
    //}

    //[Public]
    //public interface IPanelSystemAccess
    //{
    //    void RequestObjectPropertyValue(string @object, string @property);
    //    void RequestProcedureExecution(string name, string partner, params object[] arguments);
    //}

}
