using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;
using StepBro.Core.Execution;

namespace TestModule
{
    public interface IInterfaceForClassWithInterface : IDisposable
    {
        void SetupA([Implicit] ICallContext context);
        bool SetupDone { get; }
    }

    public class TestClassWithInterface : IInterfaceForClassWithInterface
    {
        public static IInterfaceForClassWithInterface CreateInstanceInterface()
        {
            return new TestClassWithInterface() as IInterfaceForClassWithInterface;
        }

        public bool SetupDone { get; set; }

        public void Dispose()
        {
        }

        public void SetupA([Implicit] ICallContext context)
        {
            this.SetupDone = true;
        }
    }
}
