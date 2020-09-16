using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule
{
    public class TestClassWithSimpleMethods
    {
        [Public]
        public static IEnumerable<string> MethodListSomeNames()
        {
            yield return "Anders";
            yield return "Berditto";
            yield return "Chrushtor";
            yield return "Dowfick";
        }
    }
}
