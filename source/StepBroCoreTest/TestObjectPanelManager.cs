﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Controls;
using StepBro.Core.Controls.WinForms;
using StepBro.Core.Data;
using System.Collections.Generic;
using System.Linq;
using ObjectPanel = StepBro.Core.Controls.WinForms.ObjectPanel;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestObjectPanelManager
    {
        private static ObjectPanelManager CreateObjectPanelManager()
        {
            ObjectPanelManager manager = new ObjectPanelManager(out IService service);
            manager.AddPanelCreator(new Creator1());
            manager.AddPanelCreator(new Creator2());
            service.Start(null, null);

            Assert.AreEqual(5, manager.ListPanelTypes().Count());
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());

            return manager;
        }

        [TestMethod]
        public void ObjectPanelManager_CreateStaticSingleOnly()
        {
            var manager = CreateObjectPanelManager();

            const string NAME = "Panel1 Title";

            var panelA = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNotNull(panelA);
            Assert.IsTrue(panelA.GetType() == typeof(Panel1));
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());
            // Not expecting to be able to create one more
            var panelB = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNull(panelB);
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Dispose, to be able to create new.
            panelA.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());

            panelB = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNotNull(panelB);
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Now dispose the panels
            panelA.Dispose();
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());
            panelB.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());
        }

        [TestMethod]
        public void ObjectPanelManager_CreateStaticMultipleAllowed()
        {
            var manager = CreateObjectPanelManager();

            const string NAME = "Panel5 Title";

            var panelA = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNotNull(panelA);
            Assert.IsTrue(panelA.GetType() == typeof(Panel5));
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Expect to be able to create more instances
            var panelB = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNotNull(panelB);
            Assert.IsTrue(panelB.GetType() == typeof(Panel5));
            Assert.AreEqual(2, manager.ListCreatedPanels().Count());

            // ... and still more
            var panelC = manager.CreateStaticPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME));
            Assert.IsNotNull(panelC);
            Assert.IsTrue(panelC.GetType() == typeof(Panel5));
            Assert.AreEqual(3, manager.ListCreatedPanels().Count());

            // Now dispose the panels
            panelB.Dispose();
            Assert.AreEqual(2, manager.ListCreatedPanels().Count());
            panelA.Dispose();
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());
            panelC.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());
        }

        [TestMethod]
        public void ObjectPanelManager_CreateObjectSingleOnly()
        {
            var manager = CreateObjectPanelManager();

            const string NAME = "Panel2 Title";

            var vc1 = new VariableContainer<bool>("", "", true, false);
            var panelA = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc1);
            Assert.IsNotNull(panelA);
            Assert.IsTrue(panelA.GetType() == typeof(Panel2));
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Not expecting to be able to create one more
            var vc2 = new VariableContainer<bool>("", "", true, false);
            var panelB = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc2);
            Assert.IsNull(panelB);
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Dispose, to be able to create new.
            panelA.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());

            panelB = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc2);
            Assert.IsNotNull(panelB);
            Assert.IsTrue(panelB.GetType() == typeof(Panel2));
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Now dispose the panels
            panelA.Dispose();
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());
            panelB.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());
        }

        [TestMethod]
        public void ObjectPanelManager_CreateObjectMultipleAllowed()
        {
            var manager = CreateObjectPanelManager();

            const string NAME = "Panel3 Title";

            var vc1 = new VariableContainer<long>("", "", 10L, false);
            var panelA = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc1);
            Assert.IsNotNull(panelA);
            Assert.IsTrue(panelA.GetType() == typeof(Panel3));
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());

            // Expect to be able to create more instances
            var vc2 = new VariableContainer<long>("", "", 20L, false);
            var panelB = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc2);
            Assert.IsNotNull(panelB);
            Assert.IsTrue(panelB.GetType() == typeof(Panel3));
            Assert.AreEqual(2, manager.ListCreatedPanels().Count());

            // ... and still more
            var vc3 = new VariableContainer<long>("", "", 30L, false);
            var panelC = manager.CreateObjectPanel(manager.ListPanelTypes().FirstOrDefault(p => p.Name == NAME), vc3);
            Assert.IsNotNull(panelC);
            Assert.IsTrue(panelC.GetType() == typeof(Panel3));
            Assert.AreEqual(3, manager.ListCreatedPanels().Count());

            // Now dispose the panels
            panelB.Dispose();
            Assert.AreEqual(2, manager.ListCreatedPanels().Count());
            panelA.Dispose();
            Assert.AreEqual(1, manager.ListCreatedPanels().Count());
            panelC.Dispose();
            Assert.AreEqual(0, manager.ListCreatedPanels().Count());
        }

        #region Test Data

        private class Creator1 : ObjectPanelCreator
        {
            public Creator1() : base() { }

            protected override IEnumerable<ObjectPanelInfo> CreatePanelList()
            {
                yield return new ObjectPanelInfo<Panel1>("Panel1 Title", "", false);
                yield return new ObjectPanelInfo<Panel2, bool>("Panel2 Title", "", false);
                yield return new ObjectPanelInfo<Panel3, long>("Panel3 Title", "", true);
            }
        }
        private class Creator2 : ObjectPanelCreator
        {
            public Creator2() : base() { }

            protected override IEnumerable<ObjectPanelInfo> CreatePanelList()
            {
                yield return new ObjectPanelInfo<Panel4, string>("Panel4 Title", "", false);
                yield return new ObjectPanelInfo<Panel5>("Panel5 Title", "", true);
            }
        }

        public class ObjectPanelBindable : ObjectPanel
        {
            public ObjectPanelBindable() : base() { }
            public override bool IsBindable { get { return true; } }
            protected override bool TryBind(object @object) { return true; }
        }

        public class Panel1 : ObjectPanel { }
        public class Panel2 : ObjectPanelBindable { }
        public class Panel3 : ObjectPanelBindable { }
        public class Panel4 : ObjectPanelBindable { }
        public class Panel5 : ObjectPanel { }

        #endregion
    }
}
