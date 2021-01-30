using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.General;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestCommandLineParser
    {
        [TestMethod]
        public void CommandLineNoArguments()
        {
            var options = CommandLineParser.Parse<CoreCommandlineOptions>(null, new string[] { });
            Assert.IsFalse(options.HasParsingErrors);
            Assert.IsNull(options.ParsingErrors);
        }

        [TestMethod]
        public void CommandLineScriptFile()
        {
            var options = CommandLineParser.Parse<CoreCommandlineOptions>(null,
                new string[] { @"c:\temp\myscript.sbs" });
            Assert.IsFalse(options.HasParsingErrors);
            Assert.IsNull(options.ParsingErrors);
            Assert.AreEqual(@"c:\temp\myscript.sbs", options.InputFile);
        }

        //[TestMethod]
        //public void CommandLineScriptAndProject()
        //{
        //    var options = CommandLineParser.Parse<CoreCommandlineOptions>(null,
        //        new string[] { "-r", @"c:\temp\myscript.tss", "-p", @"c:\temp\library\standard project.sbs" });
        //    Assert.IsFalse(options.HasParsingErrors);
        //    Assert.IsNull(options.ParsingErrors);
        //    Assert.AreEqual(@"c:\temp\myscript.sbs", options.InputFile);
        //    Assert.AreEqual(@"c:\temp\library\standard project.sbs", options.ProjectFile);
        //}

        [TestMethod]
        public void CommandLineExecuteTarget()
        {
            var options = CommandLineParser.Parse<CoreCommandlineOptions>(null, new string[] { "-e", "hansen" });
            Assert.IsFalse(options.HasParsingErrors);
            Assert.IsNull(options.ParsingErrors);
            Assert.AreEqual("hansen", options.TargetElement);
        }

        [TestMethod]
        public void CommandLineUnknownArguments()
        {
            var options = CommandLineParser.Parse<CoreCommandlineOptions>(null, new string[] { "-g", "mogens" });
            Assert.IsTrue(options.HasParsingErrors);
            Assert.IsNotNull(options.ParsingErrors);
        }

        [TestMethod]
        public void CommandLineMissingFileArgument()
        {
            var options = CommandLineParser.Parse<CoreCommandlineOptions>(null, new string[] { });
            Assert.IsFalse(options.HasParsingErrors);
            Assert.IsNull(options.ParsingErrors);
        }
    }
}
