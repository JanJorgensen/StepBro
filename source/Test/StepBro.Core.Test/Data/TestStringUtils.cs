using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static StepBro.Core.Data.StringUtils;

namespace StepBro.Core.Test.Data
{
    [TestClass]
    public class TestStringUtils
    {
        [TestMethod]
        public void WildcardCStringComparers()
        {
            Func<string, string> equalsMatcher = new EqualsStringMatch("AbsaLon").Matches;
            Assert.AreEqual("AbsaLon", equalsMatcher("AbsaLon"));
            Assert.AreEqual(null, equalsMatcher("Absalon"));
            Assert.AreEqual(null, equalsMatcher("mAbsaLon"));
            Assert.AreEqual(null, equalsMatcher("AbsaLons"));

            Func<string, string> startsWithMatcher = new StartsWithStringMatch("AbsaLon").Matches;
            Assert.AreEqual("", startsWithMatcher("AbsaLon"));
            Assert.AreEqual(null, startsWithMatcher("Absalon"));
            Assert.AreEqual(null, startsWithMatcher("mAbsaLon"));
            Assert.AreEqual("s", startsWithMatcher("AbsaLons"));
            Assert.AreEqual(null, startsWithMatcher("Absalonsk"));

            Func<string, string> endsWithMatcher = new EndsWithStringMatch("AbsaLon").Matches;
            Assert.AreEqual("", endsWithMatcher("AbsaLon"));
            Assert.AreEqual(null, endsWithMatcher("Absalon"));
            Assert.AreEqual("m", endsWithMatcher("mAbsaLon"));
            Assert.AreEqual(null, endsWithMatcher("mAbsalon"));
            Assert.AreEqual(null, endsWithMatcher("AbsaLonsk"));
        }
    }
}
