using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBroCoreTest.Data
{
    public enum DummyEnum { First = 11, Second = 22, Third = 33 }

    public class DummyDataClass
    {
        public bool BoolProp { get; set; }
        public int IntProp { get; set; }
        public uint UIntProp { get; set; }
        public string stringProp { get; set; }
    }

    public interface IDummyClass
    {
        long MethodWithCallContextA([Implicit] ICallContext context);
        long MethodWithCallContextB([Implicit] ICallContext context, string s);

    }

    public class DummyClass : IDummyClass, IScriptDisposable
    {
        private long m_propInt;
        private bool m_propBool = false;
        private bool m_disposed = false;
        private int m_propAccessCounter = 0;
        private static DummyClass m_lastCreated = null;

        static public long StaticPropInt { get { return 871; } }
        static public string StaticPropString { get; set; }
        static public TimeSpan StaticPropTimespan { get; set; }
        static public bool StaticPropBool { get; set; } = true;

        static public DummyClass OneInstance { get { return new DummyClass(928L); } }
        static public IDummyClass OneInstanceInterface { get { return (IDummyClass)(new DummyClass(726L)); } }

        public DummyClass() : this(0)
        {
        }

        public DummyClass(long propInt)
        {
            m_propInt = propInt;
            m_createCount++;
            m_lastCreated = this;
        }

        private static long m_createCount = 0;
        private static long m_disposeCount = 0;
        public static long CreateCount { get { return m_createCount; } }
        public static long DisposeCount { get { return m_disposeCount; } }
        public static DummyClass LastCreated { get { return m_lastCreated; } }

        public static void ResetTestData()
        {
            m_createCount = 0;
            m_disposeCount = 0;
            if (m_lastCreated != null)
            {
                m_lastCreated.m_propAccessCounter = 0;
            }
            m_lastCreated = null;
        }

        public static DummyClass Create() { return new DummyClass(10); }

        public DummyClass Self { get { return this; } }
        public StepBro.Core.Data.Verdict Result { get { return Verdict.Inconclusive; } }
        public long PropInt
        {
            get { m_propAccessCounter++; return m_propInt; }
            set { m_propAccessCounter++; m_propInt = value; }
        }
        public bool PropBool
        {
            get { m_propAccessCounter++; return m_propBool; }
            set { m_propAccessCounter++; m_propBool = value; }
        }

        public TimeSpan GetTimespan() { return TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 514L); }

        public bool IsDisposed { get { return m_disposed; } }
        public void Dispose()
        {
            m_disposeCount++;
            m_disposed = true;
            this.PropInt += 1000;
        }

        public event EventHandler Disposing;

        public void Dispose(ICallContext context)
        {
            this.Disposing?.Invoke(this, EventArgs.Empty);
            this.Dispose();
            this.PropInt += 80;
        }

        public Func<long> DelegateLong
        {
            get
            {
                return new Func<long>(() => 21L);
            }
        }


        public static long MethodStaticLongOut1() { return 4001L; }
        public static long MethodStaticLongOut2(long a) { return a + 4002L; }
        public static long MethodStaticLongOut22(long a, long b) { return a + b + 4022L; }
        public static long MethodStaticLongOut3(long a = 5L) { return a + 4003L; }
        public static long MethodStaticLongOut4(long a, string b, double c) { return a + 4004L; }
        public static long MethodStaticLongOut5(long a = 5L, bool b = true, TimeSpan c = default(TimeSpan)) { return a + 4005L; }
        public static long MethodStaticLongOut6(long a, bool b = true, TimeSpan c = default(TimeSpan)) { return a + 4006L; }
        public static long MethodStaticLongOut7(DummyEnum value) { return (long)((int)value) * 7L; }
        public static long MethodStaticLongOut8(object value) { return (value == null) ? -1L : value.ToString().Length; }

        public static long MethodStaticLongSevaralArgs(
            long a = 5L,
            bool b = true,
            TimeSpan c = default(TimeSpan),
            string d = "vink",
            string e = "dab",
            double f = 2.0,
            Verdict g = Verdict.Unset)
        {
            return a * 13 + (b ? 3 : 5) + c.Ticks + d.Length + (long)(f * 1000.0);
        }

        public static IAsyncResult MethodStaticAsyncVoid()
        {
            return System.Threading.Tasks.Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(130);
                });
        }
        public static StepBro.Core.Tasks.IAsyncResult<long> MethodStaticAsyncTyped()
        {
            return new StepBro.Core.Tasks.TaskToAsyncResult<long>(System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.Sleep(110);
                return 12321L;
            }));
        }

        public StepBro.Core.Tasks.IAsyncResult<object> MethodAsyncObject([Implicit] ICallContext context, string s)
        {
            return new StepBro.Core.Tasks.TaskToAsyncResult<object>(System.Threading.Tasks.Task.Run(() =>
            {
                context.Logger.Log("Yup: " + s);
                System.Threading.Thread.Sleep(10);
                return (object)(long)s.Length;
            }));
        }

        public long MethodLongOut1() { return 5001L; }
        public long MethodLongOut2(long a) { return a + 5002L; }
        public long MethodLongOut3(long a = 5L) { return a + 5003L; }
        public long MethodLongOut4(long a, string b, double c) { return a + 5004L; }
        public long MethodLongOut5(long a = 5L, bool b = true, TimeSpan c = default(TimeSpan)) { return a + 5005L; }

        public long MethodWithCallContextA([Implicit] ICallContext context)
        { context.Logger.Log("740"); return 740L + m_propInt; }
        public long MethodWithCallContextB([Implicit] ICallContext context, string s)
        { context.Logger.Log(s); return s.Length * 14L + m_propInt; }
        //spublic IDisposable MethodWithCallContextC([Implicit] ICallContext context) { return 740L; }

        #region Static methods with same name
        //public static long MethodStaticAWithOverload() { return 7001; }
        //public static long MethodStaticAWithOverload(long a) { return 7002 + a; }
        //public static double MethodStaticAWithOverload(double a) { return 7003.4 + a; }
        //public static long MethodStaticAWithOverload(out long a) { a = 5L; return 7004; }
        //public static long MethodStaticAWithOverload(out double a) { a = 3.14; return 7005; }
        //public static long MethodStaticAWithOverload([Implicit] ICallContext context) { return 7006; }
        //public static long MethodStaticAWithOverload([Implicit] ICallContext context, string a) { return 7007; }
        //public static long MethodStaticAWithOverload([Implicit] ICallContext context, long a) { return 7008; }
        //public static long MethodStaticAWithOverload([Implicit] ICallContext context, long a = 7L, string b = "Nix") { return 7009; }   // Called with args: (n), this is the same as the above.
        #endregion

        public static string MethodStaticStringParamsString(string format, params string[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                return "F: " + format + ". Args: " + string.Join(", ", parameters);
            }
            else
            {
                return "F: " + format + ". Args: <none>";
            }
        }

        public static string MethodStaticStringParamsObject(string format, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                return "F: " + format + ". Args: " + string.Join(", ", parameters.Select(o => o.ToString()));
            }
            else
            {
                return "F: " + format + ". Args: <none>";
            }
        }

        public static long GenerateNumber1(Predicate<long> filter)
        {
            for (long i = 123; i < 1000; i++)
            {
                if (filter(i)) return i;
            }
            return -1L;
        }

        public static long GenerateNumber2(long add, Predicate<long> filter)
        {
            for (long i = 123; i < 1000; i++)
            {
                if (filter(i)) return i + add;
            }
            return -1L;
        }

        public static long GenerateNumber3(Predicate<long> filter, long add)
        {
            for (long i = 123; i < 1000; i++)
            {
                if (filter(i)) return i + add;
            }
            return -1L;
        }

        public static long GenerateNumber(Predicate<long> filter)
        {
            return GenerateNumber1(filter);
        }
        public static long GenerateNumber(long add, Predicate<long> filter)
        {
            return GenerateNumber2(add, filter);
        }
        public static long GenerateNumber(Predicate<long> filter, long add)
        {
            return GenerateNumber3(filter, add);
        }

        private static readonly DummyDataClass[] m_dummyDatas = new DummyDataClass[] {
                new DummyDataClass() { BoolProp = true, IntProp = 62, UIntProp = 12u, stringProp = "Anders"},
                new DummyDataClass() { BoolProp = false, IntProp = 9, UIntProp = 125u, stringProp = "Bent"},
                new DummyDataClass() { BoolProp = false, IntProp = -38, UIntProp = 42u, stringProp = "Christian"},
                new DummyDataClass() { BoolProp = true, IntProp = 92, UIntProp = 85u, stringProp = "Dennis" } };

        public static DummyDataClass GetAnObject(Predicate<DummyDataClass> filter)
        {
            foreach (var o in m_dummyDatas)
            {
                if (filter == null || filter(o)) return o;
            }
            return null;
        }

        public static long? MethodNullableIntNull()
        {
            return null;
        }
        public static long? MethodNullableIntValue()
        {
            return 1735L;
        }
        public static string MethodNullableStringNull()
        {
            return null;
        }
        public static string MethodNullableStringValue()
        {
            return "Jenson";
        }

        public static IEnumerable<string> MethodListSomeNames()
        {
            yield return "Anders";
            yield return "Berditto";
            yield return "Chrushtor";
            yield return "Dowfick";
        }

        public static List<string> CreateListOfStrings()
        {
            var list = new List<string>();
            list.AddRange(new string[] { 
                "Anders",
                "Anders",
                "Andres", // ! 
                "Anders",
                "Anders",
                "Anders",
                "Anders",
                "Bent Fabric",
                "Bente Bent",
                "Anders",
                "Anders",
                "Bent Nollerik",
                "Bodil",
                "Anders",
                "Anders",
                "Anders",
                "Anders",
                "Bente Birk",
                "Anders A",
                "Anders B",
                "Anders C",
                "Anders D",
                "Anders E",
                "Anders F",
                "Anders G",
                "Christian",
            });
            return list;
        }
    
        public static object MethodReportingError([Implicit] ICallContext context)
        {
            context.ReportError("<the error description>");
            return null;
        }
        public static object MethodReportingFail([Implicit] ICallContext context)
        {
            context.ReportFailure("<the failure description>");
            return null;
        }

        public static LogLineData CreateLogLineData()
        {
            LogLineData first = new LogLineData(
                null, 
                LogLineData.LogType.Neutral, 
                0, 
                "*Anders", 
                DateTime.Parse("2023-09-26T11:35:00.0000000Z"));

            LogLineData second = new LogLineData(
                first,
                LogLineData.LogType.Neutral,
                1,
                "*Bent",
                DateTime.Parse("2023-09-26T11:36:00.0000000Z"));

            LogLineData third = new LogLineData(
                second,
                LogLineData.LogType.Neutral,
                1,
                "*Christian",
                DateTime.Parse("2023-09-26T11:37:00.0000000Z"));

            LogLineData fourth = new LogLineData(
                third,
                LogLineData.LogType.Neutral,
                1,
                "*Dorte",
                DateTime.Parse("2023-09-26T11:38:00.0000000Z"));

            LogLineData fifth = new LogLineData(
                fourth,
                LogLineData.LogType.Neutral,
                1,
                "*Emil",
                DateTime.Parse("2023-09-26T11:39:00.0000000Z"));

            return first;
        }

        public static LogLineLineReader CreateLogLineLineReader(LogLineData first, object sync)
        {
            return new LogLineLineReader(null, first, sync);
        }
    }
}
