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
                    StepBro.Core.Main.GetService<ILogger>(),
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
                while ((DateTime.Now - execution.Task.StartTime) < TimeSpan.FromMilliseconds(10000))
                {
                    var state = execution.Task.CurrentState;
                    if (state > StepBro.Core.Tasks.TaskExecutionState.Running)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(1);
                }
                System.Diagnostics.Debug.WriteLine($"Execution time: {(execution.Task.EndTime - execution.Task.StartTime).ToString()}");
                Assert.AreEqual(StepBro.Core.Tasks.TaskExecutionState.Ended, execution.Task.CurrentState);

                var log = new LogInspector((ServiceManager.Global.Get<ILogger>() as LoggerScope).Logger);
                log.DebugDump();

                log.ExpectNext("0 - Pre - StepBro - Main logger created");
                //log.ExpectNext("1 - Normal - MainLogger - Service started");
                log.ExpectNext("1 - System - StepBro - Successfully read the station properties file.");
                log.ExpectNext("1 - Pre - Script Execution - Main");
                log.ExpectNext("2 - Pre - myfile.Main - <no arguments>");
                log.ExpectNext("3 - Normal - 3 Log - Started");
                log.ExpectNext("3 - Normal - 4 delay - 100ms");
                log.ExpectNext("3 - Normal - 5 Log - Ending");
                log.ExpectNext("3 - Post");
                log.ExpectNext("2 - Post - Script execution ended. Succes.");
                log.ExpectEnd();
            }
            finally
            {
                StepBro.Core.Main.DeinitializeInternal(true);
            }
        }
    }
}