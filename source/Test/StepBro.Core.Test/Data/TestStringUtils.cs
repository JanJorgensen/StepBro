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
            Predicate<string> equalsMatcher = new EqualsStringMatch("AbsaLon").Matches;
            Assert.IsTrue(equalsMatcher("AbsaLon"));
            Assert.IsFalse(equalsMatcher("Absalon"));
            Assert.IsFalse(equalsMatcher("mAbsaLon"));
            Assert.IsFalse(equalsMatcher("AbsaLons"));

            Predicate<string> startsWithMatcher = new StartsWithStringMatch("AbsaLon").Matches;
            Assert.IsTrue(startsWithMatcher("AbsaLon"));
            Assert.IsFalse(startsWithMatcher("Absalon"));
            Assert.IsFalse(startsWithMatcher("mAbsaLon"));
            Assert.IsTrue(startsWithMatcher("AbsaLons"));
            Assert.IsFalse(startsWithMatcher("Absalonsk"));

            Predicate<string> endsWithMatcher = new EndsWithStringMatch("AbsaLon").Matches;
            Assert.IsTrue(endsWithMatcher("AbsaLon"));
            Assert.IsFalse(endsWithMatcher("Absalon"));
            Assert.IsTrue(endsWithMatcher("mAbsaLon"));
            Assert.IsFalse(endsWithMatcher("mAbsalon"));
            Assert.IsFalse(endsWithMatcher("AbsaLonsk"));
        }
    }
}
