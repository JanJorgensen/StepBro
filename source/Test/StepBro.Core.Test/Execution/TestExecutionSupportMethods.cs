using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestExecutionSupportMethods
    {
        [TestMethod]
        public void TestThisReference()
        {
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestThisReferenceProcedureName()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   return this.Name;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("MyProcedure", (string)result);
        }


        [TestMethod]
        public void TestSetResult()
        {
            var f = new StringBuilder();
            f.AppendLine("void MyProc() {");
            f.AppendLine("   this.SetResult(fail, \"Something very wrong!!\");");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProc");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Fail, taskContext.Result.Verdict);
            Assert.AreEqual("Something very wrong!!", taskContext.Result.Description);
        }

        [TestMethod]
        public void TestHexStringToByteArray()
        {
            var f = """
                procedure void TestEncoding()
                {
                    var source = "01 22 33 51 52 53 54 55 56 57 58 DD EE 0D";
                    var binary = source.FromHexStringToByteArray();
                    var textual = binary.ToHexString(" ", 3);
                    expect (textual == "51 52 53 54 55 56 57 58 DD EE 0D");
                }
            """;
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(0, file.Errors.ErrorCount);
            var procedure = file.GetFileElement<IFileProcedure>("TestEncoding");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Pass, taskContext.Result.Verdict);
        }

        [TestMethod]
        public void TestByteStuffingEncoderSimple()
        {
            var f = """
                StepBro.Core.Data.ByteStuffingEncoder byteStuffing = StepBro.Core.Data.ByteStuffingEncoder(0xF8, 0x00, 0xF4, 0x0D, 0xF6, 0xF8, 0xF8);
                StepBro.Core.Data.BinaryEncoding binaryEncoding = StepBro.Core.Data.BinaryEncoding(StepBro.Core.Data.BinaryEncoding.Endianness.LittleEndian);

                procedure void TestEncoding()
                {
                    var data1 = "00 01 02 03 0A 0B 0C 0D 0E 0F 10 11 F7 F8 F9 FA".FromHexStringToByteArray();
                    var encoded = byteStuffing.Encode(data1);
                    var encodedHex = encoded.ToHexString(" ");
                    expect (encodedHex == "F8 F4 01 02 03 0A 0B 0C F8 F6 0E 0F 10 11 F7 F8 F8 F9 FA");

                    var data2 = "F8 F4 01 02 03 0A 0B 0C F8 F6 0E 0F 10 11 F7 F8 F8 F9 FA".FromHexStringToByteArray();
                    var decoded = byteStuffing.Decode(data2);
                    var decodedHex = decoded.ToHexString(" ");
                    expect (decodedHex == "00 01 02 03 0A 0B 0C 0D 0E 0F 10 11 F7 F8 F9 FA");
                }
            """;
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(0, file.Errors.ErrorCount);
            var procedure = file.GetFileElement<IFileProcedure>("TestEncoding");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Pass, taskContext.Result.Verdict);
        }

        [TestMethod]
        public void TestByteStuffingEncoderPartly()
        {
            var f = """
                StepBro.Core.Data.ByteStuffingEncoder byteStuffing = StepBro.Core.Data.ByteStuffingEncoder(0xF8, 0x00, 0xF4, 0x0D, 0xF6, 0xF8, 0xF8);
                StepBro.Core.Data.BinaryEncoding binaryEncoding = StepBro.Core.Data.BinaryEncoding(StepBro.Core.Data.BinaryEncoding.Endianness.LittleEndian);

                procedure void TestEncoding()
                {
                    var data1 = "00 F8 02 03 0A 0B 0C 0D 0E 0F 10 11 F7 F8 F9 FA".FromHexStringToByteArray();
                    var encoded = byteStuffing.Encode(data1, 0, 3, -1, -1);
                    var encodedHex = encoded.ToHexString(" ");
                    expect (encodedHex == "00 F8 02 03 0A 0B 0C F8 F6 0E 0F 10 11 F7 F8 F8 F9 FA");
                }
            """;
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(0, file.Errors.ErrorCount);
            var procedure = file.GetFileElement<IFileProcedure>("TestEncoding");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Pass, taskContext.Result.Verdict);
        }

        [TestMethod]
        public void TestBinaryEncoding()
        {
            var f = """
                StepBro.Core.Data.BinaryEncoding binaryEncoding = StepBro.Core.Data.BinaryEncoding(StepBro.Core.Data.BinaryEncoding.Endianness.LittleEndian);
                procedure string TestEncoding()
                {
                    var data = "22 33 51 52 53 54 55 56 57 58 DD EE".FromHexStringToByteArray();
                    var decoder = StepBro.Core.Data.BinaryDecoder(binaryEncoding, data);
                    int messageCounter = decoder.ReadUInt16();
                    var payload = decoder.GetBlock(decoder.SizeLeft - 2);
                    int crc = decoder.ReadUInt16();
                    expect (messageCounter == 0x3322);
                    expect (payload.Length == 8);
                    expect (crc == 0xEEDD);
                }
            """;
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(0, file.Errors.ErrorCount);
            var procedure = file.GetFileElement<IFileProcedure>("TestEncoding");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Pass, taskContext.Result.Verdict);
        }
    }
}
