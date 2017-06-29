using System;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Shield.Core.Persistance.Data
{
    internal class Migration
    {
        public void RunMigrations(ISqlSyntaxProvider sqlSyntax, IMigrationEntryService migrationEntryService, ILogger logger)
        {
            const string productName = nameof(Shield);
            var currentVersion = new SemVersion(0, 0, 0);

            var scriptsForMigration = new IMigration[]
            {
                new Persistance.Data.Migrations.EnvironmentMigration (sqlSyntax, logger),
                new Persistance.Data.Migrations.ConfigurationMigration (sqlSyntax, logger),
                new Persistance.Data.Migrations.JournalMigration (sqlSyntax, logger),
                new Persistance.Data.Migrations.DomainMigration (sqlSyntax,logger)
            };

            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName);
            var latestMigration = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;

            var targetVersion = new SemVersion(1, 0, 0);
            if (targetVersion == currentVersion)
                return;

            MigrationRunner migrationsRunner = new MigrationRunner(migrationEntryService, logger, currentVersion, targetVersion, productName, scriptsForMigration);

            try
            {
                migrationsRunner.Execute(ApplicationContext.Current.DatabaseContext.Database);
            }
            catch (Exception ex)
            {
                LogHelper.Error<Migration>("Error running Shield migration", ex);
            }
        }
    }
}
