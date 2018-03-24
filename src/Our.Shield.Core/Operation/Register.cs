using Our.Shield.Core.Services;
using Umbraco.Core;

namespace Our.Shield.Core.Operation
{
    /// <inheritdoc />
    /// <summary>
    /// Initialization class
    /// </summary>
    public class Register : ApplicationEventHandler
    {
        /// <inheritdoc />
        /// <summary>
        /// Overrides the ApplicationEventHandler ApplicationStarting method
        /// </summary>
        /// <param name="umbracoApplication">The Umbraco Application</param>
        /// <param name="applicationContext">The Application Context</param>
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarting(umbracoApplication, applicationContext);

            //ServerRegistrarResolver.Current.SetServerRegistrar(new FrontEndReadOnlyServerRegistrar());
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            new Persistance.Data.Migrations.Migration().RunMigrations(applicationContext.DatabaseContext.SqlSyntax, 
                applicationContext.Services.MigrationEntryService, applicationContext.ProfilingLogger.Logger);

            JobService.Instance.Init(umbracoApplication, applicationContext);

            Umbraco.Core.Services.ContentService.Published += UmbracoContentService.ClearCache;
            Umbraco.Core.Services.ContentService.UnPublished += UmbracoContentService.ClearCache;

            Umbraco.Core.Services.MediaService.Saved += UmbracoMediaService.ClearCache;
            Umbraco.Core.Services.MediaService.Deleted += UmbracoMediaService.ClearCache;
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
