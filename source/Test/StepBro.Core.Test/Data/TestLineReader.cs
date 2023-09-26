using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBroCoreTest.Data;
using System;
using System.Collections.Generic;

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

        public static List<string> CreateListOfStrings()
        {
            var list = new List<string>();
            list.AddRange(new string[] {
                "Anders",
                "Anders",
                "Andres", // ! 
                "Anders",
                "Anders",
                "Anders",
                "Anders",
                "Bent Fabric",
                "Bente Bent",
                "Anders",
                "Anders",
                "Bent Nollerik",
                "Bodil",
                "Anders",
                "Anders",
                "Anders",
                "Anders",
                "Bente Birk",
                "Anders A",
                "Anders B",
                "Anders C",
                "Anders D",
                "Anders E",
                "Anders F",
                "Anders G",
                "Christian",
            });
            return list;
        }

        public static LogLineData CreateLogLineData()
        {
            LogLineData first = new LogLineData(
                null,
                LogLineData.LogType.Neutral,
                0,
                "*Anders",
                DateTime.Parse("2023-09-26T11:35:00.0000000Z"));

            LogLineData second = new LogLineData(
                first,
                LogLineData.LogType.Neutral,
                1,
                "*Bent",
                DateTime.Parse("2023-09-26T11:36:00.0000000Z"));

            LogLineData third = new LogLineData(
                second,
                LogLineData.LogType.Neutral,
                1,
                "*Christian",
                DateTime.Parse("2023-09-26T11:37:00.0000000Z"));

            LogLineData fourth = new LogLineData(
                third,
                LogLineData.LogType.Neutral,
                1,
                "*Dorte",
                DateTime.Parse("2023-09-26T11:38:00.0000000Z"));

            LogLineData fifth = new LogLineData(
                fourth,
                LogLineData.LogType.Neutral,
                1,
                "*Emil",
                DateTime.Parse("2023-09-26T11:39:00.0000000Z"));

            return first;
        }

        public static LogLineLineReader CreateLogLineLineReader(LogLineData first, object sync)
        {
            return new LogLineLineReader(null, first, sync);
        }

        [TestMethod]
        public void LineReaderCreation()
        {
            var list = CreateListOfStrings();
            var reader = list.ToLineReader();
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
        }

        [TestMethod]
        public void LineReaderNext()
        {
            var list = CreateListOfStrings();
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
            var list = CreateListOfStrings();
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
            Assert.AreEqual("Anders", reader.Current.Text);
            Assert.IsTrue(reader.HasMore);
            Assert.IsTrue(reader.LinesHaveTimestamp);
        }

        [TestMethod]
        public void LogLineLineReaderFind01()
        {
            object sync = new object();
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
            var logLineData = CreateLogLineData();
            var reader = CreateLogLineLineReader(logLineData, sync);
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
