using Raven.Client.Documents;
using Raven.Client.Documents.Commands;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.Backups;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Smuggler;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Perf.Core;
using Perf.Core.Entities;
using Perf.Core.Helpers;

namespace Perf.Raven4.PerfTests
{
    public class Raven4PerfTest : PerfTest
    {
        private IDocumentStore store;

        public Raven4PerfTest(Options o) : base(o)
        {
        }

        protected override void Connect()
        {
            store = new DocumentStore
            {
                Certificate = string.IsNullOrEmpty(o.Cert) ? null : new X509Certificate2(o.Cert),
                Urls = new string[] { o.Url },
                Database = o.Db
            };

            store.Initialize();
        }

        protected override void Disconnect()
        {
            store.Dispose();
        }

        /// <summary>
        /// Creates a Raven.Perf Database
        /// Ref: https://ravendb.net/docs/article-page/4.2/csharp/client-api/operations/server-wide/create-database
        /// </summary>
        protected override void CreateDb()
        {
            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(o.Db)));
        }

        /// <summary>
        /// Imports (Smugglers) Data from a dbdump
        /// </summary>
        protected override void ImportDump()
        {
            var importOp = store.Smuggler.ForDatabase(o.Db)
                .ImportAsync(new DatabaseSmugglerImportOptions {
                    OperateOnTypes = DatabaseItemType.Documents },
                    o.Dump
            );

            importOp.GetAwaiter().GetResult().WaitForCompletion();

            o.DbSize = GetDbSize();
        }

        /// <summary>
        /// Exports (Smugglers) Data from a database to a dbdump
        /// </summary>
        protected override void ExportDump()
        {
            var exportOp = store.Smuggler.ForDatabase(o.Db)
                .ExportAsync(new DatabaseSmugglerExportOptions
                {
                    OperateOnTypes = DatabaseItemType.Documents
                },
                o.Dump.Replace(".", "_export.")
            );

            exportOp.GetAwaiter().GetResult().WaitForCompletion();
        }

        /// <summary>
        /// Restores a RavenDB 4 Backup
        /// </summary>
        protected override void RestoreDb()
        {
            var restoreCfg = new RestoreBackupConfiguration
            {
                DatabaseName = o.Db,
                BackupLocation = o.Backup
            };

            var restoreTask = new RestoreBackupOperation(restoreCfg);
            store.Maintenance.Server.Send(restoreTask).WaitForCompletion();

            o.DbSize = GetDbSize();
        }

        protected override void DropDb()
        {
            Retry(() =>
            {
                store.Maintenance.Server.Send(new DeleteDatabasesOperation(o.Db, true));
            });
        }

        protected override void Insert()
        {
            using (var s = store.OpenSession())
            {
                recs.ForEach(r => s.Store(r));
                s.SaveChanges();
            }
        }

        protected override void Query()
        {
            var count = 0;
            using (var s = store.OpenSession())
            {
                while (true)
                {
                    var docs =
                        s.Query<Listing>()
                        .Skip(count)
                        .Take(1000)
                        .ToList();

                    if (docs.Count == 0)
                        break;

                    count += docs.Count;
                }
            }
        }

        protected override void Delete()
        {
            if (recs == null || recs.Count == 0)
            {
                Log("[ERROR] No recs previously loaded.");
                return;
            }

            using (var s = store.OpenSession())
            {
                recs.ForEach(r => s.Delete(r.Id));
                s.SaveChanges();
            }
        }

        protected override void BulkInsert()
        {
            recs = recs ?? ContentGenerator.CreateListings(o.Sample);
            using (var bulkInsert = store.BulkInsert())
            {
                recs.ForEach(r => bulkInsert.Store(r));
            }
        }

        protected override void BulkDelete()
        {
            try
            {
                var op = store.Operations.Send(new DeleteByQueryOperation("from listings where name != 'YnJ1bm8gaGlsZGVuYnJhbmQK'"));
                op.WaitForCompletion(TimeSpan.FromSeconds(15));
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        protected override void Update()
        {
            throw new NotImplementedException();
        }

        private long GetDbSize()
        {
            var stats = store.Maintenance.Send(new GetStatisticsOperation());
            return stats.CountOfDocuments;
        }
    }
}
