using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using StepBro.Core;
using StepBro.Core.Tasks;
using StepBroCoreTest.Mocks;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestServiceManager
    {
        internal static MiniLogger logger = new MiniLogger();
        [TestMethod]
        public void ServiceCreation()
        {
            logger.Clear();
            IService service;
            var obj = new TestService7(out service);
            Assert.IsNotNull(service);
            Assert.IsNotNull(obj);
            Assert.AreSame(obj, service.ServiceObject);
            Assert.AreEqual(2, service.Dependencies.Count());
        }

        [TestMethod]
        public void OneService()
        {
            logger.Clear();
            IService service;
            var serviceObject = new TestService1(out service);

            // REGISTER
            var manager = ServiceManager.Create();
            manager.Manager.Register(service);

            var deps = manager.Manager.ListUnregisteredDependencies().ToArray();
            Assert.AreEqual(0, deps.Length);

            TestService1 serviceFromManager;
            try
            {
                serviceFromManager = manager.Manager.Get<TestService1>();
                Assert.Fail("Expected exception");
            }
            catch { }

            // START
            manager.StartServices(new TaskContextMock(logger));
            Assert.AreEqual(ServiceManager.ServiceManagerState.Started, manager.Manager.State);
            var walker = new MiniLoggerWalker(logger.First);
            walker.ExpectNext("Starting service TestService1 - Progress: 0");
            walker.ExpectNext("Start TestService1");
            walker.ExpectNext("Started all services - Progress: 1");
            walker.ExpectEnd();

            // ACCESS
            serviceFromManager = manager.Manager.Get<TestService1>();
            Assert.IsNotNull(serviceFromManager);
            Assert.AreSame(serviceFromManager, serviceObject);

            // STOP
            manager.StopServices(new TaskContextMock(logger));
            Assert.AreEqual(ServiceManager.ServiceManagerState.Stopped, manager.Manager.State);
            walker.ContinueAgain();
            walker.ExpectNext("Stopping service TestService1 - Progress: 0");
            walker.ExpectNext("Stop TestService1");
            walker.ExpectNext("Stopped all services - Progress: 1");
            walker.ExpectEnd();
        }

        [TestMethod]
        public void OneServiceWithUnknownDependency()
        {
            logger.Clear();
            IService service;
            var serviceObject = new TestService2(out service);

            // REGISTER
            var manager = ServiceManager.Create();
            manager.Manager.Register(service);

            try
            {
                var serviceFromManager = manager.Manager.Get<TestService1>();
                Assert.Fail("Expected exception");
            }
            catch { }

            var deps = manager.Manager.ListUnregisteredDependencies().ToArray();
            Assert.AreEqual(1, deps.Length);
            Assert.AreEqual(typeof(TestService1), deps[0]);

            try
            {
                manager.StartServices(new TaskContextMock(logger));
                Assert.Fail("Expected exception");
            }
            catch
            {
            }
            Assert.AreEqual(ServiceManager.ServiceManagerState.StartFailed, manager.Manager.State);
            var walker = new MiniLoggerWalker(logger.First);
            walker.ExpectNext("Failed starting services. Some dependencies are missing (TestService1). - Progress: 0");
            walker.ExpectEnd();
        }

        [TestMethod]
        public void SimpleTwoServicesWithDependency()
        {
            logger.Clear();
            var manager = ServiceManager.Create();

            // REGISTER
            IService service;
            var serviceObject1 = new TestService1(out service);
            manager.Manager.Register(service);
            Assert.AreEqual(1, manager.Manager.ListServices().Count());
            var serviceObject2 = new TestService2(out service);
            manager.Manager.Register(service);
            Assert.AreEqual(2, manager.Manager.ListServices().Count());

            var services = manager.Manager.ListServices().ToArray();
            Assert.AreEqual(2, services.Length);

            var deps = manager.Manager.ListUnregisteredDependencies().ToArray();
            Assert.AreEqual(0, deps.Length);

            // START
            manager.StartServices(new TaskContextMock(logger));
            Assert.AreEqual(ServiceManager.ServiceManagerState.Started, manager.Manager.State);
            var walker = new MiniLoggerWalker(logger.First);
            walker.ExpectNext("Starting service TestService1 - Progress: 0");
            walker.ExpectNext("Start TestService1");
            walker.ExpectNext("Starting service TestService2 - Progress: 1");
            walker.ExpectNext("Start TestService2");
            walker.ExpectNext("Started all services - Progress: 2");
            walker.ExpectEnd();

            // ACCESS
            object serviceFromManager = manager.Manager.Get<TestService1>();
            Assert.IsNotNull(serviceFromManager);
            Assert.AreSame(serviceFromManager, serviceObject1);

            serviceFromManager = manager.Manager.Get<TestService2>();
            Assert.IsNotNull(serviceFromManager);
            Assert.AreSame(serviceFromManager, serviceObject2);

            // STOP
            manager.StopServices(new TaskContextMock(logger));
            Assert.AreEqual(ServiceManager.ServiceManagerState.Stopped, manager.Manager.State);
            walker.ContinueAgain();
            walker.ExpectNext("Stopping service TestService2 - Progress: 0");
            walker.ExpectNext("Stop TestService2");
            walker.ExpectNext("Stopping service TestService1 - Progress: 1");
            walker.ExpectNext("Stop TestService1");
            walker.ExpectNext("Stopped all services - Progress: 2");
            walker.ExpectEnd();
        }

        [TestMethod, Ignore]
        // Add tests, where the dependency tree is bigger
        // Add tests, where there are circular dependency references
        // Add test, where two or more services have the same type 
        public void MoreComplicatedServiceManagerTests() { }
    }

    internal class TestServiceBase<TYPE> : ServiceBase<TYPE, TYPE> where TYPE : ServiceBase<TYPE, TYPE>
    {
        public TestServiceBase(string name, out IService service, params Type[] dependencies) : base(name, out service, dependencies) { }
        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            TestServiceManager.logger.Add($"Start {this.GetType().Name}");
        }
        protected override void Stop(ServiceManager manager, ITaskContext context)
        {
            TestServiceManager.logger.Add($"Stop {this.GetType().Name}");
        }
    }
    internal class TestService0 : TestServiceBase<TestService0>
    {
        public TestService0(out IService service) : base("TestService0", out service) { }
    }
    internal class TestService1 : TestServiceBase<TestService1>
    {
        public TestService1(out IService service) : base("TestService1", out service) { }
    }
    internal class TestService2 : TestServiceBase<TestService2>
    {
        public TestService2(out IService service) : base("TestService2", out service, typeof(TestService1)) { }
    }
    internal class TestService3 : TestServiceBase<TestService3>
    {
        public TestService3(out IService service) : base("TestService3", out service, typeof(TestService1)) { }
    }
    internal class TestService4 : TestServiceBase<TestService4>
    {
        public TestService4(out IService service) : base("TestService4", out service, typeof(TestService5)) { }
    }
    internal class TestService5 : TestServiceBase<TestService5>
    {
        public TestService5(out IService service) : base("TestService5", out service, typeof(TestService3), typeof(TestService7)) { }
    }
    internal class TestService6 : TestServiceBase<TestService6>
    {
        public TestService6(out IService service) : base("TestService6", out service, typeof(TestService5), typeof(TestService2)) { }
    }
    internal class TestService7 : TestServiceBase<TestService7>
    {
        public TestService7(out IService service) : base("TestService7", out service, typeof(TestService2), typeof(TestService0)) { }
    }


    internal class TestServiceX1 : ServiceBase<TestServiceX1, TestServiceX1>   // Service dependent on itself.
    {
        public TestServiceX1(out IService service) : base("TestServiceX1", out service, typeof(TestServiceX1)) { }
    }
    internal class TestServiceX2 : ServiceBase<TestServiceX2, TestServiceX2>   // Circular reference: TestServiceX3
    {
        public TestServiceX2(out IService service) : base("TestServiceX2", out service, typeof(TestServiceX3)) { }
    }
    internal class TestServiceX3 : ServiceBase<TestServiceX3, TestServiceX3>   // Circular reference: TestServiceX2
    {
        public TestServiceX3(out IService service) : base("TestServiceX3", out service, typeof(TestServiceX2)) { }
    }
}