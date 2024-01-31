using StepBro.Core.Api;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal interface IToolBarElementSetup
    {
        void Clear();
        ICoreAccess Core { get; }
        void Setup(PropertyBlock definition);
    }
}
