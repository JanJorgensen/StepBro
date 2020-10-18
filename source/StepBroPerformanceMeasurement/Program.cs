using System;

namespace StepBroPerformanceMeasurement
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Starting " + DateTime.Now.ToLongTimeString());

            for (int i = 0; i < 100; i++)
            {
                var fixture = new StepBroCoreTest.TestProcedure_Expect();
                fixture.TestExpectStatement();
                Console.WriteLine("i: " + i.ToString());
            }

            Console.ReadKey();
            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }
    }
}
