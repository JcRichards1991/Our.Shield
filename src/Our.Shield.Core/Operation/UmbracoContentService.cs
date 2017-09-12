using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebServices;

namespace Our.Shield.Core.Operation
{
    public class UmbracoContentService : UmbracoContentServiceBase
    {
        private const string cacheKeyId = "1a10ed15-e2b2-4ecb-b204-77d541076af3";
        private const string cacheKeyXPath = "9a93f022-d393-44bc-8ef5-6a3bfcbf8c31";

        public UmbracoContentService(UmbracoContext umbContext) : base(umbContext)
        {
        }

        public static void ClearCache(Umbraco.Core.Publishing.IPublishingStrategy sender,
            Umbraco.Core.Events.PublishEventArgs<IContent> e)
        {
            ClearApplicationCacheItem(cacheKeyId);
            ClearApplicationCacheItem(cacheKeyXPath);
        }

        public string Url(int id) => Url<IPublishedContentCache>(cacheKeyId, id);
        public string Name(int id) => Name<IPublishedContentCache>(cacheKeyId, id);
        public int? ParentId(int id) => ParentId<IPublishedContentCache>(cacheKeyId, id);
        public int? XPath(string xpath) => XPath<IPublishedContentCache>(cacheKeyId, cacheKeyXPath, xpath);
        public object Value(int id, string alias) => Value<IPublishedContentCache>(cacheKeyId, id, alias);
        public T Value<T>(int id, string alias) => Value<IPublishedContentCache, T>(cacheKeyId, id, alias);
    }
}
