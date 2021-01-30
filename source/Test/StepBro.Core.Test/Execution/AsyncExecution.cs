using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;
using System;
using System.Linq;
using System.Text;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class AsyncExecution
    {
        [TestMethod]
        public void AsyncExecutionSimple()
        {
            var f = new StringBuilder();
            f.AppendLine("procedure void Main()");
            f.AppendLine("{");
            f.AppendLine("   log (\"Started\");");
            f.AppendLine("   delay (100ms);     // Give time for test to detect state 'Running'");
            f.AppendLine("   log (\"Ending\");");
            f.AppendLine("}");

            StepBro.Core.Main.Initialize();
            try
            {
                var file = FileBuilder.ParseFiles(
                    StepBro.Core.Main.ServiceManager,
                    StepBro.Core.Main.GetService<IMainLogger>().Logger.RootLogger,
                    new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()))[0];

                Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
                var procedure = file.ListElements().First(p => p.Name == "Main") as IFileProcedure;

                var execution = StepBro.Core.Main.StartProcedureExecution(procedure);
                bool isRunning = false;
                while (execution.Task.CurrentState == StepBro.Core.Tasks.TaskExecutionState.Created) ;
                while (execution.Task.CurrentState == StepBro.Core.Tasks.TaskExecutionState.Started) ;
                while ((DateTime.Now - execution.Task.StartTime) < TimeSpan.FromMilliseconds(5000))
                {
                    if (execution.Task.CurrentState == StepBro.Core.Tasks.TaskExecutionState.Running)
                    {
                        isRunning = true;
                        break;
                    }
                }
                Assert.IsTrue(isRunning);
                while ((DateTime.Now - execution.Task.StartTime) < TimeSpan.FromMilliseconds(5000))
                {
                    if (execution.Task.CurrentState > StepBro.Core.Tasks.TaskExecutionState.Running) break;
                }
                Assert.AreEqual(StepBro.Core.Tasks.TaskExecutionState.Ended, execution.Task.CurrentState);

                var log = new LogInspector(ServiceManager.Global.Get<IMainLogger>().Logger);
                log.DebugDump();

                log.ExpectNext("0 - Pre - StepBro - Main logger created");
                log.ExpectNext("1 - Normal - MainLogger - Service started");
                log.ExpectNext("1 - Pre - Script Execution - Main");
                log.ExpectNext("1 - Pre - myfile.Main - <arguments>");
                log.ExpectNext("2 - Normal - 3 - log: Started");
                log.ExpectNext("2 - Normal - 5 - log: Ending");
                log.ExpectNext("2 - Post");
                log.ExpectNext("2 - Post - Script Execution - Script execution ended. Result value: <null>");
                log.ExpectEnd();
            }
            finally
            {
                StepBro.Core.Main.DeinitializeInternal(true);
            }
        }
    }
}