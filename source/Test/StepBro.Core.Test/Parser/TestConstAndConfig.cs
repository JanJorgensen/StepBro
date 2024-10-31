using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBroCoreTest;
using System;
using System.Linq;

namespace StepBro.Core.Test.Parser
{
    [TestClass]
    public class TestConstAndConfig
    {
        [TestMethod]
        public void ParseConfigVariables()
        {
            var var = FileBuilder.ParseConfigValue<long>("public config int MAX_TEMPERATURE = 10 * 3;");
            Assert.IsNotNull(var);
            Assert.AreEqual("MAX_TEMPERATURE", var.Name);
            Assert.ReferenceEquals(var.DataType, TypeReference.TypeInt64);
            Assert.IsTrue(var.IsReadonly);
            Assert.AreEqual(AccessModifier.Public, var.AccessProtection);
        }

        [TestMethod]
        public void UseConfigVariables()
        {
            string f1 =
                """
                config int MAX_TEMPERATURE = 12 * 3;
                int v1 = MAX_TEMPERATURE;
                
                procedure int Test()
                {
                    var v2 = 100 * MAX_TEMPERATURE;
                    return v1 + v2;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var configVariable = files[0].ListElements().FirstOrDefault(p => p.Name == "MAX_TEMPERATURE");
            Assert.IsNotNull(configVariable);
            Assert.IsNotNull(configVariable.DataType);
            Assert.IsTrue(configVariable.DataType == TypeReference.TypeInt64);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "Test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(3636L, (Int64)result);
        }

        [TestMethod]
        public void ParseConstVariable()
        {
            string f1 =
                """
                const int KILO_PI = 3142;

                procedure int Test()
                {
                    var v1 = 400;
                    return v1 + KILO_PI;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var constElement = files[0].ListElements().FirstOrDefault(p => p.Name == "KILO_PI");
            Assert.IsNotNull(constElement);
            Assert.IsNotNull(constElement.DataType);
            Assert.IsTrue(constElement.DataType == TypeReference.TypeInt64);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "Test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(3542L, (Int64)result);
        }

        [TestMethod]
        public void ParseConstVariableVarSpecified()
        {
            string f1 =
                """
                const var MASS = 771;

                procedure int Test()
                {
                    var v1 = 100;
                    return v1 + MASS;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var constElement = files[0].ListElements().FirstOrDefault(p => p.Name == "MASS");
            Assert.IsNotNull(constElement);
            Assert.IsNotNull(constElement.DataType);
            Assert.IsTrue(constElement.DataType == TypeReference.TypeInt64);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "Test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(871L, (Int64)result);
        }
    }
}
