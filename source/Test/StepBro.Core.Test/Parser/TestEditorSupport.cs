using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestEditorSupport
    {
        [TestMethod]
        public void TestMethodAutoCompleteMatchWeight()
        {
            Assert.AreEqual(10000, EditorSupport.CalculateMatchingWeight("a", "a"));
            Assert.AreEqual(30000, EditorSupport.CalculateMatchingWeight("abc", "abc"));
            Assert.AreEqual(30000, EditorSupport.CalculateMatchingWeight("ABC", "ABC"));

            Assert.AreEqual(30000 - 100, EditorSupport.CalculateMatchingWeight("bcd", "abcde"));
            Assert.AreEqual(30000 - 200, EditorSupport.CalculateMatchingWeight("cde", "abcde"));

            Assert.AreEqual(30000 - 100 - 5, EditorSupport.CalculateMatchingWeight("Bcd", "abcde"));
            Assert.AreEqual(30000 - 200 - 10, EditorSupport.CalculateMatchingWeight("CDe", "abcde"));

            Assert.AreEqual(0, EditorSupport.CalculateMatchingWeight("bce", "abcde"));
            Assert.AreEqual(0, EditorSupport.CalculateMatchingWeight("ebc", "abcde"));
        }
    }
}
