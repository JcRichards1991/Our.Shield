using System;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;

namespace Shield.Core.Persistance.Dal
{
    /// <summary>
    /// Initialization class
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        /// <summary>
        /// Overrides the ApplicationEventHandler ApplicationStarted method
        /// </summary>
        /// <param name="umbracoApplication">
        /// The Umbraco Application.
        /// </param>
        /// <param name="applicationContext">
        /// The Application Context.
        /// </param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            RunMigrations();
        }
        
        private void RunMigrations()
        {
            const string productName = "Shield";
            var currentVersion = new SemVersion(0, 0, 0);

            var migrations = ApplicationContext.Current.Services.MigrationEntryService.GetAll(productName);
            var latestMigration = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latestMigration != null)
                currentVersion = latestMigration.Version;

            var targetVersion = new SemVersion(1, 0, 0);
            if (targetVersion == currentVersion)
                return;

            var migrationsRunner = new MigrationRunner(
              ApplicationContext.Current.Services.MigrationEntryService,
              ApplicationContext.Current.ProfilingLogger.Logger,
              currentVersion,
              targetVersion,
              productName);

            try
            {
                migrationsRunner.Execute(UmbracoContext.Current.Application.DatabaseContext.Database);
            }
            catch (Exception e)
            {
                LogHelper.Error<Register>("Error running Shield migration", e);
            }
        }
    }
}
