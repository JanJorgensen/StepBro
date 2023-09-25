using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    public interface IExecutionStateButton : INotifyPropertyChanged
    {
        public string Title { get; }
        public bool ShownActivated{ get; }
        public void SetButtonState(bool pushed);
    }

    public interface IExecutionScopeStatus : INotifyPropertyChanged
    {
        string MainText { get; }
        string ProgressText { get; }
        AttentionColor ProgressColor { get; }
        DateTime StartTime { get; }
        TimeSpan? ExpectedExecutionTime { get; }
        ReadOnlyObservableCollection<IExecutionStateButton> Buttons { get; }
    }
}
