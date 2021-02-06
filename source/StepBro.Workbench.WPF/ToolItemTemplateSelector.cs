using StepBro.Workbench.ToolViews;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.Workbench
{
    public class ToolItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ErrorsViewTemplate { get; set; }
        public DataTemplate OutputViewTemplate { get; set; }
        public DataTemplate CalculatorTemplate { get; set; }
        public DataTemplate ObjectPanelTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ErrorsViewModel)
                return this.ErrorsViewTemplate;
            else if (item is OutputViewModel)
                return this.OutputViewTemplate;
            else if (item is CalculatorViewModel)
                return this.CalculatorTemplate;
            else if (item is ObjectPanelToolViewModel)
                return this.ObjectPanelTemplate;
            else
                return null;
        }
    }
}
