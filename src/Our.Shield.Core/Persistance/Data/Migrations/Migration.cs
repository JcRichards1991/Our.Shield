namespace Our.Shield.Core.Persistance.Data.Migrations
{
    using System;
    using System.Linq;
    using Semver;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;
    using Umbraco.Core.Services;
    using System.Collections.Generic;
    using Umbraco.Core.Models;

    /// <summary>
    /// 
    /// </summary>
    internal class Migration
    {
        public static readonly SemVersion TargetVersion = new SemVersion(1, 0, 2);

        public static SemVersion CurrentVersion = new SemVersion(0, 0, 0);

        public static IEnumerable<IMigrationEntry> Migrations;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlSyntax"></param>
        /// <param name="migrationEntryService"></param>
        /// <param name="logger"></param>
        public void RunMigrations(ISqlSyntaxProvider sqlSyntax, IMigrationEntryService migrationEntryService, ILogger logger)
        {
            const string productName = nameof(Shield);

            var scriptsForMigration = new IMigration[]
            {
                new Versions.Migration102(sqlSyntax, logger)
            };

            Migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName).OrderByDescending(x => x.CreateDate);
            var latestMigration = Migrations.FirstOrDefault();

            if (latestMigration != null)
                CurrentVersion = latestMigration.Version;
            
            if (TargetVersion == CurrentVersion)
                return;

            MigrationRunner migrationsRunner = new MigrationRunner(migrationEntryService, logger, CurrentVersion, TargetVersion, 
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
