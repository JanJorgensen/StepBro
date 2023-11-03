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
            // If we have a client session, we write to it
            if (_session != null)
            {
                string message = String.Format(format, data);
                _session.SendEvent(new Event(message));
            }

            // When we log we both want to write to the client and to here
            // to ensure both are in sync.
            Console.WriteLine(format, data);
        }
    }
}