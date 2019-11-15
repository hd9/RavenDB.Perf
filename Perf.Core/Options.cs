using CommandLine;
using CommandLine.Text;
using System.Collections.Generic;
using System.ComponentModel;

namespace Perf.Core
{

    public enum Op
    {
        [Description("Raven 3.5")] Raven3,
        [Description("Raven 4.2 Local")] Raven4,
        [Description("Raven 4.2 Container ")] Raven4Cont,
        [Description("Raven 4.2 Cloud")] Raven4Cloud
    }

    public abstract class Options
    {
        public abstract Op Op { get; }

        [Option(HelpText = "Specify the database name", Required = true)]
        public string Db { get; set; }

        [Option(HelpText = "Specify the Url of your RavenDb db or cluster", Required = true)]
        public string Url { get; set; }

        [Option(HelpText = "Specify the path to the certificate file (optional)")]
        public string Cert { get; set; }

        [Option(HelpText = "The location for the dbdump file")]
        public string Dump { get; set; }

        [Option(HelpText = "The location for the backup folder")]
        public string Backup { get; set; }

        [Option(HelpText = "Specify the # of inserts to perform. Default: 10,000 (optional)")]
        public int Sample { get; set; } = 10000;

        public long DbSize { get; set; }

        public abstract string ServerSpec();
    }

    [Verb("v3", HelpText = "Perf test a local Raven 3.5 db")]
    public class Raven3Options : Options
    {
        public override Op Op { get => Op.Raven3; }

        [Option(HelpText = "The location for the destination database folder")]
        public string DataDir { get; set; }

        [Usage(ApplicationAlias = "ravendb")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Create database", new Raven3Options { Db = "MyDB", Url = "http://127.0.0.1:8082/" });
            }
        }

        public override string ServerSpec()
        {
            return "Server Build #35274, Client Build #3.5.9\n64 bits, 8 Cores, Phys Mem 15.864 GBytes, Arch: X64";
        }
    }

    [Verb("local", HelpText = "Perf test a local Raven 4.2 db")]
    public class Raven4Options : Options
    {
        public override Op Op { get => Op.Raven4; }

        [Usage(ApplicationAlias = "ravendb")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Drop database", new Raven4Options { Db = "MyDB", Url = "http://127.0.0.1:8082/" });
            }
        }

        public override string ServerSpec()
        {
            return "Build 42021, Version 4.2, SemVer 4.2.4, Commit e35f53f\n64 bits, 8 Cores, Phys Mem 15.864 GBytes, Arch: X64";
        }
    }

    [Verb("docker", HelpText = "Perf test a Docker Raven instance ")]
    public class RavenDockerOptions : Options
    {
        public override Op Op { get => Op.Raven4Cont; }

        [Usage(ApplicationAlias = "ravendb")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Import database", new RavenDockerOptions {
                    Db = "MyDB",
                    Url = "http://127.0.0.1:8082/",
                    Dump = @"c:\bkp\raven.dbdump"
                });
            }
        }

        public override string ServerSpec()
        {
            return "Docker Build 42026, Version 4.2, SemVer 4.2.5-patch-42026, Commit 43f3d55\nPID 8, 64 bits, 2 Cores, Phys Mem 1.952 GBytes, Arch: X64";
        }
    }


    [Verb("cloud", HelpText = "Perf test a Raven 4.2 Cloud")]
    public class RavenCloudOptions : Options
    {
        public override Op Op { get => Op.Raven4Cloud; }

        [Usage(ApplicationAlias = "ravendb")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Restore database", new RavenCloudOptions {
                    Db = "MyDB",
                    Url = "http://127.0.0.1:8082/",
                    Dump = @"c:\bkp\folder"
                });
            }
        }

        public override string ServerSpec()
        {
            return "Free Edition: AWS US-east-1, 2 vCPU, 0.5 GB RAM, 10 GB Storage";
        }
    }
}
