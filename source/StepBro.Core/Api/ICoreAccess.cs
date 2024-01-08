using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public interface ICoreAccess
    {
        /// <summary>
        /// Request starting a script execution of the specified script element.
        /// </summary>
        /// <param name="element">The file element to execute (procedure or testlist).</param>
        /// <param name="model">The model/partner to use for the execution.</param>
        /// <param name="objectVariable">An optional oblect (variable reference) to use as the target for the execution.</param>
        /// <param name="args">The input arguments for the target procedure (or partner).</param>
        /// <returns>Interface to the started execution.</returns>
        IExecutionAccess StartExecution(string element, string model, string objectVariable, object[] args);
        public int ExecutionsRunning { get; }
    }

    public interface IExecutionAccess
    {
        public TaskExecutionState State { get; }
        event EventHandler CurrentStateChanged;
        void RequestStopExecution();
        object ReturnValue { get; }
    }

    public class ExecutionAccessDummy : IExecutionAccess
    {
        public ExecutionAccessDummy(TaskExecutionState state)
        {
            this.State = state;
        }
        public TaskExecutionState State { get; private set; }

        public object ReturnValue
        {
            get { return null; }
        }

        public event EventHandler CurrentStateChanged;

        public void RequestStopExecution()
        {
        }
    }
}
