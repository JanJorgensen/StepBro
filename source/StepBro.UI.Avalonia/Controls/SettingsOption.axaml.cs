using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using StepBro.UI.ControlModels;

namespace StepBro.UI.Controls;

public partial class SettingsOption : UserControl
{
    public SettingsOption()
    {
        InitializeComponent();
        this.DataContext = new SettingsOptionModel();
    }

    public SettingsOptionModel? Data { get { return this.DataContext as SettingsOptionModel; } }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        if (this.Data != null)
        {
            if (this.Data.OptionValue == false || this.Data.UserCanSetFalse)
            {
                this.Data.OptionValue = !this.Data.OptionValue;
            }
        }
    }
}