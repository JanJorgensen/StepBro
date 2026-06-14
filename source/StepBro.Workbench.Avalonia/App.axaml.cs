using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.HostSupport.Models;
using StepBro.UI.Controls;
using StepBro.Workbench.ViewModels;
using StepBro.Workbench.Views;
using System.Linq;
using System.Threading;
using StepBro.Core.Logging;

namespace StepBro.Workbench
{
    public partial class App : Application
    {
        private ILogger threadLogger = null;
        private bool m_closing = false;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = new MainWindowViewModel();
                mainViewModel.StepBroHostModel = new HostAppModel();
                var logViewerModel = new LogViewerModel(new LogViewEntryFactory());
                IService m_textFileSystemService = null;
                new TextFileSystem(out m_textFileSystemService);
                mainViewModel.StepBroHostModel.Initialize(logViewerModel, m_textFileSystemService);
                logViewerModel.Setup();
                //logViewer.DataContext = logViewerModel;


                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };
                ((MainWindow)desktop.MainWindow).StartLogging();
            }

            base.OnFrameworkInitializationCompleted();
        }

        void LoggerThread()
        {
            System.Random rnd = new System.Random();
            while (!m_closing)
            {
                Thread.Sleep(50);

                for (int i = 0; i < 10; i++)
                {
                    threadLogger.Log("Spunk " + StepBro.Core.Data.AlphaID.Create((uint)rnd.Next(2000000000), 5));
                }
            }
        }

    }
}