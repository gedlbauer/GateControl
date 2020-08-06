using System;
using System.Collections.Generic;
using System.Text;

namespace GateControl
{
    public static class Cons
    {
        public static void WriteWarning(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("warn");
            Console.ResetColor();
            Console.WriteLine($"] {message}");
        }

        public static void WriteError(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("erro");
            Console.ResetColor();
            Console.WriteLine($"] {message}");
        }

        public static void WriteInfo(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("info");
            Console.ResetColor();
            Console.WriteLine($"] {message}");
        }

        public static void WriteStatus(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("stat");
            Console.ResetColor();
            Console.WriteLine($"] {message}");
        }

        public static void WriteDebug(string message)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("debu");
            Console.ResetColor();
            Console.WriteLine($"] {message}");
        }
    }
}
