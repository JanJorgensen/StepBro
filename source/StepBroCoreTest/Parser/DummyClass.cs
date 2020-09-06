using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Linq;

namespace StepBroCoreTest.Parser
{
    public enum DummyEnum { First = 11, Second = 22, Third = 33 }

    public interface IDummyClass
    {
        long MethodWithCallContextA([Implicit] ICallContext context);
        long MethodWithCallContextB([Implicit] ICallContext context, string s);

    }

    public class DummyClass : IDummyClass, IDisposable
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

        //private static DummyClass g_oneInstance = new DummyClass(928L);
        //static public DummyClass OneInstance { get { return g_oneInstance; } }
        static public DummyClass OneInstance { get { return new DummyClass(928L); } }
        static public IDummyClass OneInstanceInterface { get { return (IDummyClass)(new DummyClass(726L)); } }

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

        public long MethodLongOut1() { return 5001L; }
        public long MethodLongOut2(long a) { return a + 5002L; }
        public long MethodLongOut3(long a = 5L) { return a + 5003L; }
        public long MethodLongOut4(long a, string b, double c) { return a + 5004L; }
        public long MethodLongOut5(long a = 5L, bool b = true, TimeSpan c = default(TimeSpan)) { return a + 5005L; }

        public long MethodWithCallContextA([Implicit] ICallContext context)
        { context.Logger.Log(nameof(MethodWithCallContextA), "740"); return 740L + m_propInt; }
        public long MethodWithCallContextB([Implicit] ICallContext context, string s)
        { context.Logger.Log(nameof(MethodWithCallContextB), s); return s.Length * 14L + m_propInt; }
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

        public static long GenerateNumber(System.Predicate<long> filter)
        {
            for (long i = 123; i < 1000; i++)
            {
                if (filter(i)) return i;
            }
            return -1L;
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
    }
}
