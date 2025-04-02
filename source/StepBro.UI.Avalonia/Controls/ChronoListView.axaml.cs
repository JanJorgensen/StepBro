using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace StepBro.UI.Controls;

public partial class ChronoListView : UserControl
{
    public ChronoListView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        //panelScrollbarAlignment.Height = scrollbarHorizontal.Height;
        //panelScrollbarAlignment.Width = scrollbarVertical.Width;
    }
}