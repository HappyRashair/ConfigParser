using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal static class CWrapper
    {
        private readonly static string OneIndent = new string(' ', 4);
        private readonly static string TwiceIndent = new string(' ', 8);
        private readonly static string ThriceIndent = new string(' ', 12);

        public static void WriteCyan(string message, bool shouldLog = true)
        {
            if(!shouldLog)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{ThriceIndent}{message}");
            Console.ResetColor();
        }

        public static void WriteGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }


        public static void WriteRed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{message}");
            Console.ResetColor();
        }

        public static void WriteYellow(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteIndent(string message)
        {
            Console.WriteLine($"{OneIndent}{message}");
        }
    }
}
