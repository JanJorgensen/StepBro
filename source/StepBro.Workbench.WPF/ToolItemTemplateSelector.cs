using StepBro.Workbench.ToolViews;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.Workbench
{
    public class ToolItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CalculatorTemplate { get; set; }
        public DataTemplate ToolItem1Template { get; set; }
        public DataTemplate ToolItem2Template { get; set; }
        public DataTemplate ToolItem3Template { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CalculatorViewModel)
                return this.CalculatorTemplate;
            else if (item is ToolItem1ViewModel)
                return this.ToolItem1Template;
            else if (item is ToolItem2ViewModel)
                return this.ToolItem2Template;
            else if (item is ToolItem3ViewModel)
                return this.ToolItem3Template;
            else
                return null;
        }
    }
}
