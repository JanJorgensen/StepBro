using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.TestInterface;
using System.Linq;

namespace StepBroModulesTest
{
    [TestClass]
    public class SerialTestInterfaceTest
    {
        [TestMethod]
        public void DecodeCommandListEntry()
        {
            int id;
            Assert.AreEqual("henry", SerialTestConnection.DecodeCommandListLine("20 - henry", out id));
            Assert.AreEqual(20, id);

            Assert.AreEqual("absolut", SerialTestConnection.DecodeCommandListLine("-1 - absolut", out id));
            Assert.AreEqual(-1, id);

            Assert.AreEqual("cornetto", SerialTestConnection.DecodeCommandListLine(" - cornetto", out id));
            Assert.AreEqual(-1, id);

            Assert.AreEqual("fish", SerialTestConnection.DecodeCommandListLine("fish", out id));
            Assert.AreEqual(-1, id);
        }

        [TestMethod]
        public void DecodeRemoteProcedureInfo_Int()
        {
            var lines = new string[] {
            "Command 42: anders",
            "Help: Just a prototype command to be deleted.",
            "Parameters: bool input. Return type: int"};
            var procInfo = SerialTestConnection.DecodeRemoteProcedureInfo("anders", 42, lines.ToList());
            Assert.IsNull(procInfo.Error);
            Assert.AreEqual("anders", procInfo.Name);
            Assert.AreEqual(42, procInfo.ID);
            Assert.AreEqual("Just a prototype command to be deleted.", procInfo.Description);
            Assert.AreEqual(typeof(long), procInfo.ReturnType);
            Assert.AreEqual(1, procInfo.Parameters.Count());
            Assert.AreEqual("input", procInfo.Parameters.ElementAt(0).Name);
            Assert.AreEqual(typeof(bool), procInfo.Parameters.ElementAt(0).Type);
        }

        [TestMethod]
        public void DecodeRemoteProcedureInfo_ReturnOnly()
        {
            var lines = new string[] {
            "Command: bent",
            "Return type: float"};
            var procInfo = SerialTestConnection.DecodeRemoteProcedureInfo("bent", -1, lines.ToList());
            Assert.IsNull(procInfo.Error);
            Assert.AreEqual("bent", procInfo.Name);
            Assert.AreEqual(-1, procInfo.ID);
            Assert.IsNull(procInfo.Description);
            Assert.AreEqual(typeof(double), procInfo.ReturnType);
            Assert.AreEqual(0, procInfo.Parameters.Count());
        }

        [TestMethod]
        public void DecodeRemoteProcedureInfo_StringInt_Bool()
        {
            var lines = new string[] {
            "Command 9: chris",
            "Parameters: string name, int age. Return type: bool"};
            var procInfo = SerialTestConnection.DecodeRemoteProcedureInfo("chris", 9, lines.ToList());
            Assert.IsNull(procInfo.Error);
            Assert.AreEqual("chris", procInfo.Name);
            Assert.AreEqual(9, procInfo.ID);
            Assert.IsNull(procInfo.Description);
            Assert.AreEqual(typeof(bool), procInfo.ReturnType);
            Assert.AreEqual(2, procInfo.Parameters.Count());
            Assert.AreEqual("name", procInfo.Parameters.ElementAt(0).Name);
            Assert.AreEqual(typeof(string), procInfo.Parameters.ElementAt(0).Type);
            Assert.AreEqual("age", procInfo.Parameters.ElementAt(1).Name);
            Assert.AreEqual(typeof(long), procInfo.Parameters.ElementAt(1).Type);
        }

        [TestMethod]
        public void DecodeRemoteProcedureInfo_OneUntypedPar()
        {
            var lines = new string[] {
            "Command 121: denise",
            "Parameters: channel. Return type: int"};
            var procInfo = SerialTestConnection.DecodeRemoteProcedureInfo("denise", 121, lines.ToList());
            Assert.IsNull(procInfo.Error);
            Assert.AreEqual("denise", procInfo.Name);
            Assert.AreEqual(121, procInfo.ID);
            Assert.IsNull(procInfo.Description);
            Assert.AreEqual(typeof(long), procInfo.ReturnType);
            Assert.AreEqual(1, procInfo.Parameters.Count());
            Assert.AreEqual("channel", procInfo.Parameters.ElementAt(0).Name);
            Assert.AreEqual(typeof(object), procInfo.Parameters.ElementAt(0).Type);
        }

        [TestMethod]
        public void DecodeRemoteProcedureInfo_UntypedPars()
        {
            var lines = new string[] {
            "Command 31: emmerson",
            "Parameters: anton, string bent, chric, dennis, int eric. Return type: bool"};
            var procInfo = SerialTestConnection.DecodeRemoteProcedureInfo("emmerson", 31, lines.ToList());
            Assert.IsNull(procInfo.Error);
            Assert.AreEqual("emmerson", procInfo.Name);
            Assert.AreEqual(31, procInfo.ID);
            Assert.IsNull(procInfo.Description);
            Assert.AreEqual(typeof(bool), procInfo.ReturnType);
            Assert.AreEqual(5, procInfo.Parameters.Count());
            Assert.AreEqual("anton", procInfo.Parameters.ElementAt(0).Name);
            Assert.AreEqual(typeof(object), procInfo.Parameters.ElementAt(0).Type);
            Assert.AreEqual("bent", procInfo.Parameters.ElementAt(1).Name);
            Assert.AreEqual(typeof(string), procInfo.Parameters.ElementAt(1).Type);
            Assert.AreEqual("chric", procInfo.Parameters.ElementAt(2).Name);
            Assert.AreEqual(typeof(object), procInfo.Parameters.ElementAt(2).Type);
            Assert.AreEqual("dennis", procInfo.Parameters.ElementAt(3).Name);
            Assert.AreEqual(typeof(object), procInfo.Parameters.ElementAt(3).Type);
            Assert.AreEqual("eric", procInfo.Parameters.ElementAt(4).Name);
            Assert.AreEqual(typeof(long), procInfo.Parameters.ElementAt(4).Type);
        }
    }
}
