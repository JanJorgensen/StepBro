using ActiproSoftware.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace StepBro.Workbench.ToolViews
{

    public class CalculatorViewModel : ToolItemViewModel
    {
        public class HistoryEntry
        {
            public string Command { get; set; }
            public string Result { get; set; }
            public Brush LineColor { get; set; }
        }

        private readonly DeferrableObservableCollection<HistoryEntry> m_historyEntries = new DeferrableObservableCollection<HistoryEntry>();
        private DocumentItemViewModel m_activatingDocument = null;

        public CalculatorViewModel()
        {
            this.SerializationId = "ToolCalculator";
            this.Title = "Calculator";

            //m_historyEntries.Add(new HistoryEntry() { Command = "Anders", Result = "Bent", LineColor = Brushes.Beige });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Christian", Result = "Dennis", LineColor = Brushes.White });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Anders", Result = "Bent", LineColor = Brushes.Beige });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Christian", Result = "Dennis", LineColor = Brushes.White });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Anders", Result = "Bent", LineColor = Brushes.Beige });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Christian", Result = "Dennis", LineColor = Brushes.White });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Anders", Result = "Bent", LineColor = Brushes.Beige });
            //m_historyEntries.Add(new HistoryEntry() { Command = "Christian", Result = "Dennis", LineColor = Brushes.White });
        }

        public IList<HistoryEntry> HistoryEntries { get { return m_historyEntries; } }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(this.IsActive) && !this.IsActive)
            {
                this.ActivatingDocument = null;
            }
        }

        /// <summary>
        /// Reference to the document that was active when the calculator was opened (or given the focus).
        /// </summary>
        public DocumentItemViewModel ActivatingDocument
        {
            get { return m_activatingDocument; }
            set
            {
                m_activatingDocument = value;
                this.NotifyPropertyChanged(nameof(ActivatingDocument));
            }
        }
    }

}
