using System;
using System.IO;

namespace VSCodeDebug
{
    internal class Program
    {
        static DebugSession _session = null;
        
        private static void Main(string[] argv)
        {
            // stdin/stdout
			Log("waiting for debug protocol on stdin/stdout");
			RunSession(Console.OpenStandardInput(), Console.OpenStandardOutput());
        }

        private static void RunSession(Stream inputStream, Stream outputStream)
		{
			_session = new StepBroDebugSession();
			_session.Start(inputStream, outputStream).Wait();
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
            if (_session != null)
            {
                string message = String.Format(format, data);
                _session.SendEvent(new Event(message));
            }
            else
            {
                Console.WriteLine(format, data);
            }
        }
    }
}