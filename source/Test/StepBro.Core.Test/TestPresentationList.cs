using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Test
{
    [TestClass]
    public class TestPresentationList
    {
        class PresentationEntry
        {
            public PresentationEntry(LogEntry logEntry, long index)
            {
                this.LogEntry = logEntry;
                this.SourceIndex = index;
            }
            public LogEntry LogEntry { get; private set; }
            public long SourceIndex { get; private set; }
            public override string ToString()
            {
                return "PresentationEntry for #" + this.SourceIndex + "; " + this.LogEntry.ToString();
            }
        }

        class PresentationList : PresentationListForListData<LogEntry, PresentationEntry>
        {
            public PresentationList(IDataListSource<LogEntry> source) : base(source, 10000, 1000)
            {
            }

            public override void CreatePresentationEntry(LogEntry entry, long sourceIndex, Action<PresentationEntry> adder)
            {
                adder(new PresentationEntry(entry, sourceIndex));
            }

            public override LogEntry PresentationToSource(PresentationEntry entry)
            {
                throw new NotImplementedException();
            }

            public DataCache<PresentationEntry> Cache { get { return m_cache; } }
        }

        [TestMethod]
        public void PutInWithFilterAndFollowTip()
        {
            var logger = new Logger("", false, "UnitTest", "Go!");

            var loggerState = logger.GetState();
            Assert.AreEqual(1L, loggerState.EffectiveCount);

            int i = 2;
            for (; i <= 25000; i++) // Just enough to not overflow the cache with the filter used below.
            {
                logger.RootLogger.Log("Bent" + i.ToString("D6"));
            }

            var presentation = new PresentationList(logger);

            Predicate<LogEntry> filter = (LogEntry entry) =>
            {
                return entry.Text.Substring(entry.Text.Length - 3).Contains('3');
            };
            presentation.Reset(filter, Int64.MaxValue);

            var firstPresented = presentation.Get(0L);
            Assert.AreEqual("Bent000003", firstPresented.LogEntry.Text);
            var state = presentation.GetState();

            Assert.AreEqual(6775L, state.EffectiveCount);
            var cacheRange = presentation.Cache.CachedRange();
            Assert.AreEqual(0L, cacheRange.Item1);
            Assert.AreEqual(6774L, cacheRange.Item2);

            //Add more, to fill the cache and thereby loose entries in the beginning.
            for (; i <= 38000; i++)
            {
                logger.RootLogger.Log("Entry" + i.ToString("D6"));
            }

            state = presentation.GetState();
            Assert.AreEqual(6775L, state.EffectiveCount);

            presentation.UpdateHead();
            state = presentation.GetState();
            Assert.AreEqual(10298L, state.EffectiveCount);

            cacheRange = presentation.Cache.CachedRange();
            Assert.AreEqual(298L, cacheRange.Item1);
            Assert.AreEqual(10297L, cacheRange.Item2);
        }
    }
}
