using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace WindowsEventCollector.ConsoleApp.Helpers
{
    public class WriteLineHelper
    {
        public static void WriteLineToConsole(object message)
        {
            Console.WriteLine($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }

        public static void WriteToConsole(object message)
        {
            Console.Write($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }
    }
}
