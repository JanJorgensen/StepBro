using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms
{
    public interface IResizeable
    {
        int MaxWidth { get; }
        void SetWidth(int width);
        int GetPreferredWidth();
        string WidthGroup { get; }
    }
}
