using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedureParsing
    {
        [TestMethod]
        public void TestProcedureParametersNone()
        {
            var proc = FileBuilder.ParseProcedure("void Func(){}");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersOnlyReturn()
        {
            var proc = FileBuilder.ParseProcedure("int Func(){}");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersSingleInput()
        {
            var proc = FileBuilder.ParseProcedure("void Func(int a){}");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersWithDefaultValue()
        {
            var proc = FileBuilder.ParseProcedure("void Func(int a = 30){}");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersInterfaceReference()
        {
            var proc = FileBuilder.ParseProcedure(
                typeof(TestProcedureParsing),
                "void Func( " + typeof(ISomeInterface).Namespace + "." + nameof(ISomeInterface) + " reference){ }");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);
            Assert.AreEqual(typeof(ISomeInterface), proc.Parameters[0].Value.Type);
        }

        [TestMethod]
        public void TestSimpleVoidProcedureExecution()
        {
            var proc = FileBuilder.ParseProcedure("void Func(){}");
            Assert.AreEqual(0, proc.Parameters.Length);

            var ret = proc.Call();
            Assert.IsNull(ret);
        }


        public interface ISomeInterface
        {
            bool BooleanProperty { get; set; }
            int IntegerReturn();
            string StringReturn(int input);
        }
    }
}
