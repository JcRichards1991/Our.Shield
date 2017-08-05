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

    /// <summary>
    /// 
    /// </summary>
    internal class Migration
    {
        private readonly SemVersion TargetVersion = new SemVersion(1, 0, 2);

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

            var scriptsForMigration = new IMigration[]
            {
                new Versions.Migration100(sqlSyntax, logger),
                new Versions.Migration101(sqlSyntax, logger),
                new Versions.Migration102(sqlSyntax, logger)
            };

            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName);
            var latestMigration = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;
            
            if (TargetVersion == currentVersion)
                return;

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
