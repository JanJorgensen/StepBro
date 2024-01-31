using System.Collections.Generic;
using System;
using System.IO;

namespace StepBroCreateProfilingScripts
{
    internal class Program
    {
        static string path = "";

        static void Main(string[] args)
        {
            using (var fs = Create("ManyTools.sbs"))
            {
                const int toolCount = 1000;
                var portName = new Func<int, string>(a => "port" + StepBro.Core.Data.AlphaID.Create(a, 4));
                var connectionName = new Func<int, string>(a => "harness" + StepBro.Core.Data.AlphaID.Create(a, 4));

                fs.WriteLine("using StepBro.Streams;");
                fs.WriteLine("using StepBro.TestInterface;");

                fs.WriteLine("type HarnessConnection : SerialTestConnection;");

                for (int i = 0; i < toolCount; i++)
                {
                    fs.WriteLine($"public SerialPort {portName(i)} = SerialPort() {{ PortName: \"Com4\", BaudRate = 460800, StopBits = One }}");
                    fs.WriteLine($"public HarnessConnection {connectionName(i)} = SerialTestConnection() {{ Stream = {portName(i)}, CommandResponseTimeout = 20s,");
                    fs.WriteLine("    commands:[{\"Help|Get Help\": \"help\"},{ \"Help|About\": \"about\"},{ \"List|Commands\": \"list commands\"},{ \"List|Objects\": \"list objects\"},{ \"LED\": \"led 2\"}]}");
                }
            }

            using (var fs = Create("BigCallTree.sbs"))
            {
                const int l2width = 100;
                const int l3width = 100;

                var secondlevel = new Func<int, string>(a => "ProcL2" + StepBro.Core.Data.AlphaID.Create(a, 4));
                var thirdlevel = new Func<int, string>(a => "ProcL3" + StepBro.Core.Data.AlphaID.Create(a, 4));

                for (int i = 0; i < l3width; i++)
                {
                    fs.WriteLine($"procedure void {thirdlevel(i)}()");
                    fs.WriteLine("{");
                    fs.WriteLine($"    log (\"Here in ProcL3{thirdlevel(i)}\");");
                    fs.WriteLine("}");
                }
                for (int i = 0; i < l2width; i++)
                {
                    fs.WriteLine($"procedure void {secondlevel(i)}()");
                    fs.WriteLine("{");
                    fs.WriteLine($"    log (\"Here in ProcL3{secondlevel(i)}\");");
                    for (int j = 0; j < l3width; j++)
                    {
                        fs.WriteLine($"    {thirdlevel(j)}();");
                    }
                    fs.WriteLine("}");
                }
                fs.WriteLine($"procedure void Main()");
                fs.WriteLine("{");
                fs.WriteLine($"    log (\"Here in Main\");");
                for (int i = 0; i < l2width; i++)
                {
                    fs.WriteLine($"    {secondlevel(i)}();");
                }
                fs.WriteLine("}");
            }
        }

        static StreamWriter Create(string name)
        {
            string filepath = Path.Combine(path, name);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            return new StreamWriter(filepath);
        }
    }
}