using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

using StepBro.Core.Data;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestObjectMonitor
    {
        public class MyClass : IAvailability
        {
            bool m_isDisposed = false;

            public MyClass()
            {
                //s_Monitors.Add(new MonitorClass(this));
            }

            ~MyClass()
            {
                System.Diagnostics.Debug.WriteLine("DTOR");
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            public event EventHandler Disposed;
            public event EventHandler Disposing;

            public void Dispose()
            {
                if (!m_isDisposed)
                {
                    m_isDisposed = true;
                    this.Disposing?.Invoke(this, EventArgs.Empty);
                    this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("IsDisposed"));
                    this.Disposed?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool IsDisposed
            {
                get { return m_isDisposed; }
            }

            public bool IsStillValid
            {
                get
                {
                    return !m_isDisposed;
                }
            }
        }

        public class UsingClass
        {
            private MyClass m_utilityObject = null;

            public void SetUtilityObject(MyClass obj)
            {
                if (m_utilityObject != null)
                {
                    m_utilityObject.PropertyChanged += UtilityObject_PropertyChanged;
                }
            }

            private void UtilityObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
            }
        }

        [TestMethod]
        public void TestObjectMonitorWholeCycle()
        {
            var obj = new MyClass();
            var monitor = ObjectMonitorManager.Global.CreateObjectMonitor(obj);
            System.GC.Collect();
            monitor.UpdateState();
            Assert.AreEqual(ObjectMonitor.State.TargetActive, monitor.CurrentState);

            obj.Dispose();
            System.GC.Collect();
            monitor.UpdateState();
            Assert.AreEqual(ObjectMonitor.State.TargetDisposed, monitor.CurrentState);

            Assert.IsTrue(obj.IsDisposed);      // This line also ensures object reference right until this point.
            obj = null;
            System.GC.Collect();

            monitor.UpdateState();
            Assert.AreEqual(ObjectMonitor.State.TargetVoid, monitor.CurrentState);

            //UsingClass user = new UsingClass();

            //MonitorClass monitor = new MonitorClass(new MyClass());

        }

        private void Obj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
    }
}
