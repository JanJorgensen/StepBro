using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.StateMachine
{
    public enum Event
    {
        Enter = 0,
        Exit,
        PollTimer,
        Timer,
        ExternalEvent
    }
}
