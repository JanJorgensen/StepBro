using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StepBro.UI.Panels
{
    public abstract class CustomPanelType
    {
        public string Name { get; private set; }
        public abstract string TypeIdentification { get; }
        public string Description { get; private set; }
        public bool IsObjectPanel { get; private set; }
        public bool AllowMultipleInstances { get; private set; }


        internal CustomPanelType(string name, string description, bool isObjectPanel, bool allowMultpile)
        {
            this.Name = name;
            this.Description = description;
            this.IsObjectPanel = isObjectPanel;
            this.AllowMultipleInstances = allowMultpile;
        }

        public virtual bool IsCompatibleWithType(Type type)
        {
            return false;
        }

        public virtual bool IsCompatibleWithObject(object @object)
        {
            if (@object == null) return false;
            else
            {
                return this.IsCompatibleWithType(@object.GetType());
            }
        }

        public abstract UserControl CreatePanelView();

        /// <summary>
        /// Tries to bind created custom panel to the specified object.
        /// </summary>
        /// <param name="object">The data context object for the panel.</param>
        /// <returns>Whether the binding succeeded.</returns>
        public virtual bool TryBind(object @object)
        {
            if (this.IsObjectPanel) throw new NotImplementedException($"{this.GetType().FullName} should override this method ({nameof(this.TryBind)}).");
            else throw new NotSupportedException("This panel is not bindable (it is a static panel).");
        }

        /// <summary>
        /// Sets the object binding for a created custom panel.
        /// </summary>
        /// <param name="object">The data context object for the panel. This recference can be <code>null</code> when the object is disposed.</param>
        /// <returns>Whether the binding succeeded.</returns>
        public virtual bool SetPanelObjectBinding(UserControl control, object @object)
        {
            if (this.IsObjectPanel) throw new NotImplementedException($"{this.GetType().FullName} should override this method ({nameof(this.TryBind)}).");
            else throw new NotSupportedException("This panel is not bindable (it is a static panel).");
        }
    }

    public class CustomPanelType<TPanel, TObject> : CustomPanelType 
        where TPanel : UserControl 
        where TObject : class
    {
        public CustomPanelType(string name, string description, bool allowMultiple) :
            base(name, description, true, allowMultiple)
        {
        }

        public override string TypeIdentification { get { return typeof(TPanel).FullName; } }

        public override bool IsCompatibleWithType(Type type)
        {
            return type.IsAssignableFrom(typeof(TObject));
        }

        public override UserControl CreatePanelView()
        {
            var panel = System.Activator.CreateInstance<TPanel>();
            return (UserControl)panel;
        }

        public override bool TryBind(object @object)
        {
            return this.TryBind(@object as TObject);
        }

        public virtual bool TryBind(TObject @object)
        {
            return (@object != null);
        }

        public override bool SetPanelObjectBinding(UserControl control, object @object)
        {
            return this.SetPanelObjectBinding(control as TPanel, @object as TObject);
        }

        public virtual bool SetPanelObjectBinding(TPanel control, TObject @object)
        {
            throw new NotImplementedException($"{this.GetType().FullName} should override this method ({nameof(this.SetPanelObjectBinding)}).");
        }
    }

    public class CustomPanelType<TPanel, TPanelVM, TObject> : CustomPanelType<TPanel, TObject>
        where TPanel : UserControl
        where TObject : class
    {
        public CustomPanelType(string name, string description, bool allowMultiple) :
            base(name, description, allowMultiple)
        {
        }

        public override UserControl CreatePanelView()
        {
            var panel = System.Activator.CreateInstance<TPanel>();
            var vm = System.Activator.CreateInstance<TPanelVM>();
            panel.DataContext = vm;
            return panel;
        }
    }

    public class CustomPanelType<TPanel> : CustomPanelType where TPanel : UserControl
    {
        public CustomPanelType(string name, string description, bool allowMultiple) :
            base(name, description, false, allowMultiple)
        {
        }
        public override string TypeIdentification { get { return typeof(TPanel).FullName; } }

        public override UserControl CreatePanelView()
        {
            var panel = System.Activator.CreateInstance<TPanel>();
            return (UserControl)panel;
        }
    }
}
