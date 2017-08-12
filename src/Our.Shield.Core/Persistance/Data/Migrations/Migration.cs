using Semver;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Our.Shield.Core.Persistance.Data.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    internal class Migration
    {
        public static readonly SemVersion TargetVersion = new SemVersion(1, 0, 2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlSyntax"></param>
        /// <param name="migrationEntryService"></param>
        /// <param name="logger"></param>
        public void RunMigrations(ISqlSyntaxProvider sqlSyntax, IMigrationEntryService migrationEntryService, ILogger logger)
        {
            const string productName = nameof(Shield);

            var currentVersion = new SemVersion(0, 0, 0);
            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName).OrderByDescending(x => x.CreateDate);
            var latestMigration = migrations.FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;
            
            if (TargetVersion == currentVersion)
                return;

            IMigration[] scriptsForMigration = new IMigration[]
            {
                new Versions.Migration102Create(sqlSyntax, logger)
                //  new versions.Migrations103 etc.
            };

            if (currentVersion == new SemVersion(1, 0, 1))
            {
                scriptsForMigration[0] = new Versions.Migration102(sqlSyntax, logger);
            }

            MigrationRunner migrationsRunner = new MigrationRunner(migrationEntryService, logger, currentVersion, TargetVersion, 
            productName, scriptsForMigration);

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
