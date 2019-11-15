using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Core
{
    public class StaticSvc
    {

        protected static void Log(string log)
        {
            Console.WriteLine(log);
        }

        protected static void Log(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log("\n");
            Console.WriteLine(e);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        protected static void Logg(string log, int padding = 4)
        {
            Console.Write($"{new string(' ', padding)}- {log}");
        }

        protected static void Ok()
        {
            Console.WriteLine("OK");
        }

        protected static void Bar()
        {
            Console.WriteLine("----------------------------------");
        }

    }
}
