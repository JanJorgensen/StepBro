using ActiproSoftware.Windows;
using System;

namespace StepBro.TestInterface.UI
{
    public class CommandTerminalViewModel : ObservableObjectBase
    {
        private SerialTestConnection m_connecton = null;

        public SerialTestConnection Connection
        {
            get { return m_connecton; }
            set
            {
                if (!Object.ReferenceEquals(value, m_connecton))
                {
                    m_connecton = value;
                    this.NotifyPropertyChanged(nameof(Connection));
                }
            }
        }
    }
}
