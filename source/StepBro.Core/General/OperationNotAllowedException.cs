using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public class OperationNotAllowedException : Exception
    {
        public OperationNotAllowedException(string message) : base(message)
        {
        }
    }
}
