using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBroCoreTest.Data;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedure_UsingStatement
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceManager.Dispose();
        }

        [TestMethod]
        public void TestSimpleUsing_ExistingVar()
        {
            var proc = FileBuilder.ParseProcedure(
                typeof(DummyClass),
                "int Func()",
                "{",
                "   "+ typeof(DummyClass).FullName +" obj = null;",
                "   using (obj = " + typeof(DummyClass).FullName + ".Create()) { obj.PropInt = 16; }",
                "   return obj.PropInt; ",
                "}");

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1096L, (long)result);
        }

        [TestMethod]
        public void TestSimpleUsing_ExistingVarAlreadyCreated()
        {
            var proc = FileBuilder.ParseProcedure(
                typeof(DummyClass),
                "int Func()",
                "{",
                "   var obj = " + typeof(DummyClass).FullName + ".Create();",
                "   using (obj) { obj.PropInt = 27; }",
                "   return obj.PropInt; ",
                "}");

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1107L, (long)result);
        }

        [TestMethod]
        public void TestSimpleUsing_CreateVar()
        {
            var proc = FileBuilder.ParseProcedure(
                typeof(DummyClass),
                "int Func()",
                "{",
                "   " + typeof(DummyClass).FullName + " cpy = null;",
                "   using (var obj = " + typeof(DummyClass).FullName + ".Create()) { obj.PropInt = 19; cpy = obj; }",
                "   return cpy.PropInt; ",
                "}");

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1099L, (long)result);
        }
    }
}
