using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;

using FET = StepBro.Core.ScriptData.FileElementType;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestFileTypeScan
    {
        [TestMethod]
        public void TestFileScanSimpleProc()
        {
            var result = FileBuilder.TypeScanFile("namespace Anders; void ProcA(){}");
            var top = result.TopElement;
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Anders", top.Name);

            Assert.AreEqual(1, top.Childs.Count);
            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[0].Type);
            Assert.AreEqual("ProcA", top.Childs[0].Name);
            Assert.AreEqual(0, top.Childs[0].Parameters.Count);
            Assert.AreEqual("void", top.Childs[0].ReturnType);
        }

        [TestMethod]
        public void TestFileScanPrivateSimpleProc()
        {
            var result = FileBuilder.TypeScanFile("namespace Bent; private void ProcB(){}");
            var top = result.TopElement;
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Bent", top.Name);

            Assert.AreEqual(1, top.Childs.Count);
            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[0].Type);
            Assert.AreEqual("ProcB", top.Childs[0].Name);
            Assert.AreEqual(0, top.Childs[0].Parameters.Count);
            Assert.AreEqual("void", top.Childs[0].ReturnType);
            Assert.IsNotNull(top.Childs[0].Modifiers);
            Assert.AreEqual(1, top.Childs[0].Modifiers.Count);
            Assert.AreEqual("private", top.Childs[0].Modifiers[0]);
        }

        [TestMethod]
        public void TestFileScanProcWithTypes()
        {
            var result = FileBuilder.TypeScanFile("namespace Anders; bool ProcA(int a){}");
            var top = result.TopElement;
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Anders", top.Name);

            Assert.AreEqual(1, top.Childs.Count);
            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[0].Type);
            Assert.AreEqual("ProcA", top.Childs[0].Name);
            Assert.AreEqual("bool", top.Childs[0].ReturnType);
            Assert.AreEqual(1, top.Childs[0].Parameters.Count);
            Assert.AreEqual("a", top.Childs[0].Parameters[0].Name);
            Assert.AreEqual("int", top.Childs[0].Parameters[0].TypeName);
        }

        [TestMethod]
        public void TestFileScanSomeSimpleEmptyProcs()
        {
            var result = FileBuilder.TypeScanFile(
                "namespace Christian; public string ProcC1(int a){} timespan ProcC2(decimal b){} private verdict ProcC3(bool c){} ");
            var top = result.TopElement;
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Christian", top.Name);

            Assert.AreEqual(3, top.Childs.Count);

            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[0].Type);
            Assert.AreEqual("ProcC1", top.Childs[0].Name);
            Assert.AreEqual(1, top.Childs[0].Parameters.Count);
            Assert.AreEqual("a", top.Childs[0].Parameters[0].Name);
            Assert.AreEqual("int", top.Childs[0].Parameters[0].TypeName);
            Assert.AreEqual("string", top.Childs[0].ReturnType);
            Assert.IsNotNull(top.Childs[0].Modifiers);
            Assert.AreEqual(1, top.Childs[0].Modifiers.Count);
            Assert.AreEqual("public", top.Childs[0].Modifiers[0]);

            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[1].Type);
            Assert.AreEqual("ProcC2", top.Childs[1].Name);
            Assert.AreEqual(1, top.Childs[1].Parameters.Count);
            Assert.AreEqual("b", top.Childs[1].Parameters[0].Name);
            Assert.AreEqual("decimal", top.Childs[1].Parameters[0].TypeName);
            Assert.AreEqual("timespan", top.Childs[1].ReturnType);
            Assert.IsNull(top.Childs[1].Modifiers);

            Assert.AreEqual(FET.ProcedureDeclaration, top.Childs[2].Type);
            Assert.AreEqual("ProcC3", top.Childs[2].Name);
            Assert.AreEqual(1, top.Childs[2].Parameters.Count);
            Assert.AreEqual("c", top.Childs[2].Parameters[0].Name);
            Assert.AreEqual("bool", top.Childs[2].Parameters[0].TypeName);
            Assert.AreEqual("verdict", top.Childs[2].ReturnType);
            Assert.IsNotNull(top.Childs[2].Modifiers);
            Assert.AreEqual(1, top.Childs[2].Modifiers.Count);
            Assert.AreEqual("private", top.Childs[2].Modifiers[0]);
        }

        [TestMethod]
        public void TestFileScanSimpleNamespaceSimpleVars()
        {
            var result = FileBuilder.TypeScanFile(
                "namespace Anders; int a = 176; string b = \"Hi\"; public bool c = false; timespan d = 6s; private decimal e = 2.6m;");
            var top = result.TopElement;
            Assert.IsNotNull(result);
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Anders", top.Name);

            Assert.AreEqual(5, top.Childs.Count);
            var varRef = top.Childs[0];
            Assert.AreEqual(FET.FileVariable, varRef.Type);
            Assert.AreEqual("a", varRef.Name);
            Assert.AreEqual("int", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, varRef.Type);
            Assert.AreEqual("b", varRef.Name);
            Assert.AreEqual("string", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, varRef.Type);
            Assert.AreEqual("c", varRef.Name);
            Assert.AreEqual("bool", varRef.ReturnType);
            Assert.AreEqual("public", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, top.Childs[3].Type);
            Assert.AreEqual("d", varRef.Name);
            Assert.AreEqual("timespan", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, varRef.Type);
            Assert.AreEqual("e", varRef.Name);
            Assert.AreEqual("decimal", varRef.ReturnType);
            Assert.AreEqual("private", varRef.ModifiersString());
        }

        [TestMethod]
        public void TestFileScanMixedProcsAndVars()
        {
            var result = FileBuilder.TypeScanFile(
                "namespace Christian; public int Pa(){} int a = 176; private string Pb(){} private string b = jdh; " +
                "decimal Pc(){} public bool c = false; verdict Pd(){} timespan d = 6s; bool Pe(){} private decimal e = 2.6m;");
            var top = result.TopElement;
            Assert.IsNotNull(result);
            Assert.AreEqual(FET.Namespace, top.Type);
            Assert.AreEqual("Christian", top.Name);

            Assert.AreEqual(10, top.Childs.Count);
            var varRef = top.Childs[0];
            Assert.AreEqual(FET.ProcedureDeclaration, varRef.Type);
            Assert.AreEqual("Pa", varRef.Name);
            Assert.AreEqual("int", varRef.ReturnType);
            Assert.AreEqual("public", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, varRef.Type);
            Assert.AreEqual("a", varRef.Name);
            Assert.AreEqual("int", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.ProcedureDeclaration, varRef.Type);
            Assert.AreEqual("Pb", varRef.Name);
            Assert.AreEqual("string", varRef.ReturnType);
            Assert.AreEqual("private", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, top.Childs[3].Type);
            Assert.AreEqual("b", varRef.Name);
            Assert.AreEqual("string", varRef.ReturnType);
            Assert.AreEqual("private", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.ProcedureDeclaration, varRef.Type);
            Assert.AreEqual("Pc", varRef.Name);
            Assert.AreEqual("decimal", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, top.Childs[3].Type);
            Assert.AreEqual("c", varRef.Name);
            Assert.AreEqual("bool", varRef.ReturnType);
            Assert.AreEqual("public", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.ProcedureDeclaration, varRef.Type);
            Assert.AreEqual("Pd", varRef.Name);
            Assert.AreEqual("verdict", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, top.Childs[3].Type);
            Assert.AreEqual("d", varRef.Name);
            Assert.AreEqual("timespan", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.ProcedureDeclaration, varRef.Type);
            Assert.AreEqual("Pe", varRef.Name);
            Assert.AreEqual("bool", varRef.ReturnType);
            Assert.AreEqual("", varRef.ModifiersString());

            varRef = varRef.Next();
            Assert.AreEqual(FET.FileVariable, top.Childs[3].Type);
            Assert.AreEqual("e", varRef.Name);
            Assert.AreEqual("decimal", varRef.ReturnType);
            Assert.AreEqual("private", varRef.ModifiersString());
        }

        [TestMethod]
        public void TestFileScanUsings()
        {
            var result = FileBuilder.TypeScanFile("using Andrea; using Betty.Chrissy; using \"Denise.sbs\"; public using Erica; namespace Uncle; void ProcA(){}");
            Assert.AreEqual(FET.Namespace, result.TopElement.Type);
            Assert.AreEqual("Uncle", result.TopElement.Name);

            Assert.AreEqual(4, result.ListUsings().Count());
            Assert.AreEqual("i", result.ListUsings().ElementAt(0).Type);
            Assert.AreEqual("Andrea", result.ListUsings().ElementAt(0).Name);
            Assert.AreEqual("i", result.ListUsings().ElementAt(1).Type);
            Assert.AreEqual("Betty.Chrissy", result.ListUsings().ElementAt(1).Name);
            Assert.AreEqual("p", result.ListUsings().ElementAt(2).Type);
            Assert.AreEqual("Denise.sbs", result.ListUsings().ElementAt(2).Name);
            Assert.AreEqual("I", result.ListUsings().ElementAt(3).Type);
            Assert.AreEqual("Erica", result.ListUsings().ElementAt(3).Name);

            Assert.AreEqual(1, result.TopElement.Childs.Count);
            Assert.AreEqual(FET.ProcedureDeclaration, result.TopElement.Childs[0].Type);
            Assert.AreEqual("ProcA", result.TopElement.Childs[0].Name);
            Assert.AreEqual(0, result.TopElement.Childs[0].Parameters.Count);
            Assert.AreEqual("void", result.TopElement.Childs[0].ReturnType);
        }

        [TestMethod]
        public void TestFileScanTypedefs()
        {
            // Typedef for .net generic type.
            var result = FileBuilder.TypeScanFile("type MyType : TheBaseType;");
            Assert.AreEqual(FET.Namespace, result.TopElement.Type);
            Assert.AreEqual("Angus", result.TopElement.Name);

            var top = result.TopElement;
            Assert.AreEqual(1, top.Childs.Count);

            var child = top.Childs[0];
            Assert.AreEqual("MyType", child.Name);
            Assert.AreEqual("TheBaseType", child.DataType.GetGenericType().Item1);


            // Typedef for .net generic type.
            result = FileBuilder.TypeScanFile("type StringList : List<string>;");
            Assert.AreEqual(FET.Namespace, result.TopElement.Type);
            Assert.AreEqual("Angus", result.TopElement.Name);

            top = result.TopElement;
            Assert.AreEqual(1, top.Childs.Count);

            child = top.Childs[0];
            Assert.AreEqual(FET.TypeDef, child.Type);
            Assert.AreEqual("StringList", child.Name);
            Assert.AreEqual("List", child.DataType.GetGenericType().Item1);
            Assert.AreEqual(1, child.DataType.ParameterCount);
            Assert.AreEqual("string", child.DataType.GetParameter(0).GetGenericType().Item1);


        }
    }
}
