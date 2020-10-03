using ModuleTestSupport;
using StepBro.CAN;
using System;

namespace TestPeakCANPlugin
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ConsoleCallContext context = new ConsoleCallContext();

            var adapter = PCAN.Driver.GetAdapter(context, "USBBUS1");
            var channel = adapter.GetChannel(context, 0);
            channel.Setup(context, Baudrate.BR500K, ChannelMode.Extended);
            context.Logger.Log(null, "Channel initial open: " + channel.IsOpen);
            if (!channel.IsOpen)
            {
                context.Logger.Log(null, "Opening");
                channel.Open(context);
                context.Logger.Log(null, "Opened");
            }


            context.Logger.Log(null, "<THE END>");
            System.Threading.Thread.Sleep(2000);
        }
    }
}
