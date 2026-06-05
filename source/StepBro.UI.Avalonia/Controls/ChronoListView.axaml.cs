using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using StepBro.HostSupport.Models;
using System;

namespace StepBro.UI.Controls;

public partial class ChronoListView : UserControl
{
    ChronoListViewModel<ChronoListViewEntry> m_model = null;

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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (this.DataContext is ChronoListViewModel<ChronoListViewEntry> model)
        {
            m_model = model;
            viewPort.Setup(model.ViewPort);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.End && e.KeyModifiers == KeyModifiers.Control)
        {
            m_model.HeadMode = true;
        }
    }
}