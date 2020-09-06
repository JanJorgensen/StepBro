using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Process;

namespace ProcessModuleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var process = Process.Start(filename: "notepad.exe");
            while (!process.DotNetProcess.Responding)
            {
                System.Threading.Thread.Sleep(100);
                process.DotNetProcess.Refresh();
            }

            process.RequestStop();
            while (process.CurrentState != StepBro.Core.Tasks.TaskExecutionState.Ended)
            {
                System.Threading.Thread.Sleep(100);
                process.DotNetProcess.Refresh();
            }

            System.Threading.Thread.Sleep(5000);

            process.RequestStop();
        }
    }
}
