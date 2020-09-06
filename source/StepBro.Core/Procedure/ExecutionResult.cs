using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;

namespace StepBro.Core.Procedure
{
    public abstract class ExecutionResult
    {
        protected ExecutionResult()
        {

        }

        /// <summary>
        /// Gets the default verdict for the 
        /// </summary>
        public Verdict DefaultVerdict { get { return Verdict.Unset; } }
    }
}
