using System;
using System.IO;

namespace VSCodeDebug
{
    internal class Program
    {
        private static void Main(string[] argv)
        {
            // stdin/stdout
			Log("waiting for debug protocol on stdin/stdout");
			RunSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        }

        private static void RunSession(Stream inputStream, Stream outputStream)
		{
			DebugSession debugSession = new StepBroDebugSession();
			debugSession.Start(inputStream, outputStream).Wait();
		}

        public static void Log(bool predicate, string format, params object[] data)
		{
			if (predicate)
			{
				Log(format, data);
			}
		}

        public static void Log(string format, params object[] data)
		{
			Console.WriteLine(format, data);
        }
    }
}