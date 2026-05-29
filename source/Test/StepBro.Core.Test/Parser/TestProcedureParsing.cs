using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System.Linq;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedureParsing
    {
        [TestMethod]
        public void TestProcedureParametersNone()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors("void Func(){}");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersOnlyReturn()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors("int Func(){}");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersSingleInput()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors("void Func(int a){}");
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);
        }

        [TestMethod]
        public void TestProcedureParametersWithDefaultValue()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors("void Func(int a = 30){}") as FileProcedure;
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.GetFormalParameters().Count);
            Assert.AreEqual(30, (long)proc.GetFormalParameters()[0].DefaultValue);

            proc = FileBuilder.ParseProcedureExpectNoErrors("void Func(string b = \"Nix!\"){}") as FileProcedure;
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.GetFormalParameters().Count);
            Assert.AreEqual("Nix!", (string)proc.GetFormalParameters()[0].DefaultValue);
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
            var proc = FileBuilder.ParseProcedureExpectNoErrors("void Func(){}");
            Assert.AreEqual(0, proc.Parameters.Length);

            var ret = proc.Call();
            Assert.IsNull(ret);
        }

        [TestMethod]
        public void TestProcedureUnknownIdentifierWithAsyncMethodCall()
        {
            var proc = FileBuilder.ParseProcedure("void Func(){ int b = 5; if (b == 5) await harness.SendDirect(\"hi\"); }");
            Assert.AreEqual(1, FileBuilder.LastInstance.Errors.ErrorCount);
        }

        [TestMethod]
        public void TestProcedureUnknownMethodCallWithAsyncMethodCall()
        {
            try {
                var proc = FileBuilder.ParseProcedure("void Func(){ bool test = await testObj.DoesntExist(); }");
            }
            catch
            {}
            Assert.AreEqual("Unresolved identifier: \"testObj\".", FileBuilder.LastInstance.Errors[0].Message);
            Assert.AreEqual(1, FileBuilder.LastInstance.Errors.ErrorCount);
        }

        [TestMethod]
        public void TestProcedureReturnInNestedIfStatement()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                void Main()
                {
                    bool b = true;
                    if (b == true)
                    {
                        log("Do stuff.");
                        if (b == true) 
                        {
                            return;
                        }
                    }
                    log("Should not execute this.");
                }
                """);
            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
        }


        //----------------------------------------------------------------------------------------------------------------------------------

        public interface ISomeInterface
        {
            bool BooleanProperty { get; set; }
            int IntegerReturn();
            string StringReturn(int input);
        }
    }
}
