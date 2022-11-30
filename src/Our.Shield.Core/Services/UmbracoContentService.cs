using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Our.Shield.Core.Services
{
    /// <inheritdoc/>
    public class UmbracoContentService : UmbracoServiceBase, IUmbracoContentService
    {
        private const string CacheKeyId = "1a10ed15-e2b2-4ecb-b204-77d541076af3";

        private const string CacheKeyXPath = "9a93f022-d393-44bc-8ef5-6a3bfcbf8c31";

        private const string CacheKeyUdi = "0b3d0ac5-7aa1-47fa-b8a9-550b835cb290";

        /// <inheritdoc/>
        public UmbracoContentService(
            IUmbracoContextAccessor umbracoContextAccessor,
            AppCaches appCaches)
            : base(umbracoContextAccessor.UmbracoContext, appCaches)
        {
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            ClearApplicationCacheItem(CacheKeyId);
            ClearApplicationCacheItem(CacheKeyXPath);
        }

        /// <inheritdoc/>
        public string Url(int id) => Url<IPublishedContentCache>(CacheKeyId, id);

        /// <inheritdoc/>
        public string Url(GuidUdi udi) => Url<IPublishedContentCache>(CacheKeyUdi, udi);

        /// <inheritdoc/>
        public string Name(int id) => Name<IPublishedContentCache>(CacheKeyId, id);

        /// <inheritdoc/>
        public int? ParentId(int id) => ParentId<IPublishedContentCache>(CacheKeyId, id);

        /// <inheritdoc/>
        public int? XPath(string xpath) => XPath<IPublishedContentCache>(CacheKeyId, CacheKeyXPath, xpath);

        /// <inheritdoc/>
        public object Value(int id, string alias) => Value<IPublishedContentCache>(CacheKeyId, id, alias);

        /// <inheritdoc/>
        public T Value<T>(int id, string alias) => Value<IPublishedContentCache, T>(CacheKeyId, id, alias);
    }
}
