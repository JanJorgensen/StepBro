using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Execution;

namespace StepBro.Core.Data
{
    public interface IProcedureOutput
    {
        ExecutionFailure Failure { get; }
        Verdict ProcedureVerdict { get; }
        Type ReturnValueType { get; }
        bool HasReturnValue { get; }
        object ReturnValue { get; }
    }

    public interface IProcedureResult<T> : IProcedureOutput
    {
        T ReturnValueTyped { get; }
    }

    //internal class 

    internal sealed class ProcedureResultVoid : IProcedureOutput
    {
        static public IProcedureOutput Create(ExecutionFailure failure = null, Verdict verdict = Verdict.Unset)
        {
            return new ProcedureResultVoid();
        }

        public ExecutionFailure Failure
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool HasReturnValue
        {
            get
            {
                return false;
            }
        }

        public Verdict ProcedureVerdict
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object ReturnValue
        {
            get
            {
                throw new NotSupportedException("There is no return value on the procedure.");
            }
        }

        public Type ReturnValueType
        {
            get
            {
                return typeof(void);
            }
        }
    }

    internal sealed class ProcedureResult<T> : IProcedureResult<T>
    {
        private T m_value;
        static public ProcedureResult<T> Create(T value)
        {
            return new ProcedureResult<T>(value);
        }

        private ProcedureResult(T value)
        {
            m_value = value;
        }

        public bool HasReturnValue
        {
            get
            {
                return true;
            }
        }

        public Verdict ProcedureVerdict
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type ReturnValueType
        {
            get
            {
                return typeof(T);
            }
        }

        public object ReturnValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public T ReturnValueTyped
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ExecutionFailure Failure
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
