using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public interface IChronoListView
    {
        int HorizontalScrollPosition { get; }
        ChronoListViewDynamicSettings DynamicSettings { get; }
        Font NormalFont { get; }
        Brush NormalTextColor { get; }
    }
}
