using Perf.Core;
using Perf.Core.Entities;
using Perf.Core.Helpers;
using Raven.Abstractions.Data;
using Raven.Abstractions.Smuggler;
using Raven.Client;
using Raven.Client.Document;
using Raven.Smuggler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perf.Raven3.PerfTests
{
    public class Raven3PerfTest : PerfTest
    {

        private IDocumentStore store;

        public Raven3PerfTest(Options o) : base(o)
        {
            
        }

        protected override void Connect()
        {
            store = new DocumentStore { Url = o.Url };
            store.Initialize();
        }

        protected override void Disconnect()
        {
            store.Dispose();
        }

        protected override void CreateDb()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/client-api/commands/how-to/create-delete-database
            store
                .DatabaseCommands
                .GlobalAdmin
                .CreateDatabase(new DatabaseDocument
                {
                    Id = o.Db,
                    Settings =
                    {
                        { "Raven/DataDir", ((Raven3Options)o).DataDir }
                    }
                });

            // The Raven/DataDir setting is mandatory
        }

        protected override void DropDb()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/client-api/commands/how-to/create-delete-database
            Retry(() => {
                store
                    .DatabaseCommands
                    .GlobalAdmin
                    .DeleteDatabase(o.Db, hardDelete: true);
            });
        }

        protected override void ExportDump()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/server/administration/exporting-and-importing-data#smugglerdatabaseapi
            var smugglerApi = new SmugglerDatabaseApi(new SmugglerDatabaseOptions
            {
                OperateOnTypes = ItemType.Documents | ItemType.Indexes | ItemType.Transformers,
                Incremental = false
            });

            var exportOptions = new SmugglerExportOptions<RavenConnectionStringOptions>
            {
                ToFile = o.Dump.Replace(".", "_export."),
                From = new RavenConnectionStringOptions
                {
                    DefaultDatabase = o.Db,
                    Url = o.Url
                }
            };

            smugglerApi.ExportData(exportOptions).GetAwaiter().GetResult();
        }

        protected override void ImportDump()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/server/administration/exporting-and-importing-data#importing
            var smugglerApi = new SmugglerDatabaseApi(new SmugglerDatabaseOptions
            {
                OperateOnTypes = ItemType.Documents,
                Incremental = false
            });

            var importOptions = new SmugglerImportOptions<RavenConnectionStringOptions>
            {
                FromFile = o.Dump,
                To = new RavenConnectionStringOptions
                {
                    DefaultDatabase = o.Db,
                    Url = o.Url                    
                }
            };

            smugglerApi.ImportData(importOptions).GetAwaiter().GetResult();
            GetDbSize();
        }

        protected override void RestoreDb()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/client-api/commands/how-to/start-backup-restore-operations#startrestore
            var op = store
                .DatabaseCommands
                .GlobalAdmin
                .StartRestore(
                    new DatabaseRestoreRequest
                    {
                        BackupLocation = o.Backup,
                        DatabaseName = o.Db
                    }
                );

            op.WaitForCompletion();

            GetDbSize();
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

        protected override void Update()
        {
            throw new NotImplementedException();
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
            var op = store.DatabaseCommands.DeleteByIndex(
                "Raven/DocumentsByEntityName",
                new IndexQuery { Query = "Tag:Listings" },
                new BulkOperationOptions { AllowStale = false, StaleTimeout = TimeSpan.FromSeconds(30) }
            );

            op.WaitForCompletion();
        }

        private void GetDbSize()
        {
            // https://ravendb.net/docs/article-page/3.5/csharp/client-api/commands/how-to/get-database-and-server-statistics

            Retry(() =>
            {
                o.DbSize = store
                    .DatabaseCommands
                    .ForDatabase(o.Db)
                    .GetStatistics()
                    .CountOfDocuments;
            });
        }

    }
}
