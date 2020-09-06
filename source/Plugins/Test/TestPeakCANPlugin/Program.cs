using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.CAN;
using ModuleTestSupport;
using StepBro.PCAN;

namespace TestPeakCANPlugin
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleCallContext context = new ConsoleCallContext();

            var adapter = PCANInterface.Instance.GetAdapter(context, "USBBUS1");
            var channel = adapter.GetChannel(context, 0);
            channel.Setup(context, CANBaudrate.BR500K, CANChannelMode.Extended, TimeSpan.FromMilliseconds(1500));
            Console.WriteLine("Channel initial open: " + channel.IsOpen);
            if (!channel.IsOpen)
            {
                Console.WriteLine("Opening");
                channel.Open(context);
                Console.WriteLine("Opened");
            }
            System.Threading.Thread.Sleep(2000);
        }
    }
}
