using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;

namespace StepBroCoreTest.Data
{
    [TestClass]
    public class TestValueContainer
    {
        [TestMethod]
        public void TestContainer()
        {
            var containerAccess = VariableContainer.Create("", "Anders", TypeReference.TypeInt32, false);
            Assert.IsNotNull(containerAccess);
            var container = containerAccess.Container as IValueContainer<int>;
            Assert.IsNotNull(container);
            var rc = container as IValueContainerRich;
            Assert.IsNotNull(rc);
            Assert.AreEqual(0, rc.ValueChangeIndex);
            Assert.AreEqual(0, container.GetValue());
            containerAccess.SetValue(14, null);
            Assert.AreEqual(1, rc.ValueChangeIndex);
            Assert.AreEqual(14, container.GetValue());

            ValueContainerModifier<int> modifier = (int i, out int o) => { o = i + 2; return i; };
            Assert.AreEqual(14, container.Modify(modifier));
            Assert.AreEqual(2, rc.ValueChangeIndex);
            Assert.AreEqual(16, container.GetValue());
        }
    }
}
