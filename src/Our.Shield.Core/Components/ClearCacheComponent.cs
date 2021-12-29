using Our.Shield.Core.Services;
using Umbraco.Core.Composing;
using Umbraco.Core.Services.Implement;

namespace Our.Shield.Core.Components
{
    /// <summary>
    /// Component to hook into the Umbraco's event to clear Shield's caches
    /// </summary>
    public class ClearCacheComponent : IComponent
    {
        private readonly IUmbracoContentService _umbracoContentService;
        private readonly IUmbracoMediaService _umbracoMediaService;

        /// <inheritdoc/>
        public ClearCacheComponent(
            IUmbracoContentService umbracoContentService,
            IUmbracoMediaService umbracoMediaService)
        {
            _umbracoContentService = umbracoContentService;
            _umbracoMediaService = umbracoMediaService;
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            ContentService.Published += (sender, e) => _umbracoContentService.ClearCache();
            ContentService.Unpublished += (sender, e) => _umbracoContentService.ClearCache();

            MediaService.Saved += (sender, e) => _umbracoMediaService.ClearCache();
            MediaService.Deleted += (sender, e) => _umbracoMediaService.ClearCache();
        }

        /// <inheritdoc/>
        public void Terminate()
        {
            ContentService.Published -= (sender, e) => _umbracoContentService.ClearCache();
            ContentService.Unpublished -= (sender, e) => _umbracoContentService.ClearCache();

            MediaService.Saved -= (sender, e) => _umbracoContentService.ClearCache();
            MediaService.Deleted -= (sender, e) => _umbracoContentService.ClearCache();
        }
    }
}
