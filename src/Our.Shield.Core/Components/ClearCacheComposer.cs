using Our.Shield.Core.Services;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Our.Shield.Core.Components
{
    /// <summary>
    /// Component to hook into the Umbraco's event to clear Shield's caches
    /// </summary>
    public class ClearCacheComponent : IComponent
    {
        /// <inheritdoc/>
        public void Initialize()
        {
            ContentService.Published += UmbracoContentService.ClearCache;
            ContentService.Unpublished += UmbracoContentService.ClearCache;

            MediaService.Saved += UmbracoMediaService.ClearCache;
            MediaService.Deleted += UmbracoMediaService.ClearCache;
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            throw new System.NotImplementedException();
        }
    }
}
