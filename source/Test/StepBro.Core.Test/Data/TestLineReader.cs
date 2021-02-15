using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBroCoreTest.Data;

namespace StepBro.Core.Test.Data
{
    [TestClass]
    public class TestLineReader
    {
        //ILineReaderEntry Current { get; }
        //bool LinesHaveTimestamp { get; }
        //bool HasMore { get; }
        //bool Next();
        //void Flush(ILineReaderEntry stopAt = null);
        //IEnumerable<ILineReaderEntry> Peak();

        [TestMethod]
        public void LineReaderCreation()
        {
            var list = DummyClass.CreateListOfStrings();
            var reader = list.ToLineReader();
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
        }

        [TestMethod]
        public void LineReaderNext()
        {
            var list = DummyClass.CreateListOfStrings();
            var reader = list.ToLineReader();
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);

            Assert.IsTrue(reader.Next());
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);

            Assert.IsTrue(reader.Next());
            Assert.AreEqual("Andres", reader.Current.Text);     // ! not Anders, but Andres
            Assert.IsTrue(reader.HasMore);

            for (int i = 0; i < 5; i++) reader.Next();  // Skip 5 elements
            Assert.AreEqual("Bent Fabric", reader.Current.Text);
        }

        [TestMethod]
        public void LineReaderPeakAndFlush()
        {
            var list = DummyClass.CreateListOfStrings();
            var reader = list.ToLineReader();
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);

            Assert.IsTrue(reader.Next());
            Assert.IsTrue(reader.Next());
            Assert.AreEqual("Andres", reader.Current.Text);

            var peaker = reader.Peak();
            Assert.AreEqual("Andres", reader.Current.Text);    // Still at the same element.

            var nollerik = peaker.FirstOrDefault(e => e.Text.Equals("Bent Nollerik"));
            Assert.AreEqual("Bent Nollerik", nollerik.Text);

            reader.Flush(nollerik);
            Assert.AreEqual("Bent Nollerik", reader.Current.Text);  // New current element.
        }
    }
}
