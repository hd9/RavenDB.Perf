using CommandLine;
using Perf.Core;
using Perf.Raven4.PerfTests;
using System;

namespace Raven.Perf
{
    public class Program : StaticSvc
    {

        static readonly DateTime startedAt = DateTime.Now;

        // Sample Commands:
        //   # raven4 local
        //   $ perfdb local --db testdb2 --url http://127.0.0.1:8080/ --dump <your.ravendbdump> --backup <your-bkp-folder>
        // 
        //   # raven4 cloud
        //   $ perfdb cloud --db testdb2 --url http://your.ravendb.cloud/ --dump <your.ravendump> --backup <your-bkp-folder>

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
                .ParseArguments<Raven4Options, RavenCloudOptions, RavenDockerOptions>(args)
                .MapResult(
                    (Raven4Options opts) => Run(opts),
                    (RavenCloudOptions opts) => Run(opts),
                    (RavenDockerOptions opts) => Run(opts),
                errs => 1);
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static int Run(Options o)
        {
            new Raven4PerfTest(o).RunTests();
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
