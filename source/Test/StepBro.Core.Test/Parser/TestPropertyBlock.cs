using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using System.Globalization;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestPropertyBlock
    {
        [TestMethod]
        public void TestPropertyBlockWithSingleValue()
        {
            var block = FileBuilder.ParsePropertyBlock("{ anders = \"Jens\" }");
            AssertBlockWithOneValue(block, "anders", "Jens");

            block = FileBuilder.ParsePropertyBlock("{ bent = 52 }");
            AssertBlockWithOneValue(block, "bent", 52L);

            block = FileBuilder.ParsePropertyBlock("{ christian = 0xA7 }");
            AssertBlockWithOneValue(block, "christian", 0xA7L);

            block = FileBuilder.ParsePropertyBlock("{ dennis = 3Bh }");
            AssertBlockWithOneValue(block, "dennis", 0x3BL);

            block = FileBuilder.ParsePropertyBlock("{ eric = error }");
            AssertBlockWithOneValue(block, "eric", Verdict.Error);

            block = FileBuilder.ParsePropertyBlock("{ flemming = 3.1417 }");
            AssertBlockWithOneValue(block, "flemming", 3.1417);

            block = FileBuilder.ParsePropertyBlock("{ gert = true }");
            AssertBlockWithOneValue(block, "gert", true);

            block = FileBuilder.ParsePropertyBlock("{ hubert = 14.5ms }");
            AssertBlockWithOneValue(block, "hubert", TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 145) / 10));

            block = FileBuilder.ParsePropertyBlock("{ \"Ivan Petersen\" = 40 }");
            AssertBlockWithOneValue(block, "Ivan Petersen", 40L);
        }

        [TestMethod]
        public void TestPropertyBlockWithDifferentAssignmentOperators()
        {
            var block = FileBuilder.ParsePropertyBlock("{ anders = 12 }");
            AssertBlockWithOneValue(block, "anders", 12L);
            block = FileBuilder.ParsePropertyBlock("{bent=23}");
            AssertBlockWithOneValue(block, "bent", 23L);

            block = FileBuilder.ParsePropertyBlock("{ christian : 34 }");
            AssertBlockWithOneValue(block, "christian", 34L);
            block = FileBuilder.ParsePropertyBlock("{dennis:45}");
            AssertBlockWithOneValue(block, "dennis", 45L);

            block = FileBuilder.ParsePropertyBlock("{ erik += 56 }");
            AssertBlockWithOneValue(block, "erik", 56L);
            block = FileBuilder.ParsePropertyBlock("{frank+=67}");
            AssertBlockWithOneValue(block, "frank", 67L);
        }

        [TestMethod]
        public void TestPropertyBlockTypedWithSingleValue()
        {
            var block = FileBuilder.ParsePropertyBlock("{ anden anders = \"Jens\" }");
            AssertBlockWithOneValue(block, "anden", "anders", "Jens");

            block = FileBuilder.ParsePropertyBlock("{ cykel fisken bent = 52 }");
            AssertBlockWithOneValue(block, "cykel fisken", "bent", 52L);

            block = FileBuilder.ParsePropertyBlock("{ svensker goblen christian = 0xA7 }");
            AssertBlockWithOneValue(block, "svensker goblen", "christian", 0xA7L);

            block = FileBuilder.ParsePropertyBlock("{ private int dennis = 3Bh }");
            AssertBlockWithOneValue(block, "private int", "dennis", 0x3BL);

            block = FileBuilder.ParsePropertyBlock("{ premature verdict eric = error }");
            AssertBlockWithOneValue(block, "premature verdict", "eric", Verdict.Error);

            block = FileBuilder.ParsePropertyBlock("{ int double flemming = 3.1417 }");
            AssertBlockWithOneValue(block, "int double", "flemming", 3.1417);

            block = FileBuilder.ParsePropertyBlock("{ override partner gert = true }");
            AssertBlockWithOneValue(block, "override partner", "gert", true);

            block = FileBuilder.ParsePropertyBlock("{ execution log hubert = 14.5ms }");
            AssertBlockWithOneValue(block, "execution log", "hubert", TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 145) / 10));
        }

        [TestMethod]
        public void TestPropertyBlockDoubleTypedWithSingleValue()
        {
            var block = FileBuilder.ParsePropertyBlock("{ anden anders antonsen = \"Jens\" }");
            AssertBlockWithOneValue(block, "anden anders", "antonsen", "Jens");

            block = FileBuilder.ParsePropertyBlock("{ fisken bent = 52 }");
            AssertBlockWithOneValue(block, "fisken", "bent", 52L);

            block = FileBuilder.ParsePropertyBlock("{ goblen christian = 0xA7 }");
            AssertBlockWithOneValue(block, "goblen", "christian", 0xA7L);

            block = FileBuilder.ParsePropertyBlock("{ int dennis = 3Bh }");
            AssertBlockWithOneValue(block, "int", "dennis", 0x3BL);

            block = FileBuilder.ParsePropertyBlock("{ verdict eric = error }");
            AssertBlockWithOneValue(block, "verdict", "eric", Verdict.Error);

            block = FileBuilder.ParsePropertyBlock("{ double flemming = 3.1417 }");
            AssertBlockWithOneValue(block, "double", "flemming", 3.1417);

            block = FileBuilder.ParsePropertyBlock("{ procedure gert = true }");
            AssertBlockWithOneValue(block, "procedure", "gert", true);

            block = FileBuilder.ParsePropertyBlock("{ log hubert = 14.5ms }");
            AssertBlockWithOneValue(block, "log", "hubert", TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 145) / 10));
        }

        [TestMethod]
        public void TestPropertyBlockSimpleNullValue()
        {
            var block = FileBuilder.ParsePropertyBlock("{ ftms = null }");
            Assert.AreEqual("{ ftms=<null> }", block.GetTestString());

            block = FileBuilder.ParsePropertyBlock("{ lgbt ftms = null }");
            Assert.AreEqual("{ lgbt ftms=<null> }", block.GetTestString());
        }

        [TestMethod]
        public void TestPropertyBlockWithMoreValues()
        {
            var block = FileBuilder.ParsePropertyBlock("{ anders = \"Jens\", berit = true, chris=23.7s }");
            Assert.AreEqual("{ anders=Jens, berit=True, chris=00:00:23.7000000 }", block.GetTestString());

            block = FileBuilder.ParsePropertyBlock("{ anders = \"Jens\", pigen berit = true, drengen chris=23.7s }");
            Assert.AreEqual("{ anders=Jens, pigen berit=True, drengen chris=00:00:23.7000000 }", block.GetTestString());
        }

        [TestMethod]
        public void TestPropertyBlockNested()
        {
            var block = FileBuilder.ParsePropertyBlock("{ abe = { f1 = true, f2 = 12 }, bavian = { citron={x=15,y=2}, appelsin = \"Nora\" } }");
            Assert.AreEqual("{ abe = { f1=True, f2=12 }, bavian = { citron = { x=15, y=2 }, appelsin=Nora } }", block.GetTestString());

            block = FileBuilder.ParsePropertyBlock("{ axe abe = { f1 = true, fly f2 = 12 }, monkey bavian = { citron={x=15,y1 y2=2}, dejlig appelsin = \"Nora\" } }");
            Assert.AreEqual("{ axe abe = { f1=True, fly f2=12 }, monkey bavian = { citron = { x=15, y1 y2=2 }, dejlig appelsin=Nora } }", block.GetTestString());
        }

        [TestMethod]
        public void TestPropertySimpleArrays()
        {
            var block = FileBuilder.ParsePropertyBlock(
                "{ ak = [7,6,5,4,3,2,1,0], be = [true, true, false, true], cy = [\"Mor\", \"Far\"] }");
            Assert.AreEqual("{ ak = [ 7, 6, 5, 4, 3, 2, 1, 0 ], be = [ True, True, False, True ], cy = [ Mor, Far ] }", block.GetTestString());

            block = FileBuilder.ParsePropertyBlock(
                "{ cars ak = [7,6,5,4,3,2,1,0], planes be = [true, true, false, true], cycles cy = [\"Mor\", \"Far\"] }");
            Assert.AreEqual("{ cars ak = [ 7, 6, 5, 4, 3, 2, 1, 0 ], planes be = [ True, True, False, True ], cycles cy = [ Mor, Far ] }", block.GetTestString());
        }

        [TestMethod]
        public void TestPropertySimpleEvent()
        {
            var block = FileBuilder.ParsePropertyBlock("{ on Timeout : error }");
            Assert.AreEqual("{ on Timeout : Error }", block.GetTestString());
        }

        [TestMethod]
        public void TestPropertyMixedBlock()
        {
            var block = FileBuilder.ParsePropertyBlock("{ on Timeout : error, aksel = \"Jeps\", date = @2016-06-12, ages = [12.4, 5.3, 7.4], Timeout = 500ms }");
            Assert.AreEqual("{ on Timeout : Error, aksel=Jeps, date=06/12/2016 00:00:00, ages = [ 12.4, 5.3, 7.4 ], Timeout=00:00:00.5000000 }", block.GetTestString());
        }

        #region Utils
        private void AssertBlockWithOneValue(PropertyBlock block, string name, object expected)
        {
            Assert.AreEqual(1, block.Count);
            Assert.IsNull(block[0].SpecifiedTypeName);
            Assert.AreEqual(name, block[0].Name, false, CultureInfo.InvariantCulture);
            Assert.AreEqual(PropertyBlockEntryType.Value, block[0].BlockEntryType);
            Assert.AreEqual(expected, (block[0] as PropertyBlockValue).Value);
        }
        private void AssertBlockWithOneValue(PropertyBlock block, string type, string name, object expected)
        {
            Assert.AreEqual(1, block.Count);
            Assert.AreEqual(type, block[0].SpecifiedTypeName, false, CultureInfo.InvariantCulture);
            Assert.AreEqual(name, block[0].Name, false, CultureInfo.InvariantCulture);
            Assert.AreEqual(PropertyBlockEntryType.Value, block[0].BlockEntryType);
            Assert.AreEqual(expected, (block[0] as PropertyBlockValue).Value);
        }
        #endregion
    }
}
