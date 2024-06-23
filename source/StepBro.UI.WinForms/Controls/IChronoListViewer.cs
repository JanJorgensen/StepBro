using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public interface IChronoListViewer
    {
        DateTime ZeroTime { get; }
        IElementIndexer<ChronoListViewEntry> Source { get; }
    }
}
