﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.Host
{
    public enum HostType
    {
        Console,
        WinForms,
        WPF,
        Mock
    }

    public interface IHost : IObjectHost
    {
        HostType Type { get; }
        void LogUserAction(string text);
        void LogSystem(string text);
        IEnumerable<Type> ListHostCodeModuleTypes();
        bool SupportsUserInteraction { get; }
        UserInteraction SetupUserInteraction(ICallContext context, string header);
    }
}
