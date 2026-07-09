using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StepBro.HostSupport.Models;
using System;

namespace StepBro.UI.Controls;

public partial class LogViewer : UserControl
{
    public LogViewer()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (this.DataContext is LogViewerModel model)
        {
            listView.DataContext = model.ListViewModel;
        }
    }
}