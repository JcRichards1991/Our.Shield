using System;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web;
using umbraco.businesslogic;

namespace Shield.Core.Initialse
{
    /// <summary>
    /// Shield custom section
    /// </summary>
    [Application(Constants.App.Alias, Constants.App.Name, Constants.App.Icon, 1000)]
    public class Application : umbraco.interfaces.IApplication
    {

    }

    /// <summary>
    /// Initialization class.
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        /// <summary>
        /// Overrides the ApplicationEventHandler ApplicationStarting method.
        /// </summary>
        /// <param name="umbracoApplication">
        /// The Umbraco Application.
        /// </param>
        /// <param name="applicationContext">
        /// The Application Context.
        /// </param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarting(umbracoApplication, applicationContext);

            //ServerRegistrarResolver.Current.SetServerRegistrar(new FrontEndReadOnlyServerRegistrar());
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            RunMigrations();
            Operation.Executor.Instance.Init();
        }
        
        private void RunMigrations()
        {
            const string productName = nameof(Shield);
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



    // TEST CODE. 
    // Keep for now so don't need to find it again in future.

    //public class FrontEndReadOnlyServerRegistrar : IServerRegistrar2
    //{
    //    public IEnumerable<IServerAddress> Registrations
    //    {
    //        get { return Enumerable.Empty<IServerAddress>(); }
    //    }
        
    //    public ServerRole GetCurrentServerRole()
    //    {
    //        return ServerRole.Slave;
    //    }
        
    //    public string GetCurrentServerUmbracoApplicationUrl()
    //    {
    //        return "http://shield.local/josh";
    //    }
    //}
}
