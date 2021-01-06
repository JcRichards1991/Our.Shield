using Umbraco.Core.Composing;

namespace Our.Shield.Core.Components
{
    /// <summary>
    /// Component to hook into the Umbraco's event to clear Shield's caches
    /// </summary>
    public class ClearCacheComponent : IComponent
    {
        /// <inheritdoc/>
        public ClearCacheComponent()
        {
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            //new Migration().RunMigrations(
            //    _sqlSyntaxProvider,
            //    applicationContext.Services.MigrationEntryService,
            //    _logger);

            //JobService.Instance.Init(_sqlContext);

            //ContentService.Published += UmbracoContentService.ClearCache;
            //ContentService.Unpublished += UmbracoContentService.ClearCache;

            //MediaService.Saved += UmbracoMediaService.ClearCache;
            //MediaService.Deleted += UmbracoMediaService.ClearCache;
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            //ContentService.Published -= UmbracoContentService.ClearCache;
            //ContentService.Unpublished -= UmbracoContentService.ClearCache;

            //MediaService.Saved -= UmbracoMediaService.ClearCache;
            //MediaService.Deleted -= UmbracoMediaService.ClearCache;
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
}
