using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Our.Shield.Core.Services
{
    /// <inheritdoc/>
    public class UmbracoContentService : UmbracoServiceBase
    {
        private const string CacheKeyId = "1a10ed15-e2b2-4ecb-b204-77d541076af3";
        private const string CacheKeyXPath = "9a93f022-d393-44bc-8ef5-6a3bfcbf8c31";

        /// <inheritdoc/>
        public UmbracoContentService(IUmbracoContextAccessor umbracoAssessor) : base (umbracoAssessor.UmbracoContext)
        {

        }
        
        /// <inheritdoc/>
        public static void ClearCache(object sender, PublishEventArgs<IContent> e)
        {
            ClearApplicationCacheItem(CacheKeyId);
            ClearApplicationCacheItem(CacheKeyXPath);
        }

        /// <inheritdoc/>
        public string Url(int id) => Url<IPublishedContentCache>(CacheKeyId, id);

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
