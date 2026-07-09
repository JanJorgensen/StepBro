using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StepBro.HostSupport.Models;
using System.Runtime.CompilerServices;

namespace StepBro.Workbench.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            this.PrimaryFileExplorerCommand = new RelayCommand(() => { this.PrimaryFileExplorerSelected = !this.PrimaryFileExplorerSelected; });
            this.PrimarySettingsCommand = new RelayCommand(() => { this.PrimarySettingsSelected = !this.PrimarySettingsSelected; });
        }

        public HostAppModel StepBroHostModel { get; set; } = null;


        public RelayCommand PrimaryFileExplorerCommand { get; }
        public RelayCommand PrimarySettingsCommand { get; }

        private bool m_isPrimarySelectionSlave = false;
        [ObservableProperty]
        private bool m_primaryPanelVisible = true;      // Indicates to Primary Panel properties that their value is being reset by another property.

        private bool m_primaryFileExplorerSelected = true;
        public bool PrimaryFileExplorerSelected
        {
            get { return m_primaryFileExplorerSelected; }
            set
            {
                if (value == true)
                {
                    m_isPrimarySelectionSlave = true;
                    this.PrimarySettingsSelected = false;
                    m_isPrimarySelectionSlave = false;
                    this.PrimaryFileExplorerButtonBackground = Brushes.LightSkyBlue;
                }
                else
                {
                    this.PrimaryFileExplorerButtonBackground = Brushes.Transparent;
                }
                this.SetPrimaryPanelSettingProperty(ref m_primaryFileExplorerSelected, value);
            }
        }
        [ObservableProperty]
        private IBrush m_primaryFileExplorerButtonBackground = Brushes.Transparent;

        private bool m_primarySettingsSelected = false;
        public bool PrimarySettingsSelected
        {
            get { return m_primarySettingsSelected; }
            set
            {
                if (value == true)
                {
                    m_isPrimarySelectionSlave = true;
                    this.PrimaryFileExplorerSelected = false;
                    m_isPrimarySelectionSlave = false;
                    this.PrimarySettingsButtonBackground = Brushes.LightSkyBlue;
                }
                else
                {
                    this.PrimarySettingsButtonBackground = Brushes.Transparent;
                }
                SetPrimaryPanelSettingProperty(ref m_primarySettingsSelected, value);
            }
        }
        [ObservableProperty]
        private IBrush m_primarySettingsButtonBackground = Brushes.Transparent;

        private bool SetPrimaryPanelSettingProperty(ref bool field, bool value, [CallerMemberName] string? propertyName = null)
        {
            System.Diagnostics.Debug.WriteLine($"Primary Panel Prop: {propertyName} -> {value}");
            if (value == false && !m_isPrimarySelectionSlave)
            {
                this.PrimaryPanelVisible = false;
            }
            var wasChanged = this.SetProperty(ref field, value, propertyName);
            if (!m_isPrimarySelectionSlave && value == true)
            {
                this.PrimaryPanelVisible = true;
            }
            return wasChanged;
        }
    }
}
