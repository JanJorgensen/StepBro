using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    internal interface IInternalDispose
    {
        /// <summary>
        /// Used internally when exiting a procedure.
        /// </summary>
        void InternalDispose();
    }
}
