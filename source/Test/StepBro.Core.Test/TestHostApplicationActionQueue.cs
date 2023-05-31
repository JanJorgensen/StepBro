using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestHostApplicationActionQueue
    {
        private IHostApplicationActionQueue queue = null;
        private int m_testVariable = 0;

        [TestInitialize]
        public void Setup()
        {
            var servicesAdmin = ServiceManager.Create();
            var services = servicesAdmin.Manager;

            var mainLogger = new Logger("", false, "StepBro", "Main logger created in TestHostApplicationActionQueue.Setup");
            services.Register(mainLogger.RootScopeService);
            IService service;
            var queueService = new HostApplicationActionQueue(out service);
            services.Register(service);

            TaskContextDummy taskContext = new TaskContextDummy();
            servicesAdmin.StartServices(taskContext);

            queue = services.Get<IHostApplicationActionQueue>();
        }

        [TestMethod]
        public void AddSingleTask()
        {
            m_testVariable = 0;
            var task = queue.AddTask("TaskDelay100", true, null, this.TaskDelay100);
            Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(72, m_testVariable);
        }

        [TestMethod]
        public void AddTwoTasks()
        {
            m_testVariable = 0;
            var task1 = queue.AddTask("TaskDelay100", true, null, this.TaskDelay100);
            Assert.IsTrue(task1.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(72, m_testVariable);
            var task2 = queue.AddTask("TaskDelay200", true, null, this.TaskDelay200);
            Assert.IsTrue(task2.Wait(TimeSpan.FromSeconds(1)));
            Assert.AreEqual(82, m_testVariable);
        }

        [TestMethod]
        public void AddSeveralTasks()
        {
            m_testVariable = 0;
            var tasksFinished = queue.FinishedTasks;
            for (int i = 0; i < 200; i++)
            {
                queue.AddTask("TaskAdd17", true, null, this.TaskAdd17);
            }
            var taskEnd = queue.AddTask("TaskNOP", true, null, this.TaskNOP);
            Assert.IsTrue(taskEnd.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(3400, m_testVariable);
            tasksFinished = queue.FinishedTasks - tasksFinished;
            Assert.AreEqual(201U, tasksFinished);
        }

        private void TaskDelay100(ITaskContext context)
        {
            System.Threading.Thread.Sleep(100);
            m_testVariable = 72;
        }

        private void TaskDelay200(ITaskContext context)
        {
            System.Threading.Thread.Sleep(200);
            m_testVariable = 82;
        }

        private void TaskAdd17(ITaskContext context)
        {
            System.Threading.Thread.Sleep(1);
            m_testVariable += 17;
        }

        private void TaskNOP(ITaskContext context)
        {
        }
    }
}
