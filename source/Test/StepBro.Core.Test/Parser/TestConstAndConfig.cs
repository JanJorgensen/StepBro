using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBroCoreTest;
using System;
using System.Linq;
using StepBroCoreTest.Utils;

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
        public void OverrideConfigVariables()
        {
            string f1 =
                """
                using "mysub.sbs";
                override config int MAX_TEMPERATURE = 12 * 5;
                
                procedure int Test()
                {
                    log("MAX_TEMPERATURE: " + MAX_TEMPERATURE);
                    var t = 0;
                    t = GetTemperature();
                    return t;
                }
                """;

            string f2 =
                """
                config int MAX_TEMPERATURE = 12 * 3;
                int v1 = MAX_TEMPERATURE;
                
                procedure int GetTemperature()
                {
                    var v2 = 100 * MAX_TEMPERATURE;
                    return v1 + v2;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()),
                new Tuple<string, string>("mysub.sbs", f2.ToString()));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var configVariable = files[0].ListElements().FirstOrDefault(p => p.Name == "MAX_TEMPERATURE");
            Assert.IsNotNull(configVariable);
            Assert.IsNotNull(configVariable.DataType);
            Assert.IsTrue(configVariable.DataType == TypeReference.TypeInt64);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "Test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6060L, (Int64)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.Test - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - MAX_TEMPERATURE: 60");
            log.ExpectNext("2 - Pre - 8 mysub.GetTemperature - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
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

        [TestMethod]
        public void OverrideConstVariables()
        {
            string f1 =
                """
                using "mysub.sbs";
                override const int MAX_TEMPERATURE = 12 * 5;
                
                procedure int Test()
                {
                    log("MAX_TEMPERATURE: " + MAX_TEMPERATURE);
                    var t = 0;
                    t = GetTemperature();
                    return t;
                }
                """;

            string f2 =
                """
                const int MAX_TEMPERATURE = 12 * 3;
                int v1 = MAX_TEMPERATURE;
                
                procedure int GetTemperature()
                {
                    var v2 = 100 * MAX_TEMPERATURE;
                    return v1 + v2;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()),
                new Tuple<string, string>("mysub.sbs", f2.ToString()));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var configVariable = files[0].ListElements().FirstOrDefault(p => p.Name == "MAX_TEMPERATURE");
            Assert.IsNotNull(configVariable);
            Assert.IsNotNull(configVariable.DataType);
            Assert.IsTrue(configVariable.DataType == TypeReference.TypeInt64);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "Test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6060L, (Int64)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.Test - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - MAX_TEMPERATURE: 60");
            log.ExpectNext("2 - Pre - 8 mysub.GetTemperature - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ConstVariableForProcedureReference()
        {
            string f1 =
                """
                using "mysub.sbs";
                procedure void MyHarnessSetup() : HarnessSetupMethod
                {
                    log("MyHarnessSetup");
                }
                
                override const HarnessSetupMethod HARNESS_SETUP = MyHarnessSetup;
                """;

            string f2 =
                """
                procedure void HarnessSetupMethod();
                procedure void NoSpecificHarnessSetup() : HarnessSetupMethod
                {
                    log("NoSpecificHarnessSetup");
                }
                
                public const HarnessSetupMethod HARNESS_SETUP = NoSpecificHarnessSetup;
                
                public void Setup()
                {
                    log("The General Setup");
                    HARNESS_SETUP();
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()),
                new Tuple<string, string>("mysub.sbs", f2.ToString()));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var procedure = files[1].ListElements().FirstOrDefault(p => p.Name == "Setup") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            taskContext.CallProcedure(procedure);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - mysub.Setup - <no arguments>");
            log.ExpectNext("2 - Normal - 11 Log - The General Setup");
            log.ExpectNext("2 - Pre - 12 myfile.MyHarnessSetup - <no arguments>");
            log.ExpectNext("3 - Normal - 4 Log - MyHarnessSetup");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}
