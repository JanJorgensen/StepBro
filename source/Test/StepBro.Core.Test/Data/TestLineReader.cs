using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBroCoreTest.Data;
using System;

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

        [TestMethod]
        public void LogLineLineReaderCreation()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);
        }

        [TestMethod]
        public void LogLineLineReaderFind01()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian");
            Assert.AreEqual("Christian", found);
        }

        [TestMethod]
        public void LogLineLineReaderFind02()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(100));
            Assert.AreEqual(null, found);
        }

        [TestMethod]
        public void LogLineLineReaderFind03()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);
        }

        [TestMethod]
        public void LogLineLineReaderFind04()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Find(null, "Anders");
            Assert.AreEqual(null, found);
        }

        [TestMethod]
        public void LogLineLineReaderFind05()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Find(null, "Dorte");
            Assert.AreEqual("Dorte", found);
        }

        [TestMethod]
        public void LogLineLineReaderFind06()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Find(null, "Dorte", limit: TimeSpan.FromSeconds(5));
            Assert.AreEqual(null, found);
        }

        [TestMethod]
        public void LogLineLineReaderFind07()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Find(null, "Christian", limit: TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Find(null, "Dorte", limit: TimeSpan.FromSeconds(65));
            Assert.AreEqual("Dorte", found);
        }

        [TestMethod]
        public void LogLineLineReaderAwait01()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Await(null, "Christian", TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);
        }

        [TestMethod]
        public void LogLineLineReaderAwait02()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Await(null, "Christian", TimeSpan.FromSeconds(5));
            // As the reader already has all the data
            // we will find it within the allotted time always.
            // Even when the timestamps are off, as that is not
            // what await is for. Await is for real-time, Find is for timestamps.
            Assert.AreEqual("Christian", found);
        }

        [TestMethod]
        public void LogLineLineReaderAwait03()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Await(null, "Christian", TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Await(null, "Anders", TimeSpan.FromSeconds(1));
            Assert.AreEqual(null, found);
        }

        [TestMethod]
        public void LogLineLineReaderAwait04()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Await(null, "Christian", TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Await(null, "Dorte", TimeSpan.FromSeconds(65));
            Assert.AreEqual("Dorte", found);
        }

        [TestMethod]
        public void LogLineLineReaderAwait05()
        {
            object sync = new object();
            var logLineData = DummyClass.CreateLogLineData();
            var reader = DummyClass.CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);

            string found = reader.Await(null, "Christian", TimeSpan.FromSeconds(130));
            Assert.AreEqual("Christian", found);

            found = reader.Await(null, "Dorte", TimeSpan.FromSeconds(35));
            Assert.AreEqual("Dorte", found); // Same as 02
        }
    }
}
