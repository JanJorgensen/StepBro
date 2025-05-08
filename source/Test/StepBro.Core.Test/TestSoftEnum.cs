using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestSoftEnum
    {
        [TestMethod]
        public void TestTypeCreate()
        {
            try
            {
                m_populatingNotificationReceived = false;
                m_populatedNotificationReceived = false;

                var type = SoftEnumManager.Instance.CreateOrGetType("MyTestScript", "MySoftEnum");
                var creator = type.Setup();
                creator.Type.Populating += TheEnum_Populating;
                creator.Type.Populated += TheEnum_Populated;

                Assert.IsFalse(m_populatingNotificationReceived);
                Assert.IsFalse(m_populatedNotificationReceived);

                using (var populator = creator.Populate(null))
                {
                    Assert.IsTrue(m_populatingNotificationReceived);
                    m_populatingNotificationReceived = false;
                    Assert.IsFalse(m_populatedNotificationReceived);

                    populator.AddEntry("Anders", 1);
                    populator.AddEntry("Bent", 2);
                    populator.AddEntry("Charles", 3);
                    populator.AddEntry("Dennis", 4);

                    Assert.IsFalse(m_populatingNotificationReceived);
                    Assert.IsFalse(m_populatedNotificationReceived);
                }
                Assert.IsFalse(m_populatingNotificationReceived);
                Assert.IsTrue(m_populatedNotificationReceived);
            }
            finally
            {
                //SoftEnumTyped<MySoftEnum>.Reset();
            }
        }

        //[TestMethod]
        //public void TestCasting()
        //{
        //    try
        //    {
        //        try
        //        {
        //            using (var populator = SoftEnumTyped<MySoftEnum>.CreateType().Populate(null))
        //            {
        //                populator.AddEntry("Anders", 1);
        //                populator.AddEntry("Bent", 2);
        //                populator.AddEntry("Charles", 3);
        //                populator.AddEntry("Dennis", 4);
        //            }
        //        }
        //        catch { }

        //        Assert.AreEqual(3, SoftEnumTyped<MySoftEnum>.FromString("Charles").Value);
        //        Assert.AreEqual("Bent", SoftEnumTyped<MySoftEnum>.FromValue(2).Name);

        //        SoftEnumTyped<MySoftEnum> myValue1 = 2;
        //        Assert.AreEqual(2, myValue1.Value);
        //        Assert.AreEqual("Bent", myValue1.Name);

        //        SoftEnumTyped<MySoftEnum> myValue2 = "Charles";
        //        Assert.AreEqual("Charles", myValue2.Name);
        //        Assert.AreEqual(3, myValue2.Value);

        //        SoftEnumTyped<MySoftEnum> myValue3 = (SoftEnumTyped<MySoftEnum>)"Dennis";
        //        Assert.AreEqual("Dennis", myValue3.Name);
        //        Assert.AreEqual(4, myValue3.Value);
        //    }
        //    finally
        //    {
        //        SoftEnumTyped<MySoftEnum>.Reset();
        //    }
        //}

        private bool m_populatingNotificationReceived = false;
        private void TheEnum_Populating(object sender, EventArgs e)
        {
            m_populatingNotificationReceived = true;
        }

        private bool m_populatedNotificationReceived = false;
        private void TheEnum_Populated(object sender, EventArgs e)
        {
            m_populatedNotificationReceived = true;
        }
    }
}
