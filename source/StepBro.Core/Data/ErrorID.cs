namespace StepBro.Core.Data
{
    [DefaultVerdict(Verdict.Error)]
    public abstract class ErrorID
    {
        private Verdict? m_defaultVerdict;

        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public Verdict DefaultVerdict
        {
            get
            {
                if (!m_defaultVerdict.HasValue)
                {
                    m_defaultVerdict = ((DefaultVerdictAttribute)(this.GetType().GetCustomAttributes(typeof(DefaultVerdictAttribute), true)[0])).Value;
                }
                return m_defaultVerdict.Value;
            }
        }
    }

    public class TimeoutError : ErrorID { }
    public class ArgumentError : ErrorID { }
    public class AccessPermissionError : ErrorID { }
    public class RemoteError : ErrorID { }
    public class ObjectNotFoundError : ErrorID { }
    public class DynamicMethodNotFoundError : ErrorID { }
    public class DynamicPropertyNotFoundError : ErrorID { }
}
