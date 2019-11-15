using Perf.Core.Entities;
using Perf.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Perf.Core
{
    public abstract class PerfTest
    {

        #region Attributes
        protected readonly Options o;
        protected readonly PerfReport rpt = new PerfReport();
        protected DateTime startTime;
        protected TimeSpan totalTime;
        protected List<Listing> recs;
        private int runId = 1;
        private const string ok = "[ OK ]";
        private const string failed = "[ FAILED ]";
        private const string plus = "[+]";
        #endregion

        #region ctor
        public PerfTest(Options o)
        {
            this.o = o;
        }
        #endregion

        abstract protected void Connect();
        abstract protected void Disconnect();
        abstract protected void CreateDb();
        abstract protected void DropDb();
        abstract protected void RestoreDb();
        abstract protected void ImportDump();
        abstract protected void ExportDump();
        abstract protected void Insert();
        abstract protected void Update();
        abstract protected void Delete();
        abstract protected void Query();
        abstract protected void BulkInsert();
        abstract protected void BulkDelete();

        private void RunTasks(List<Action> tasks)
        {
            Log($"\n[Test #{runId}] Performing tests for [{string.Join(", ", tasks.Select(t => t.Method.Name))}]: ", 0, true);
            tasks.ForEach(t =>
            {
                var e = false;
                try
                {
                    var m = t.Method.Name;
                    StartRun($"Running '{m}'...");
                    t();
                }
                catch (Exception ex)
                {
                    Log(ex);
                    e = true;
                }
                finally
                {
                    EndRun(t.Method.Name, e);
                }
            });

            runId++;
        }

        public void RunTests()
        {
            Init();

            RunTasks(new List<Action> {
                new Action(Connect),
                new Action(CreateDb),
                new Action(ImportDump),
                new Action(ExportDump),
                new Action(DropDb),
                new Action(Disconnect),
            });

            RunTasks(new List<Action> {
                new Action(Connect),
                new Action(RestoreDb),
                new Action(DropDb),
                new Action(Disconnect),
            });

            RunTasks(new List<Action> {
                new Action(Connect),
                new Action(CreateDb),
                new Action(Insert),
                new Action(Query),
                new Action(Delete),
                new Action(DropDb),
                new Action(Disconnect),
            });

            RunTasks(new List<Action> {
                new Action(Connect),
                new Action(CreateDb),
                new Action(BulkInsert),
                new Action(BulkDelete),
                new Action(DropDb),
                new Action(Disconnect),
            });

            Report();
        }

        private void Init()
        {
            Log("Initializing test data...");
            GenContent();
        }

        protected void Report()
        {
            Header("Test Report");

            Log("Environment Setup:");
            Log(o.ServerSpec(), 2);            

            Log($"\nHere's the output for tests on {o.Op}:");
            var runId = -1;
            rpt.Metrics.ForEach(m =>
            {
                if (runId != m.RunId)
                {
                    Log($"\n  Test #{m.RunId}:", 2);
                    runId = m.RunId;
                }

                Log(m.ToString(), 4);
            });

            Stats();
        }

        protected void Stats()
        {
            Header("Stats (avg):");
            Log($"Sample Size   : {o.Sample:N0} records", 2);
            Log($"Database Size : {o.DbSize:N0} records\n", 2);

            var metrics = from m in rpt.Metrics
                          group m by m.Op into g
                          select new {
                              Op = g.Key,
                              AvgTime = new TimeSpan((long)g.Average(m => m.TotalTime.Ticks))
                          };

            metrics.ToList().ForEach(m =>
            {
                var rps = RecsPerSec(m.Op, m.AvgTime);
                Log(string.Format(
                    "{0}: {1:000.0000} seconds{2}",
                    m.Op.PadRight(14),
                    m.AvgTime.TotalSeconds,
                    rps.HasValue ? $" [{rps:N0} recs/sec]" : null)
                , 2);

            });

            Log("\n");
        }

        private double? RecsPerSec(string key, TimeSpan totalTime)
        {
            switch (key)
            {
                case "ImportDump":
                case "ExportDump":
                case "RestoreDb":
                    return o.DbSize / totalTime.TotalSeconds;

                case "Insert":
                case "Query":
                case "Delete":
                case "BulkInsert":
                case "BulkDelete":
                    return o.Sample / totalTime.TotalSeconds;

                default:
                    return null;
            }
        }

        private void GenContent()
        {
            Logg($"Generating {o.Sample:N0} random records, please wait... ");
            recs = ContentGenerator.CreateListings(o.Sample);
            Ok();
        }

        #region Logs
        protected void Log(string log, int padding = 0, bool highlight = false)
        {
            if (highlight) Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{new string(' ', padding)}{log}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        protected void Log(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.WriteLine(e);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        protected void Logg(string log, int padding = 2, string prefix = plus)
        {
            Console.Write($"{new string(' ', padding)}{prefix} {log}");
        }

        protected void Ok()
        {
            Console.WriteLine(ok);
        }

        protected void Header(string msg)
        {
            Log("\n");
            Bar();
            Log(msg);
            Bar();
        }

        protected static void Bar()
        {
            Console.WriteLine("----------------------------------");
        }

        protected void StartRun(string msg)
        {
            Logg($"{msg}\n");
            startTime = DateTime.Now;
        }

        protected void EndRun(string op, bool err = false)
        {
            rpt.AddMetric(runId, op, DateTime.Now - startTime, err);
        }

        protected void Retry(Action action, int retries = 3, int interval = 300)
        {
            var i = 0;
            while (i < retries)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    Log($"Operation failed, retrying in {interval} ms...", 4);
                    i++;
                    Thread.Sleep(interval);
                }
            }
        }
        #endregion

    }
}
