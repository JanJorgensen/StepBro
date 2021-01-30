using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestSandbox
    {
        //[TestMethod]
        //public void NullableTypeCheck()
        //{
        //    var t = typeof(int?);
        //    System.Diagnostics.Debug.WriteLine(t.Name);
        //    Assert.IsTrue(false);
        //}

        [TestMethod]
        public void CheckIsAssignableFrom()
        {
            Assert.IsTrue(typeof(Int64).IsAssignableFrom(typeof(Int64)));
            Assert.IsFalse(typeof(Int64).IsAssignableFrom(typeof(Int32)));
            Assert.IsFalse(typeof(Int32).IsAssignableFrom(typeof(Int64)));
            Assert.IsFalse(typeof(Double).IsAssignableFrom(typeof(Int64)));
            Assert.IsFalse(typeof(Int64).IsAssignableFrom(typeof(Double)));

            Assert.IsTrue(typeof(IBase).IsAssignableFrom(typeof(IInherited)));      // Read this one, to understand :-)
            Assert.IsFalse(typeof(IInherited).IsAssignableFrom(typeof(IBase)));

            Assert.IsTrue(typeof(Base).IsAssignableFrom(typeof(Inherited)));
            Assert.IsFalse(typeof(Inherited).IsAssignableFrom(typeof(Base)));

            Assert.IsTrue(typeof(IBase).IsAssignableFrom(typeof(Base)));
            Assert.IsFalse(typeof(Base).IsAssignableFrom(typeof(IBase)));

            Assert.IsFalse(typeof(Base).IsAssignableFrom(typeof(IInherited)));
            Assert.IsFalse(typeof(IInherited).IsAssignableFrom(typeof(Base)));

            Assert.IsTrue(typeof(IBase).IsAssignableFrom(typeof(Inherited)));
            Assert.IsFalse(typeof(Inherited).IsAssignableFrom(typeof(IBase)));

            Assert.IsTrue(typeof(IInherited).IsAssignableFrom(typeof(Inherited)));
            Assert.IsFalse(typeof(Inherited).IsAssignableFrom(typeof(IInherited)));
        }

        internal interface IBase { }
        internal interface IInherited : IBase { }

        internal class Base : IBase { }
        internal class Inherited : Base, IInherited { }
    }
}
