using CommandLine;
using Perf.Core;
using Perf.Raven3.PerfTests;
using System;

namespace Raven.Perf
{
    public class Program : StaticSvc
    {

        static readonly DateTime startedAt = DateTime.Now;

        // Sample Command:
        // r3perf v3local --db testdb2 --url http://127.0.0.1:8081/ --dump <dump-location> --backup <backup-location>

        /// <summary>
        /// Starts the app
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                Header("Raven.Perf Spike App");

                new Parser(p => {
                    p.EnableDashDash = false;
                    p.HelpWriter = Console.Out;
                    p.CaseSensitive = true;
                })
                .ParseArguments<Raven3Options>(args)
                .MapResult((Raven3Options opts) => Run(opts), errs => 1);
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static int Run(Options o)
        {
            new Raven3PerfTest(o).RunTests();

            Header($"Total Time: {(DateTime.Now - startedAt).TotalSeconds} seconds");

            return 0;
        }

        private static void Header(string msg)
        {
            Bar();
            Log(msg);
            Bar();
        }


    }
}
