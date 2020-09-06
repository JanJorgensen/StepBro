using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using XH = StepBro.Core.Execution.ExecutionHelperMethods;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestExecutionHelperMethods
    {
        private static double AddSmall(double value) { return value + (value * 1.0E-15); }
        private static double AddSome(double value) { return value + (value * 1.0E-12); }

        [TestMethod]
        public void TestApproximatelyEquals()
        {
            Assert.IsTrue(XH.ApproximatelyEquals(0.0, 0.0));
            Assert.IsTrue(XH.ApproximatelyEquals(3.4, 3.4));
            Assert.IsTrue(XH.ApproximatelyEquals(-3.4, -3.4));
            Assert.IsTrue(XH.ApproximatelyEquals(34E200, 34E200));
            Assert.IsTrue(XH.ApproximatelyEquals(-34E200, -34E200));

            Assert.IsFalse(XH.ApproximatelyEquals(0.0, 1.0));
            Assert.IsFalse(XH.ApproximatelyEquals(1.0, 0.0));
            Assert.IsFalse(XH.ApproximatelyEquals(0.0, -1.0));
            Assert.IsFalse(XH.ApproximatelyEquals(-1.0, 0.0));

            Assert.IsTrue(XH.ApproximatelyEquals(4.0, AddSmall(4.0)));
            Assert.IsFalse(XH.ApproximatelyEquals(4.0, AddSome(4.0)));

            Assert.IsTrue(XH.ApproximatelyEquals(-4.0, -AddSmall(4.0)));
            Assert.IsFalse(XH.ApproximatelyEquals(-4.0, -AddSome(4.0)));

            Assert.IsTrue(XH.ApproximatelyEquals(3000, AddSmall(3000.0)));
            Assert.IsFalse(XH.ApproximatelyEquals(3000.0, AddSome(3000.0)));

            Assert.IsTrue(XH.ApproximatelyEquals(-3000.0, -AddSmall(3000.0)));
            Assert.IsFalse(XH.ApproximatelyEquals(-3000.0, -AddSome(3000.0)));

            Assert.IsTrue(XH.ApproximatelyEquals(0.0000001, AddSmall(0.0000001)));
            Assert.IsFalse(XH.ApproximatelyEquals(0.0000001, AddSome(0.0000001)));

            Assert.IsTrue(XH.ApproximatelyEquals(-0.0000001, -AddSmall(0.0000001)));
            Assert.IsFalse(XH.ApproximatelyEquals(-0.0000001, -AddSome(0.0000001)));
        }

        [TestMethod]
        public void TestGreaterThanOrApproximately()
        {
            Assert.IsTrue(XH.GreaterThanOrApprox(0.0, 0.0));
            Assert.IsTrue(XH.GreaterThanOrApprox(3.4, 3.4));
            Assert.IsTrue(XH.GreaterThanOrApprox(-3.4, -3.4));
            Assert.IsTrue(XH.GreaterThanOrApprox(34E200, 34E200));
            Assert.IsTrue(XH.GreaterThanOrApprox(-34E200, -34E200));

            Assert.IsFalse(XH.GreaterThanOrApprox(0.0, 1.0));
            Assert.IsTrue(XH.GreaterThanOrApprox(1.0, 0.0));
            Assert.IsTrue(XH.GreaterThanOrApprox(0.0, -1.0));
            Assert.IsFalse(XH.GreaterThanOrApprox(-1.0, 0.0));

            Assert.IsTrue(XH.GreaterThanOrApprox(4.0, AddSmall(4.0)));
            Assert.IsFalse(XH.GreaterThanOrApprox(4.0, AddSome(4.0)));

            Assert.IsTrue(XH.GreaterThanOrApprox(-4.0, -AddSmall(4.0)));
            Assert.IsTrue(XH.GreaterThanOrApprox(-4.0, -AddSome(4.0)));

            Assert.IsTrue(XH.GreaterThanOrApprox(3000, AddSmall(3000.0)));
            Assert.IsFalse(XH.GreaterThanOrApprox(3000.0, AddSome(3000.0)));

            Assert.IsTrue(XH.GreaterThanOrApprox(-3000.0, -AddSmall(3000.0)));
            Assert.IsTrue(XH.GreaterThanOrApprox(-3000.0, -AddSome(3000.0)));

            Assert.IsTrue(XH.GreaterThanOrApprox(0.0000001, AddSmall(0.0000001)));
            Assert.IsFalse(XH.GreaterThanOrApprox(0.0000001, AddSome(0.0000001)));

            Assert.IsTrue(XH.GreaterThanOrApprox(-0.0000001, -AddSmall(0.0000001)));
            Assert.IsTrue(XH.GreaterThanOrApprox(-0.0000001, -AddSome(0.0000001)));
        }

        [TestMethod]
        public void TestLessThanOrApproximately()
        {
            Assert.IsTrue(XH.LessThanOrApprox(0.0, 0.0));
            Assert.IsTrue(XH.LessThanOrApprox(3.4, 3.4));
            Assert.IsTrue(XH.LessThanOrApprox(-3.4, -3.4));
            Assert.IsTrue(XH.LessThanOrApprox(34E200, 34E200));
            Assert.IsTrue(XH.LessThanOrApprox(-34E200, -34E200));

            Assert.IsTrue(XH.LessThanOrApprox(0.0, 1.0));
            Assert.IsFalse(XH.LessThanOrApprox(1.0, 0.0));
            Assert.IsFalse(XH.LessThanOrApprox(0.0, -1.0));
            Assert.IsTrue(XH.LessThanOrApprox(-1.0, 0.0));

            Assert.IsTrue(XH.LessThanOrApprox(4.0, AddSmall(4.0)));
            Assert.IsTrue(XH.LessThanOrApprox(4.0, AddSome(4.0)));

            Assert.IsTrue(XH.LessThanOrApprox(-4.0, -AddSmall(4.0)));
            Assert.IsFalse(XH.LessThanOrApprox(-4.0, -AddSome(4.0)));

            Assert.IsTrue(XH.LessThanOrApprox(3000, AddSmall(3000.0)));
            Assert.IsTrue(XH.LessThanOrApprox(3000.0, AddSome(3000.0)));

            Assert.IsTrue(XH.LessThanOrApprox(-3000.0, -AddSmall(3000.0)));
            Assert.IsFalse(XH.LessThanOrApprox(-3000.0, -AddSome(3000.0)));

            Assert.IsTrue(XH.LessThanOrApprox(0.0000001, AddSmall(0.0000001)));
            Assert.IsTrue(XH.LessThanOrApprox(0.0000001, AddSome(0.0000001)));

            Assert.IsTrue(XH.LessThanOrApprox(-0.0000001, -AddSmall(0.0000001)));
            Assert.IsFalse(XH.LessThanOrApprox(-0.0000001, -AddSome(0.0000001)));
        }
    }
}