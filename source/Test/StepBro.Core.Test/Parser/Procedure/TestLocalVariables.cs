using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestLocalVariables
    {
        [TestMethod]
        public void TestSimpleSpecifiedTypeVariables()
        {
            Assert.AreEqual(728L, ParseAndRun<long>("myVar", "int myVar = 728;"));
            Assert.AreEqual(13.43, ParseAndRun<double>("myVar", "decimal myVar = 13.43;"));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "bool myVar = true;"));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "string myVar = \"Snappy\";"));
            Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myVar", "verdict myVar = inconclusive;"));
        }

        [TestMethod]
        public void TestSimpleSpecifiedTypeVariablesWithConversion()
        {
            Assert.AreEqual(18.0, ParseAndRun<double>("myVar", "decimal myVar = 18;"));
        }

        [TestMethod]
        public void TestSimpleConstantVarTypeVariables()
        {
            Assert.AreEqual(728L, ParseAndRun<long>("myVar", "var myVar = 728;"));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "var myVar = true;"));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "var myVar = \"Snappy\";"));
            Assert.AreEqual(13.43, ParseAndRun<double>("myVar", "var myVar = 13.43;"));
            Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myVar", "var myVar = inconclusive;"));
        }

        [TestMethod]
        public void TestSimpleSpecifiedArrayTypeVariables()
        {
            Assert.AreEqual(26L, ParseAndRun<long>("myArr[1]", "int[] myArr = [25, 26, 27];", false));
            Assert.AreEqual(false, ParseAndRun<bool>("myArr[2]", "bool[] myArr = [true, false, false, true];", false));
            Assert.AreEqual(4.8, ParseAndRun<double>("myArr[3]", "decimal[] myArr = [1.2, 2.4, 3.6, 4.8, 5.0];", false));
            Assert.AreEqual("Bent", ParseAndRun<string>("myArr[1]", "string[] myArr = [\"Anders\", \"Bent\", \"Christian\"];", false));
            Assert.AreEqual(Verdict.Pass, ParseAndRun<Verdict>("myArr[1]", "verdict[] myArr = [unset, pass, fail];", true));

            Assert.AreEqual(TimeSpan.FromMilliseconds(2300), ParseAndRun<TimeSpan>("myArr[2]", "timespan[] myArr = [1s, 25ms, 2.3s, 87ms];", true));

            Assert.AreEqual(new DateTime(2017, 2, 25, 12, 30, 10),
                ParseAndRun<DateTime>(
                    "myArr[1]",
                    "datetime[] myArr = [@2016-03-07 12:30:00, @2017-02-25 12:30:10, @2015-07-20 12:30:00];",
                    true));
        }

        [TestMethod]
        public void TestSimpleVarAssignment()
        {
            Assert.AreEqual(727L, ParseAndRun<long>("myVar", "var myVar = 0; myVar = 727;", false));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "var myVar = false; myVar = true;", false));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "var myVar = \"\"; myVar = \"Snappy\";", false));
            Assert.AreEqual(13.45, ParseAndRun<double>("myVar", "var myVar = 0.0; myVar = 13.45;", false));
            Assert.AreEqual(Verdict.Error, ParseAndRun<Verdict>("myVar", "var myVar = unset; myVar = error;", false));
        }


        [TestMethod]
        public void TestClassTypeVariables()
        {
            Assert.AreEqual(8822L, ParseAndRun<long>("obj.PropInt", "DummyClass obj; obj.PropInt = 8822;", false, true));
            //Assert.AreEqual(8822L, ParseAndRun<long>("obj.PropInt", "var obj = DummyClass(); obj.PropInt = 8822;", false, true));     // TODO !!
        }

        [TestMethod]
        public void TestVarReAssignmentError01()
        {
            var result = Parse<long>("0", "int per = 0;\r\n bool per = true;", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(3, result.Errors[0].Line);
        }

        [TestMethod]
        public void TestVarReAssignmentError02()
        {
            var result = Parse<long>("0", "int per = 0;\r\n bool per = true;\r\n string per = \"test\";", varGeneration: false);
            Assert.AreEqual(2, result.Errors.ErrorCount);
            Assert.AreEqual(3, result.Errors[0].Line);
            Assert.AreEqual(4, result.Errors[1].Line);
        }

        [TestMethod]
        public void TestVarReAssignmentError03()
        {
            var proc = FileBuilder.ParseProcedure(
                """
                int Func()
                {
                    int per = 0;
                    bool per = true;
                    return 0;
                }
                """);

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            Assert.AreEqual(1, FileBuilder.LastInstance.Errors.ErrorCount);
            Assert.AreEqual(4, FileBuilder.LastInstance.Errors[0].Line);
        }

        [TestMethod]
        public void TestVarReDeclarationError01()
        {
            var proc = FileBuilder.ParseProcedure(
                """
                int Func()
                {
                    int per = 0;
                    bool per;
                    return 0;
                }
                """);

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            Assert.AreEqual(1, FileBuilder.LastInstance.Errors.ErrorCount);
            Assert.AreEqual(4, FileBuilder.LastInstance.Errors[0].Line);
        }

        [TestMethod]
        public void TestVarReDeclarationNoError01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int a = 5;
                    if (a == 5)
                    {
                        int per = 7;
                        a = per;
                    }
                    else
                    {
                        bool per = true;
                        if (per == true)
                        {
                            a = 2;
                        }
                        else
                        {
                            a = 0;
                        }
                    }
                    return a;
                }
                """);

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result.GetType() == typeof(long));
            Assert.AreEqual(7, (long)result);
        }

        [TestMethod]
        public void TestVarReDeclarationNoError02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int a = 4;
                    if (a == 5)
                    {
                        int per = 7;
                        a = per;
                    }
                    else
                    {
                        bool per = true;
                        if (per == true)
                        {
                            a = 2;
                        }
                        else
                        {
                            a = 0;
                        }
                    }
                    return a;
                }
                """);

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result.GetType() == typeof(long));
            Assert.AreEqual(2, (long)result);
        }

        [TestMethod]
        public void TestUseConstructorTypeVariables()
        {
            Assert.AreEqual("test", ParseAndRun<string>("myVar.ToString()", "var myVar = System.Text.StringBuilder(); myVar.Append(\"test\");"));
        }
    }
}
