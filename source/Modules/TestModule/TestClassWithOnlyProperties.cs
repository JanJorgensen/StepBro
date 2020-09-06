using System;
using StepBro.Core.Data;

namespace TestModule
{
    public class TestClassWithOnlyProperties
    {
        bool PropertyA { get; set; }
        long PropertyB { get; set; }
        string PropertyC { get; set; }
        double PropertyD { get; set; }
        TimeSpan PropertyE { get; set; }
        DateTime PropertyF { get; set; }
        Verdict PropertyG { get; set; }
    }
}
