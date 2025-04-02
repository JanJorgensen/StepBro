using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest;
using System.Text;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestBinaryDataHandling
    {
        [TestMethod]
        public void TestLatin1Conversion()
        {
            var binaryData = "01 22 33 51 52 53 54 55 56 57 58 DD EE 0D".FromHexStringToByteArray();

            var textData = binaryData.ToLatin1();
            Assert.AreEqual("\u0001\u0022\u0033QRSTUVWX\u00DD\u00EE\r", textData);

            var createdBytes = textData.FromLatin1();
            Assert.AreEqual(14, createdBytes.Length);
            Assert.AreEqual(binaryData, createdBytes);
        }
    }
}
