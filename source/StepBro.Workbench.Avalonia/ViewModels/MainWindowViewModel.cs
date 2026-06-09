using StepBro.HostSupport.Models;

namespace StepBro.Workbench.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public HostAppModel StepBroHostModel { get; set; } = null;

#pragma warning disable CA1822 // Mark members as static
        public string Greeting => "Welcome to the Workbench!";
#pragma warning restore CA1822 // Mark members as static
    }
}
