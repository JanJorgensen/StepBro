﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestConceptualFileImplementation
    {
        [TestMethod]
        public void TestConceptExecutionSimple()
        {
            MiniLogger.Instance.Clear();
            var file = new ConceptualFileImplementation();
            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(ConceptualFileImplementation.MyFirst, TimeSpan.FromSeconds(5));

            Assert.AreEqual(37L, result);
        }


        [TestMethod]
        public void TestConceptExecutionLoggingCheckEmpty()
        {
            MiniLogger.Instance.Clear();
            var file = new ConceptualFileImplementation();
            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(ConceptualFileImplementation.MyEmpty);
            var log = new LogInspector(taskContext.Logger);

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyEmpty - <no arguments>");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestConceptExecutionLoggingCheckNormal()
        {
            MiniLogger.Instance.Clear();
            var file = new ConceptualFileImplementation();
            var taskContext = ExecutionHelper.ExeContext(false);
            taskContext.CallProcedure(ConceptualFileImplementation.ProcLog1);
            var log = new LogInspector(taskContext.Logger);

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - ProcLog1 - <no arguments>");
            log.ExpectNext("2 - Normal - 0 - Step 1.1 Normal");

            #region Call Log2 procedures with ForceAlways
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: ForceAlways");

            #region Call Log2ForceAlways with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with Normal
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: Normal");

            #region Call Log2ForceAlways with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with DebugOnly
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: DebugOnly");

            #region Call Log2ForceAlways with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with Disabled
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: Disabled");

            #region Call Log2ForceAlways with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            log.ExpectNext("2 - Post");
            log.ExpectEnd();

        }

        [TestMethod]
        public void TestConceptExecutionLoggingCheckDebugging()
        {
            MiniLogger.Instance.Clear();
            var file = new ConceptualFileImplementation();
            var taskContext = ExecutionHelper.ExeContext();
            //taskContext.Logger.SetBreaker(e => e.Id == 430);
            taskContext.CallProcedure(ConceptualFileImplementation.ProcLog1);
            var log = new LogInspector(taskContext.Logger);

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - ProcLog1 - <no arguments>");
            log.ExpectNext("2 - Normal - 0 - Step 1.1 Normal");
            log.ExpectNext("2 - Normal - 0 - Step 1.2 Debugging");

            #region Call Log2 procedures with ForceAlways
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: ForceAlways");

            #region Call Log2ForceAlways with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with ForceAlways
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with Normal
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: Normal");

            #region Call Log2ForceAlways with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with Normal
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with DebugOnly
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: DebugOnly");

            #region Call Log2ForceAlways with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with DebugOnly
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            #region Call Log2 procedures with Disabled
            log.ExpectNext("2 - Normal - 0 - Step 1.3 Mode: Disabled");

            #region Call Log2ForceAlways with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2ForceAlways - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Normal with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2Normal - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2DebugOnly with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2DebugOnly - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #region Call Log2Disabled with Disabled
            log.ExpectNext("2 - Pre - 0 ProcLog2Disabled - <no arguments>");

            #region Call Log 3 with ForceAlways
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: ForceAlways");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Normal - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3DebugOnly - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - 0 ProcLog3Disabled - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Normal
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Normal");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with DebugOnly
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: DebugOnly");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            #region Call Log 3 with Disabled
            log.ExpectNext("3 - Normal - 0 - Step 2.3 Mode: Disabled");
            log.ExpectNext("3 - Pre - 0 ProcLog3ForceAlways - <no arguments>");
            log.ExpectNext("4 - Normal - 0 - Step 3.1 Normal");
            log.ExpectNext("4 - Normal - 0 - Step 3.2 Debugging");
            log.ExpectNext("4 - Post");
            #endregion

            log.ExpectNext("3 - Post");
            #endregion

            #endregion

            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestConceptExecutionLoggingCheck_JustForDebugging()
        {
            MiniLogger.Instance.Clear();
            var file = new ConceptualFileImplementation();
            var taskContext = ExecutionHelper.ExeContext();
            //taskContext.Logger.SetBreaker(e => e.Id == 430);
            taskContext.CallProcedure(ConceptualFileImplementation.ProcL1);
            var log = new LogInspector(taskContext.Logger);

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - ProcL1 - <no arguments>");
            log.ExpectNext("2 - Normal - 0 - Step 1.1 Normal");
            log.ExpectNext("2 - Normal - 0 - Step 1.2 Debugging");
            log.ExpectNext("2 - Pre - 0 ProcL2 - <no arguments>");
            log.ExpectNext("3 - Normal - 0 - Step 2.1 Normal");
            log.ExpectNext("3 - Normal - 0 - Step 2.2 Debugging");
            log.ExpectNext("3 - Pre - 0 ProcL3 - <no arguments>");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

    }
}
