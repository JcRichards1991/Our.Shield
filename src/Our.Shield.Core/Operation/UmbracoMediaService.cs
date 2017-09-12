using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.WebServices;

namespace Our.Shield.Core.Operation
{
    public class UmbracoMediaService : UmbracoContentServiceBase
    {
        private const string cacheKeyId = "8bad404c-82bb-477a-89a7-37bac7e75319";
        private const string cacheKeyXPath = "84ada488-7e7c-4e32-b46b-6c5968e4c629";
        private const string cacheKeyUrl = "7b658a7d-d166-4d59-b71f-6d98e5cb4f89";

        public UmbracoMediaService(UmbracoContext umbContext) : base(umbContext)
        {
        }

        private static void ClearMediaCache()
        {
            ClearApplicationCacheItem(cacheKeyId);
            ClearApplicationCacheItem(cacheKeyXPath);
            ClearApplicationCacheItem(cacheKeyUrl);
        }

        public static void ClearCache(Umbraco.Core.Services.IMediaService sender,
            Umbraco.Core.Events.SaveEventArgs<Umbraco.Core.Models.IMedia> e)
        {
            ClearMediaCache();
        }

        public static void ClearCache(Umbraco.Core.Services.IMediaService sender,
            Umbraco.Core.Events.DeleteEventArgs<Umbraco.Core.Models.IMedia> e)
        {
            ClearMediaCache();
        }

        public string Url(int id) => Url<IPublishedMediaCache>(cacheKeyId, id);
        public string Name(int id) => Name<IPublishedMediaCache>(cacheKeyId, id);
        public int? ParentId(int id) => ParentId<IPublishedMediaCache>(cacheKeyId, id);
        public int? XPath(string xpath) => XPath<IPublishedMediaCache>(cacheKeyId, cacheKeyXPath, xpath);
        public object Value(int id, string alias) => Value<IPublishedMediaCache>(cacheKeyId, id, alias);
        public T Value<T>(int id, string alias) => Value<IPublishedMediaCache, T>(cacheKeyId, id, alias);

        public int? Id(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Umbraco.Core.Constants.System.Root;
            }
            return GetApplicationCacheItem<int?>(cacheKeyUrl, url, () =>
            {
                IMedia published = ApplicationContext.Current.Services.MediaService.GetMediaByPath(url);
                if (published == null)
                {
                    return null;
                }
                return published.Id;
            });
        }

    }
}
