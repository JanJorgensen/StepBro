using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StepBroCoreTest
{
    internal class ConceptFileBase : IScriptFile
    {
        public string Author
        {
            get
            {
                return "Jan";
            }
        }

        public IScriptFile FileInfo
        {
            get
            {
                return this;
            }
        }

        public string FileName
        {
            get
            {
                return "ConceptualFileImplementation.cs";
            }
        }

        public string FilePath
        {
            get
            {
                return @"c:\temp";
            }
        }

        public string FileRevision
        {
            get
            {
                return "0.1.0";
            }
        }

        public LoadedFileType Type => throw new NotImplementedException();

        public int UniqueID => throw new NotImplementedException();

        public string OffDiskFileContent { get; set; }

        public int RegisteredDependantsCount => throw new NotImplementedException();

        public string Namespace => throw new NotImplementedException();

        public IErrorCollector Errors => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }
        public event EventHandler ObjectContainerListChanged { add { } remove { } }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsDependantOf(object @object)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListDependantDescriptors()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFileElement> ListElements()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IObjectContainer> ListObjectContainers()
        {
            throw new NotImplementedException();
        }

        public bool RegisterDependant(object usingObject)
        {
            throw new NotImplementedException();
        }

        public bool UnregisterDependant(object usingObject, bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConceptualFileImplementation : ConceptFileBase
    {
        public static ConceptFileBase m_file = new ConceptFileBase();

        public delegate void ProcedureDelegateVoidVoid(IScriptCallContext context);

        public static IProcedureReference MyEmpty = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "MyEmpty", ContextLogOption.Normal, MyEmptyProcedure);
        public static void MyEmptyProcedure(IScriptCallContext context)
        {
        }

        #region Simple Calling

        public delegate long MyFirstProcedureDelegate(IScriptCallContext context, TimeSpan time);
        public delegate void MySecondProcedureDelegate(IScriptCallContext context, long a, string b);
        public delegate void MyThirdProcedureDelegate(IScriptCallContext context, bool b1);
        public delegate double ProcedureDelegateDoubleDouble(IScriptCallContext context, double v);

        public static IProcedureReference MyFirst = FileProcedure.Create<MyFirstProcedureDelegate>(m_file, "", "MyFirst", ContextLogOption.Normal, MyFirstProcedure);
        public static IProcedureReference MySecond = FileProcedure.Create<MySecondProcedureDelegate>(m_file, "", "MySecond", ContextLogOption.Normal, MySecondProcedure);
        public static IProcedureReference MyThird = FileProcedure.Create<MyThirdProcedureDelegate>(m_file, "", "MyThird", ContextLogOption.DebugOnly, MyThirdProcedure);
        public static IProcedureReference SquareRoot = FileProcedure.Create<ProcedureDelegateDoubleDouble>(m_file, "", "SquareRoot", ContextLogOption.DebugOnly, SquareRootProcedure);

        public static long MyFirstProcedure(IScriptCallContext context, TimeSpan time)
        {
            //context.Setup(MyFirst, ContextLogOption.Normal, true);
            long v1 = 17L;
            string v2 = "Mette";

            context.Logger.Log("Step 1");

            using (var callcontext = context.EnterNewScriptContext(MySecond, ContextLogOption.Normal, false, null).Disposer())
            {
                MySecondProcedure(callcontext.Value, v1, v2);
            }

            context.Logger.Log("Step 2");

            using (var loopStatus = context.StatusUpdater.CreateProgressReporter("Loop", TimeSpan.FromSeconds(20), 10L, p => String.Format("Iteration #{0}", p + 1L)))
            {
                for (long i = 0L; i < 10L; i++)
                {
                    context.Logger.Log(String.Format("Step 3 Loop iteration #{0}", i + 1));
                    loopStatus.UpdateStatus(progress: i + 1L);
                    using (var callcontext = context.EnterNewScriptContext(MyThird, ContextLogOption.Normal, false, null).Disposer())
                    {
                        MyThirdProcedure(callcontext.Value, false);
                        v1 += 2;
                    }
                }
            }

            return v1;
        }

        public static void MySecondProcedure(IScriptCallContext context, long a, string b)
        {
            //context.Setup(MySecond, ContextLogOption.Normal, true);
            context.Logger.Log("Here I am, in the middle of MySecondProcedure");
        }

        public static void MyThirdProcedure(IScriptCallContext context, bool b1)
        {
            //context.Setup(MyThird, ContextLogOption.DebugOnly, true);
            context.Logger.Log("Here I am, in the middle of MyThirdProcedure");
        }

        public static double SquareRootProcedure(IScriptCallContext context, double v)
        {
            //context.Setup(SquareRoot, ContextLogOption.DebugOnly, true);
            return System.Math.Sqrt(v);
        }

        #endregion

        #region Logging Test

        public static IProcedureReference ProcLog1 = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog1", ContextLogOption.Normal, ProcLog1Procedure);
        public static IProcedureReference ProcLog2ForceAlways = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog2ForceAlways", ContextLogOption.ForceAlways, ProcLog2Procedure);
        public static IProcedureReference ProcLog2Normal = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog2Normal", ContextLogOption.Normal, ProcLog2Procedure);
        public static IProcedureReference ProcLog2DebugOnly = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog2DebugOnly", ContextLogOption.DebugOnly, ProcLog2Procedure);
        public static IProcedureReference ProcLog2Disabled = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog2Disabled", ContextLogOption.Disabled, ProcLog2Procedure);
        public static IProcedureReference ProcLog3ForceAlways = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog3ForceAlways", ContextLogOption.ForceAlways, ProcLog3Procedure);
        public static IProcedureReference ProcLog3Normal = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog3Normal", ContextLogOption.Normal, ProcLog3Procedure);
        public static IProcedureReference ProcLog3DebugOnly = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog3DebugOnly", ContextLogOption.DebugOnly, ProcLog3Procedure);
        public static IProcedureReference ProcLog3Disabled = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcLog3Disabled", ContextLogOption.Disabled, ProcLog3Procedure);


        // ForceAlways, Normal, DebugOnly, Disabled
        public static void ProcLog1Procedure(IScriptCallContext context)
        {
            //context.Setup(ProcLog1, ContextLogOption.Normal, true);
            if (context.LoggingEnabled) context.Logger.Log("Step 1.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 1.2 Debugging");
            foreach (ContextLogOption lo in Enum.GetValues(typeof(ContextLogOption)))
            {
                context.Logger.Log("Step 1.3 Mode: " + lo.ToString());
                using (var callcontext = context.EnterNewScriptContext(ProcLog2ForceAlways, lo, false, null).Disposer())
                {
                    ProcLog2Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog2Normal, lo, false, null).Disposer())
                {
                    ProcLog2Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog2DebugOnly, lo, false, null).Disposer())
                {
                    ProcLog2Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog2Disabled, lo, false, null).Disposer())
                {
                    ProcLog2Procedure(callcontext.Value);
                }
            }
        }

        public static void ProcLog2Procedure(IScriptCallContext context)
        {
            //context.Setup(ProcLog2ForceAlways, ContextLogOption.ForceAlways, true);
            if (context.LoggingEnabled) context.Logger.Log("Step 2.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 2.2 Debugging");
            foreach (ContextLogOption lo in Enum.GetValues(typeof(ContextLogOption)))
            {
                context.Logger.Log("Step 2.3 Mode: " + lo.ToString());
                using (var callcontext = context.EnterNewScriptContext(ProcLog3ForceAlways, lo, false, null).Disposer())
                {
                    ProcLog3Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog3Normal, lo, false, null).Disposer())
                {
                    ProcLog3Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog3DebugOnly, lo, false, null).Disposer())
                {
                    ProcLog3Procedure(callcontext.Value);
                }
                using (var callcontext = context.EnterNewScriptContext(ProcLog3Disabled, lo, false, null).Disposer())
                {
                    ProcLog3Procedure(callcontext.Value);
                }
            }
        }

        public static void ProcLog3Procedure(IScriptCallContext context)
        {
            if (context.LoggingEnabled) context.Logger.Log("Step 3.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 3.2 Debugging");
        }

        public static IFileProcedure ProcL1 = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcL1", ContextLogOption.Normal, ProcL1Procedure).ProcedureData;
        public static IFileProcedure ProcL2 = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcL2", ContextLogOption.Normal, ProcL2Procedure).ProcedureData;
        public static IFileProcedure ProcL3 = FileProcedure.Create<ProcedureDelegateVoidVoid>(m_file, "", "ProcL3", ContextLogOption.Normal, ProcL3Procedure).ProcedureData;

        public static void ProcL1Procedure(IScriptCallContext context)
        {
            if (context.LoggingEnabled) context.Logger.Log("Step 1.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 1.2 Debugging");
            using (var callcontext = context.EnterNewScriptContext(ProcL2, ContextLogOption.Normal, false, null).Disposer())
            {
                ProcL2Procedure(callcontext.Value);
            }
        }
        public static void ProcL2Procedure(IScriptCallContext context)
        {
            if (context.LoggingEnabled) context.Logger.Log("Step 2.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 2.2 Debugging");
            using (var callcontext = context.EnterNewScriptContext(ProcL3, ContextLogOption.Disabled, false, null).Disposer())
            {
                ProcL3Procedure(callcontext.Value);
            }
        }
        public static void ProcL3Procedure(IScriptCallContext context)
        {
            if (context.LoggingEnabled) context.Logger.Log("Step 3.1 Normal");
            if (context.LoggingEnabled && context.Logger.IsDebugging) context.Logger.Log("Step 3.2 Debugging");
        }

        #endregion
    }
}
