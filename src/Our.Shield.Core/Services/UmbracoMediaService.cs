using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using UmbConsts = Umbraco.Core.Constants;

namespace Our.Shield.Core.Services
{
    public class UmbracoMediaService : UmbracoServiceBase
    {
        private const string CacheKeyId = "8bad404c-82bb-477a-89a7-37bac7e75319";
        private const string CacheKeyXPath = "84ada488-7e7c-4e32-b46b-6c5968e4c629";
        private const string CacheKeyUrl = "7b658a7d-d166-4d59-b71f-6d98e5cb4f89";

        private readonly IMediaService _mediaService;

        public UmbracoMediaService(
            IUmbracoContextAccessor umbContextAccessor,
            AppCaches appCaches,
            IMediaService mediaService)
            : base(umbContextAccessor.UmbracoContext, appCaches)
        {
            _mediaService = mediaService;
        }

        private void ClearMediaCache()
        {
            ClearApplicationCacheItem(CacheKeyId);
            ClearApplicationCacheItem(CacheKeyXPath);
            ClearApplicationCacheItem(CacheKeyUrl);
        }

        public void ClearCache(Umbraco.Core.Services.IMediaService sender,
            Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            ClearMediaCache();
        }

        public void ClearCache(Umbraco.Core.Services.IMediaService sender,
            Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            ClearMediaCache();
        }

        public string Url(int id) => Url<IPublishedMediaCache>(CacheKeyId, id);

        public string Name(int id) => Name<IPublishedMediaCache>(CacheKeyId, id);

        public int? ParentId(int id) => ParentId<IPublishedMediaCache>(CacheKeyId, id);

        public int? XPath(string xpath) => XPath<IPublishedMediaCache>(CacheKeyId, CacheKeyXPath, xpath);

        public object Value(int id, string alias) => Value<IPublishedMediaCache>(CacheKeyId, id, alias);

        public T Value<T>(int id, string alias) => Value<IPublishedMediaCache, T>(CacheKeyId, id, alias);

        public int? Id(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return UmbConsts.System.Root;
            }

            return GetApplicationCacheItem(CacheKeyUrl, url, () =>
            {
                var published = _mediaService.GetMediaByPath(url);

                return published?.Id;
            });
        }

    }
}
