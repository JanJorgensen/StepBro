using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public delegate TResult TaskDelegateWithControl<TResult>(ITaskControl control, ITaskContext context);

    //public delegate void TaskDelegateWithJustContext(ITaskContext context); // Hmm
}
