using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Our.Shield.Core.Services
{
    public class UmbracoContentService : UmbracoServiceBase
    {
        private const string CacheKeyId = "1a10ed15-e2b2-4ecb-b204-77d541076af3";
        private const string CacheKeyXPath = "9a93f022-d393-44bc-8ef5-6a3bfcbf8c31";

        public UmbracoContentService(UmbracoContext umbContext) : base(umbContext)
        {
        }

        public static void ClearCache(Umbraco.Core.Publishing.IPublishingStrategy sender,
            Umbraco.Core.Events.PublishEventArgs<IContent> e)
        {
            ClearApplicationCacheItem(CacheKeyId);
            ClearApplicationCacheItem(CacheKeyXPath);
        }

        public string Url(int id) => Url<IPublishedContentCache>(CacheKeyId, id);
        public string Name(int id) => Name<IPublishedContentCache>(CacheKeyId, id);
        public int? ParentId(int id) => ParentId<IPublishedContentCache>(CacheKeyId, id);
        public int? XPath(string xpath) => XPath<IPublishedContentCache>(CacheKeyId, CacheKeyXPath, xpath);
        public object Value(int id, string alias) => Value<IPublishedContentCache>(CacheKeyId, id, alias);
        public T Value<T>(int id, string alias) => Value<IPublishedContentCache, T>(CacheKeyId, id, alias);
    }
}
