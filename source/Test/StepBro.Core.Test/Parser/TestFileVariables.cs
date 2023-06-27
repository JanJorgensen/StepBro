using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Api;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestFileVariables
    {
        [TestMethod]
        public void ParseSimpleFileVariables()
        {
            var var = FileBuilder.ParseFileVariable<long>(null, null, "protected int myVariable = 10 * 3;");
            Assert.IsNotNull(var);
            Assert.AreEqual("myVariable", var.Name);
            Assert.ReferenceEquals(var.DataType, TypeReference.TypeInt64);
            Assert.IsFalse(var.IsReadonly);
            Assert.AreEqual(AccessModifier.Protected, var.AccessProtection);
        }

        /// <summary>
        /// Testing use of variables in current file scope.
        /// </summary>
        [TestMethod]
        public void TestFileWithTypedSimpleVariables()
        {
            var procedure = this.CreateTestFile("return i2;");

            var taskContext = ExecutionHelper.ExeContext();

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(20, (long)result);
        }

        [TestMethod]
        public void ParseFileVariableWithEmptyPropertyBlockNoArgs()
        {
            var var = FileBuilder.ParseFileVariable<DummyInstrumentClass>(
                null, new Type[] { typeof(DummyInstrumentClass) },
                "private DummyInstrumentClass myVariable = DummyInstrumentClass{ }");
            Assert.IsNotNull(var);
            Assert.AreEqual("myVariable", var.Name);
            Assert.ReferenceEquals(var.DataType, (TypeReference)typeof(DummyInstrumentClass));
            Assert.IsTrue(var.IsReadonly);
            Assert.AreEqual(AccessModifier.Private, var.AccessProtection);
        }

        [TestMethod]
        public void ParseFileVariableWithEmptyPropertyBlockEmptyArgs()
        {
            var var = FileBuilder.ParseFileVariable<DummyInstrumentClass>(
                null, new Type[] { typeof(DummyInstrumentClass) },
                "private DummyInstrumentClass myVariable = DummyInstrumentClass(){ }");
            Assert.IsNotNull(var);
            Assert.AreEqual("myVariable", var.Name);
            Assert.ReferenceEquals(var.DataType, (TypeReference)typeof(DummyInstrumentClass));
            Assert.IsTrue(var.IsReadonly);
            Assert.AreEqual(AccessModifier.Private, var.AccessProtection);
        }

        [TestMethod]
        public void TestFileVariableWithEmptyPropertyBlock()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name + "{}");
            f.AppendLine("procedure int Test1()");
            f.AppendLine("{");
            f.AppendLine("   myTool.BoolA = true;");
            f.AppendLine("   myTool.IntA = 44;");
            f.AppendLine("   return myTool.Fcn(\"Janse\", false);");
            f.AppendLine("}");
            f.AppendLine("procedure int Test2()");
            f.AppendLine("{");
            f.AppendLine("   myTool.BoolA = true;");
            f.AppendLine("   myTool.IntA = 44;");
            f.AppendLine("   return myTool.Fcn(\"Janse\", true);");
            f.AppendLine("}");
            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f.ToString()));

            Assert.AreEqual("ObjectUsing", files[0].Namespace);

            var variable = files[0].ListElements().First(p => p.Name == "myTool") as FileVariable;
            Assert.IsNotNull(variable);
            var variableObject = variable.VariableOwnerAccess.Container.GetValue() as INameable;
            Assert.IsNotNull(variableObject);
            Assert.AreEqual("myTool", variableObject.Name);

            var procedure = files[0].ListElements().First(p => p.Name == "Test1") as IFileProcedure;
            Assert.AreEqual("Test1", procedure.Name);
            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(44L, result);

            procedure = files[0].ListElements().First(p => p.Name == "Test2") as IFileProcedure;
            Assert.AreEqual("Test2", procedure.Name);
            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(77L, result);
        }

        [TestMethod, Ignore]
        public void TestFileVariableWithArgsAndEmptyPropertyBlock()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name + "(400){}");
            f.AppendLine("procedure int Test1()");
            f.AppendLine("{");
            f.AppendLine("   myTool.BoolA = true;");
            f.AppendLine("   return myTool.Fcn(\"Janse\", false);");
            f.AppendLine("}");
            f.AppendLine("procedure int Test2()");
            f.AppendLine("{");
            f.AppendLine("   myTool.BoolA = true;");
            f.AppendLine("   return myTool.Fcn(\"Janse\", true);");
            f.AppendLine("}");
            var file = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f.ToString()))[0];

            Assert.AreEqual("ObjectUsing", file.Namespace);

            var variable = file.ListElements().First(p => p.Name == "myTool") as FileVariable;
            Assert.IsNotNull(variable);
            var variableObject = variable.VariableOwnerAccess.Container.GetValue() as INameable;
            Assert.IsNotNull(variableObject);
            Assert.AreEqual("myTool", variableObject.Name);

            var procedure = file.ListElements().First(p => p.Name == "Test1") as IFileProcedure;
            Assert.AreEqual("Test1", procedure.Name);
            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(44L, result);

            procedure = file.ListElements().First(p => p.Name == "Test2") as IFileProcedure;
            Assert.AreEqual("Test2", procedure.Name);
            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(77L, result);
        }

        [TestMethod]
        public void TestSpecialFileVariableWithPropblockConfig()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f.AppendLine("{");
            f.AppendLine("   BoolA: true,");
            f.AppendLine("   IntA:  19");
            //f.AppendLine("   Names: [\"Anders\", \"Bent\", \"Chris\"]");
            f.AppendLine("}");
            f.AppendLine("procedure int UseObject()");
            f.AppendLine("{");
            f.AppendLine("   return myTool.Fcn(\"Janse\", false);");
            f.AppendLine("}");
            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f.ToString()));
            Assert.AreEqual("ObjectUsing", files[0].Namespace);
            var procedure = files[0].ListElements().First(p => p.Name == "UseObject") as IFileProcedure;
            Assert.AreEqual("UseObject", procedure.Name);

            var variable = files[0].ListElements().First(p => p.Name == "myTool") as FileVariable;
            Assert.IsNotNull(variable);
            var variableObject = variable.VariableOwnerAccess.Container.GetValue() as INameable;
            Assert.IsNotNull(variableObject);
            Assert.AreEqual("myTool", variableObject.Name);

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(19L, result);
        }

        internal IFileProcedure CreateTestFile(string returnStatement)
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("bool b1 = true;");
            f.AppendLine("private bool b2 = false;");
            f.AppendLine("public bool b3 = true;");
            f.AppendLine("public bool b4;");
            f.AppendLine();
            f.AppendLine("int i1 = 10;");
            f.AppendLine("private int i2 = 20;");
            f.AppendLine("public int i3 = 30;");
            f.AppendLine("public int i4;");
            f.AppendLine();
            f.AppendLine("decimal f1 = 10.0;");
            f.AppendLine("private decimal f2 = 20.2;");
            f.AppendLine("public decimal f3 = 30.4;");
            f.AppendLine("double f4 = 40.6;");
            f.AppendLine("private double f5 = 50.8;");
            f.AppendLine("public double f6 = 60.2;");
            f.AppendLine("public double f7;");
            f.AppendLine();
            f.AppendLine("string s1 = \"\";");
            f.AppendLine("private string s2 = \"Hey\";");
            f.AppendLine("public string s3 = \"a line\\n\";");
            f.AppendLine("public string s4;");
            f.AppendLine();
            f.AppendLine("int UseThoseFileVariables() {");
            f.AppendLine("   " + returnStatement);
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            file.InitializeFileVariables(null);
            Assert.AreEqual(19, file.ListFileVariables().Count());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "UseThoseFileVariables") as IFileProcedure;
            Assert.AreEqual("UseThoseFileVariables", procedure.Name);
            return procedure;
        }

    }

    public class DummyInstrumentClass : IDisposable, IResettable, INameable
    {
        public static long m_nextInstanceID = 10;

        private string m_objectName;
        private List<string> m_names = new List<string>();
        private long m_id;
        private long m_resetCounts = 0;

        public DummyInstrumentClass([ObjectName] string objectName = "<no name>")
        {
            m_id = m_nextInstanceID++;
            m_objectName = objectName;
        }

        public DummyInstrumentClass(string[] names, [ObjectName] string objectName = "<no name>") : this()
        {
            m_id = m_nextInstanceID++;
            m_objectName = objectName;
            m_names = names.ToList();
        }

        public DummyInstrumentClass(long valueA, [ObjectName] string objectName = "<no name>") : this()
        {
            m_id = m_nextInstanceID++;
            m_objectName = objectName;
            this.IntA = valueA;
        }

        public long ID { get { return m_id; } }
        public long ResetCounts { get { return m_resetCounts; } }
        public bool Disposed { get; private set; } = false;

        public bool BoolA { get; set; } = false;
        public long IntA { get; set; } = 0L;
        public List<string> Names { get { return m_names; } set { m_names = value; } }

        [ObjectName] 
        public string Name { get { return m_objectName; } set { m_objectName = value; } }

        public void Dispose()
        {
            m_names.Clear();
            this.Disposed = true;
        }

        public long Fcn(string s, bool b)
        {
            if (b) return s.Length + (this.BoolA ? 72 : 81);
            else return this.IntA + m_names.Count * 1000L;
        }

        public bool Reset(ILogger logger)
        {
            m_resetCounts++;
            return true;
        }

        public void DoSomething()
        {
            this.IntA += 1000;
        }

        public void DoSomethingElse()
        {
            this.IntA /= 4;
        }
    }
}
