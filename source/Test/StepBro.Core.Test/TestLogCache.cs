using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Logging;

namespace StepBro.Core.Test
{
    [TestClass]
    public class TestLogCache
    {
        [TestMethod]
        public void InitialState()
        {
            var logger = new Logger("", false, "UnitTest", "Go!");
            var cache = new DataCache<LogEntry>(logger, 20, 10);
            
            var loggerState = logger.GetState();
            Assert.AreEqual(1L, loggerState.EffectiveCount);
            var cacheState = cache.GetState();
            Assert.AreEqual(1L, cacheState.EffectiveCount);
            var range = cache.CachedRange();
            Assert.AreEqual(-1L, range.Item1);
            Assert.AreEqual(-1L, range.Item2);
        }

        [TestMethod]
        public void OnlyFewLogEntries()
        {
            var logger = new Logger("", false, "UnitTest", "Go!");
            var cache = new DataCache<LogEntry>(logger, 20, 10);

            var first = cache.Get(0L);
            Assert.IsNotNull(first);
            var state = cache.GetState();
            Assert.AreEqual(1L, state.EffectiveCount);
            var range = cache.CachedRange();
            Assert.AreEqual(0L, range.Item1);
            Assert.AreEqual(0L, range.Item2);

            logger.RootLogger.Log("Entry2");
            logger.RootLogger.Log("Entry3");
            logger.RootLogger.Log("Entry4");
            logger.RootLogger.Log("Entry5");
            logger.RootLogger.Log("Entry6");

            state = cache.GetState();
            Assert.AreEqual(1L, state.EffectiveCount);

            first = cache.Get(0L);      // Get the first entry again, which was already in the cache.
            Assert.IsNotNull(first);
            range = cache.CachedRange();
            Assert.AreEqual(0L, range.Item1);
            Assert.AreEqual(0L, range.Item2);

            var second = cache.Get(1L); // Get the second entry, which should result in caching all entries.
            Assert.IsNotNull(second);
            range = cache.CachedRange();
            Assert.AreEqual(0L, range.Item1);
            Assert.AreEqual(5L, range.Item2);

            for (int i = 7; i <= 19; i++)
            {
                logger.RootLogger.Log("Entry" + i.ToString());
            }
            state = cache.GetState();
            Assert.AreEqual(6L, state.EffectiveCount);

            var entry = cache.Get(7L);      // Get one of the new entries, and see that more entries are cached.
            Assert.IsNotNull(entry);
            range = cache.CachedRange();
            Assert.AreEqual(0L, range.Item1);
            Assert.AreEqual(16L, range.Item2);  // # 7 plus nine more.

            entry = cache.Get(15L);         // Get one of the cached entries.
            Assert.IsNotNull(entry);
            Assert.AreEqual("Entry16", entry.Text);
            range = cache.CachedRange();
            Assert.AreEqual(0L, range.Item1);
            Assert.AreEqual(16L, range.Item2);
        }
    }
}
