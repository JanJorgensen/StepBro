using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestLayeredModuleUsage
    {
        [TestMethod]
        public void ThreeCodeModulesSetup()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(LowLayerCodeModule).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(LowLayerCodeModule).Name + " lowLevelObject");
            f.AppendLine("{");
            f.AppendLine("   IntProp: 12");
            f.AppendLine("}");
            f.AppendLine("public " + typeof(MidLayerCodeModule).Name + " midLevelObject");
            f.AppendLine("{");
            f.AppendLine("   LowLayer: lowLevelObject,");
            f.AppendLine("   IntProp: 2");
            f.AppendLine("}");
            f.AppendLine("public " + typeof(HighLayerCodeModule).Name + " highLevelObject");
            f.AppendLine("{");
            f.AppendLine("   MidLayer: midLevelObject,");
            f.AppendLine("   IntProp: 41");
            f.AppendLine("}");
            f.AppendLine("procedure int UseIt()");
            f.AppendLine("{");
            f.AppendLine("   return highLevelObject.Fcn(4);");
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles(
                (ILogger)null,
                this.GetType().Assembly,
                new Tuple<string, string>("myfile.tss", f.ToString()))[0];

            Assert.AreEqual(1, file.ListElements().Count());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "UseIt") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(1648L, result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            //log.ExpectNext("0 - Pre - TestRun - Starting");
            //log.ExpectNext("1 - Pre - MyFile.ExecuteIt - <arguments>");
            //log.ExpectNext("2 - Normal - 8 - log: Before");
            //log.ExpectNext("2 - Pre - MyFile.Helper - <arguments>");
            //log.ExpectNext("3 - Normal - 5 - log: Doing partner for MyProcedure");
            //log.ExpectNext("3 - Post");
            //log.ExpectNext("2 - Normal - 10 - log: After");
            //log.ExpectNext("2 - Post");
            //log.ExpectEnd();
        }
    }
    public class LowLayerCodeModule : IDisposable
    {
        public LowLayerCodeModule()
        {
        }
        public long IntProp { get; set; } = 0L;
        public void Dispose()
        {
        }
        public long Fcn([Implicit] ICallContext context, long i)    // i = 19
        {
            context.Logger.Log("LowLayerCodeModule.Fcn", $"i: {i}");
            return this.IntProp + i * 63L;      // 12 + 19 * 63 = 1209
        }
    }
    public class MidLayerCodeModule : IDisposable
    {
        public MidLayerCodeModule()
        {
        }
        public LowLayerCodeModule LowLayer { get; set; }
        public long IntProp { get; set; } = 0L;
        public void Dispose()
        {
        }
        public long Fcn([Implicit] ICallContext context, long i)    // i = 8
        {
            context.Logger.Log("MidLayerCodeModule.Fcn", $"i: {i}");
            var low = this.LowLayer.Fcn(context, 19L);
            return low + this.IntProp * 100 + i * 9L;   // low(19) + 2 * 100 + 8 * 9 = 1481
        }
    }
    public class HighLayerCodeModule : IDisposable
    {
        public HighLayerCodeModule()
        {
        }
        public MidLayerCodeModule MidLayer { get; set; }
        public long IntProp { get; set; } = 0L;
        public void Dispose()
        {
        }
        public long Fcn([Implicit] ICallContext context, long i)    // i = 4
        {
            context.Logger.Log("HighLayerCodeModule.Fcn", $"i: {i}");
            var mid = this.MidLayer.Fcn(context, 8L);
            return mid + this.IntProp * 3L + i * 11L;    // mid(8) + 41 * 3 + 4 * 11
        }
    }
}