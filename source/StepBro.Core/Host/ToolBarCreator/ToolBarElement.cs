using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.ToolBarCreator
{
    [Public]
    public interface IToolBarElement : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the unique id of the toolbar element.
        /// </summary>
        uint Id { get; }
        /// <summary>
        /// Reference to the parent element;
        /// </summary>
        IToolBarElement ParentElement { get; }
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
        IToolBarElement TryFindChildElement([Implicit] ICallContext context, string name);
        IEnumerable<IToolBarElement> GetChilds();
        object TryGetChildProperty(string name);
    }

    public static class ToolBarSupport
    {
        public static string GetQualifiedName(this IToolBarElement element)
        {
            if (element.ParentElement == null) { return element.ElementName; }
            else return GetQualifiedName(element.ParentElement) + "." + element.ElementName;
        }
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
