using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestKeywordFunctionCall
    {
        [TestMethod]
        [Ignore]    // Needs some work
        public void TestSimpleKeyword()
        {
            FileBuilder.ParseKeywordProcedureCall("port.Send Telegram2 after 2s (\"Plaf\", 17) : { c = 15 };");


            //var fcn = TSharpFileBuilder.ParseFunction("void Func(){}");
            //Assert.AreEqual(0, fcn.Parameters.Length);

            //fcn = TSharpFileBuilder.ParseFunction("void Func(int a){}");
            //Assert.AreEqual(1, fcn.Parameters.Length);
            //Assert.IsTrue(result.Value is long);
            //Assert.AreEqual(16L, (long)result.Value);
        }
    }
}
