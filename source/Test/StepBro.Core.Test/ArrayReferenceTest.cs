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
    public class ArrayReferenceTest
    {
        [TestMethod]
        public void TestCreateSunshine()
        {
            var arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });

            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 8);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 7);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 6);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 5);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 4);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 3);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 2);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 1);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 0);

            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4, 4);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4, 3);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4, 2);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4, 1);
            arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 4, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestLengthWrong1()
        {
            var arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, 9);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestLengthWrong2()
        {
            var arr = new ArrayReference<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1, 8);
        }
    }

    internal static class ArrayReferenceTestSupport
    {

    }
}
