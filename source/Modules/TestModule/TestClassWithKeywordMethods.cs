using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using StepBro.Core.Logging;
using StepBro.Core.Api;

namespace TestModule
{
    public class TestClassWithKeywordMethods
    {
        public System.Reflection.MethodInfo Send(IEnumerable<KeywordItem> keywords)
        {
            return null;
        }

        private string DoSendAfterDelay([Implicit] StepBro.Core.Execution.ICallContext context, string item, TimeSpan delay, string password, int identifier)
        {
            return "We sent the \"" + item + "\" after a delay. Keyword: \"" + password + "\". ID: " + identifier.ToString();
        }

        //public string Send(
        //    [Implicit] StepBro.Core.Execution.ICallContext context,
        //    [Implicit] object[] keywords
        //    )
        //{
        //    return "";
        //}
    }
}
