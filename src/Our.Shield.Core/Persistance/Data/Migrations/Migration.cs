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
        public static readonly SemVersion TargetVersion = new SemVersion(1, 0, 4);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlSyntax"></param>
        /// <param name="migrationEntryService"></param>
        /// <param name="logger"></param>
        public void RunMigrations(ISqlSyntaxProvider sqlSyntax, IMigrationEntryService migrationEntryService, ILogger logger)
        {
            const string productName = nameof(Shield);

            var currentVersion = new SemVersion(0);
            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName).OrderByDescending(x => x.CreateDate);
            var latestMigration = migrations.FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;
            
            if (TargetVersion == currentVersion)
                return;

            IMigration[] scriptsForMigration =
            {
                currentVersion == new SemVersion(0) 
                    ? (IMigration) new Versions.Migration103Create(sqlSyntax, logger)
                    : new Versions.Migration103(sqlSyntax, logger),
                new Versions.Migration104(sqlSyntax, logger)
            };

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
