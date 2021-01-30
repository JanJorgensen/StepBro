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
    public class TestErrorID
    {
        [TestMethod]
        public void TestNoNewDefault()
        {
            var id = new ErrorIDFirstLevelNoDefault();
            Assert.AreEqual(Verdict.Error, id.DefaultVerdict);
            Assert.AreEqual(nameof(ErrorIDFirstLevelNoDefault), id.Name);
        }

        [TestMethod]
        public void TestNewDefaultSet()
        {
            var id = new ErrorIDFirstLevelChangedDefault();
            Assert.AreEqual(Verdict.Fail, id.DefaultVerdict);
            Assert.AreEqual(nameof(ErrorIDFirstLevelChangedDefault), id.Name);
        }

        private class ErrorIDFirstLevelNoDefault : ErrorID { }

        [DefaultVerdict(Verdict.Fail)]
        private class ErrorIDFirstLevelChangedDefault : ErrorID { }
    }
}