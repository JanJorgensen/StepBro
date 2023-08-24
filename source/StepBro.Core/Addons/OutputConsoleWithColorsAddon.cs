using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputConsoleWithColorsAddon : IOutputFormatterTypeAddon
    {
        static public string Name { get { return "Console"; } }

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Text output in colors on console."; } }

        public OutputType FormatterType { get { return OutputType.Console; } }

        public IOutputFormatter Create()
        {
            return new TextToConsoleFormatter();
        }

        public IOutputFormatter Create(ITextWriter writer)
        {
            throw new NotImplementedException();
        }

        public class TextToConsoleFormatter : IOutputFormatter, ITextWriter
        {
            public TextToConsoleFormatter()
            {
            }

            public void LogEntry(LogEntry entry, DateTime zero)
            {
                var txt = entry.ToClearText(zero, false, false);
                if (txt != null)
                {
                    switch (entry.EntryType)
                    {
                        case Logging.LogEntry.Type.Pre:
                        case Logging.LogEntry.Type.PreHighLevel:
                        case Logging.LogEntry.Type.TaskEntry:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case Logging.LogEntry.Type.Normal:
                        case Logging.LogEntry.Type.Post:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case Logging.LogEntry.Type.Async:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case Logging.LogEntry.Type.Error:
                        case Logging.LogEntry.Type.Failure:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case Logging.LogEntry.Type.UserAction:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case Logging.LogEntry.Type.Detail:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case Logging.LogEntry.Type.System:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }
                    Console.WriteLine(txt);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            void ITextWriter.Write(string text)
            {
                System.Console.Write(text);
            }

            void ITextWriter.WriteLine(string text)
            {
                System.Console.WriteLine(text);
            }
        }
    }
}