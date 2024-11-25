using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.ControlModels
{
    public partial class SettingsOptionModel : ObservableObject
    {
        [ObservableProperty]
        private Bitmap? m_optionIcon = null;

        //ImageHelper.LoadFromResource(new Uri("avares://LoadingImages/Assets/abstract.jpg"));

        public bool HasOptionIcon => this.OptionIcon != null;

        [ObservableProperty]
        private bool m_optionValue = false;

        public bool OptionValueIsTrue { get { return this.OptionValue == true; } }
        public bool OptionValueIsFalse { get { return this.OptionValue == false; } }

        [ObservableProperty]
        private string? m_optionName = "Name";

        [ObservableProperty]
        private string m_optionSettingName = "Setting";

        public bool UserCanSetFalse { get; set; } = false;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(OptionValue))
            {
                OnPropertyChanged(nameof(this.OptionValueIsTrue));
                OnPropertyChanged(nameof(this.OptionValueIsFalse));
            }
        }
    }
}
