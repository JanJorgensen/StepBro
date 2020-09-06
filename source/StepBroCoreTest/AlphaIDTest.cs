using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;

namespace StepBroCoreTest
{
    [TestClass]
    public class AlphaIDTest
    {
        [TestMethod]
        public void TestCreateSunshine()
        {
            Assert.AreEqual("BA", AlphaID.Create(0, 1));
            Assert.AreEqual("DA", AlphaID.Create(1, 1));
            Assert.AreEqual("VA", AlphaID.Create(14, 1));
            Assert.AreEqual("BE", AlphaID.Create(15, 1));

            Assert.AreEqual("BABA", AlphaID.Create(0, 2));
            Assert.AreEqual("BADA", AlphaID.Create(1, 2));
            Assert.AreEqual("BAVA", AlphaID.Create(14, 2));
            Assert.AreEqual("BABE", AlphaID.Create(15, 2));
        }

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void TestLengthWrong1()
        //{
        //    var arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 9);
        //}
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void TestLengthWrong2()
        //{
        //    var arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1, 8);
        //}
    }
}
