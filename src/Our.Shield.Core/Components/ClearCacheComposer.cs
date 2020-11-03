using Our.Shield.Core.Persistence.Data.Migrations;
using Our.Shield.Core.Services;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Services;

namespace Our.Shield.Core.Components
{
    /// <summary>
    /// Component to hook into the Umbraco's event to clear Shield's caches
    /// </summary>
    public class ClearCacheComponent : IComponent
    {
        /// <inheritdoc/>
        public void Initialize(ISectionService sectionService)
        {
            var t = sectionService.GetSections(); // .MakeNew(UI.Constants.App.Name, UI.Constants.App.Alias, UI.Constants.App.Icon);

            new Migration().RunMigrations(
                applicationContext.DatabaseContext.SqlSyntax,
                applicationContext.Services.MigrationEntryService,
                applicationContext.ProfilingLogger.Logger);

            JobService.Instance.Init(applicationContext);

            ContentService.Published += UmbracoContentService.ClearCache;
            ContentService.Unpublished += UmbracoContentService.ClearCache;

            MediaService.Saved += UmbracoMediaService.ClearCache;
            MediaService.Deleted += UmbracoMediaService.ClearCache;
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            ContentService.Published -= UmbracoContentService.ClearCache;
            ContentService.Unpublished -= UmbracoContentService.ClearCache;

            MediaService.Saved -= UmbracoMediaService.ClearCache;
            MediaService.Deleted -= UmbracoMediaService.ClearCache;
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
