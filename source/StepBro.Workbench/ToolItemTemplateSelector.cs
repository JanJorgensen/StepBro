using StepBro.Workbench.ToolViews;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.Workbench
{
    public class ToolItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PropertiesViewTemplate { get; set; }
        public DataTemplate ErrorsViewTemplate { get; set; }
        public DataTemplate OutputViewTemplate { get; set; }
        public DataTemplate CalculatorTemplate { get; set; }
        public DataTemplate CustomPanelTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PropertiesViewModel)
                return this.PropertiesViewTemplate;
            else if (item is ErrorsViewModel)
                return this.ErrorsViewTemplate;
            else if (item is OutputViewModel)
                return this.OutputViewTemplate;
            else if (item is CalculatorViewModel)
                return this.CalculatorTemplate;
            else if (item is CustomPanelToolViewModel)
                return this.CustomPanelTemplate;
            else
                return null;
        }
    }
}
