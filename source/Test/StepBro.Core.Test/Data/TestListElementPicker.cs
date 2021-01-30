using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;

namespace StepBroCoreTest.Data
{
    [TestClass]
    public class TestListElementPicker
    {
        [TestMethod]
        public void Creation()
        {
            var list = new List<int>(new int[] { 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 });
            var picker = new ListElementPicker<int>(list);

            Assert.AreEqual(0, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.AreEqual(50, picker.Current);
            Assert.IsTrue(picker.IsCurrentUnpicked());
        }

        [TestMethod]
        public void PickAllOneByOne()
        {
            var list = new List<int>(new int[] { 50, 51, 52, 53, 54 });
            var picker = new ListElementPicker<int>(list);

            Assert.AreEqual(50, picker.Current);

            Assert.AreEqual(50, picker.Pick());
            Assert.AreEqual(51, picker.Current);
            Assert.AreEqual(1, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(51, picker.Pick());
            Assert.AreEqual(52, picker.Current);
            Assert.AreEqual(2, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(52, picker.Pick());
            Assert.AreEqual(53, picker.Current);
            Assert.AreEqual(3, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(53, picker.Pick());
            Assert.AreEqual(54, picker.Current);
            Assert.AreEqual(4, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(54, picker.Pick());
            Assert.AreEqual(-1, picker.CurrentIndex);
            Assert.AreEqual(-1, picker.CurrentIndexOfUnpicked);
            Assert.IsFalse(picker.IsCurrentUnpicked());
        }

        [TestMethod]
        public void PickAndSkip()
        {
            var list = new List<int>(new int[] { 50, 51, 52, 53, 54 });
            var picker = new ListElementPicker<int>(list);

            Assert.AreEqual(50, picker.Current);

            Assert.IsTrue(picker.SkipToNextUnpicked()); // Don't pick, just skip to next.
            Assert.AreEqual(51, picker.Current);
            Assert.AreEqual(1, picker.CurrentIndex);
            Assert.AreEqual(1, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(51, picker.Pick());
            Assert.AreEqual(52, picker.Current);
            Assert.AreEqual(2, picker.CurrentIndex);
            Assert.AreEqual(1, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.IsTrue(picker.SkipToNextUnpicked()); // Don't pick, just skip to next.
            Assert.AreEqual(53, picker.Current);
            Assert.AreEqual(3, picker.CurrentIndex);
            Assert.AreEqual(2, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(53, picker.Pick());
            Assert.AreEqual(54, picker.Current);
            Assert.AreEqual(4, picker.CurrentIndex);
            Assert.AreEqual(2, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.IsTrue(picker.SelectFirstUnpicked());
            Assert.AreEqual(50, picker.Current);
            Assert.AreEqual(0, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(50, picker.Pick());
            Assert.AreEqual(52, picker.Current);
            Assert.AreEqual(2, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(52, picker.Pick());
            Assert.AreEqual(54, picker.Current);
            Assert.AreEqual(4, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.IsTrue(picker.SelectFirstUnpicked());
            Assert.AreEqual(54, picker.Current);
            Assert.AreEqual(4, picker.CurrentIndex);
            Assert.AreEqual(0, picker.CurrentIndexOfUnpicked);
            Assert.IsTrue(picker.IsCurrentUnpicked());

            Assert.AreEqual(54, picker.Pick());     // Pick last one
            Assert.AreEqual(-1, picker.CurrentIndex);
            Assert.AreEqual(-1, picker.CurrentIndexOfUnpicked);
            Assert.IsFalse(picker.IsCurrentUnpicked());

            // All picked now.
            Assert.IsFalse(picker.SelectFirstUnpicked());
            Assert.AreEqual(-1, picker.CurrentIndex);
            Assert.AreEqual(-1, picker.CurrentIndexOfUnpicked);
            Assert.IsFalse(picker.IsCurrentUnpicked());
        }
    }
}