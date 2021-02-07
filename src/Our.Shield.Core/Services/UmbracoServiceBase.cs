using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using UmbConsts = Umbraco.Core.Constants;

namespace Our.Shield.Core.Services
{
    public class UmbracoServiceBase
    {
        private const long CacheLengthInTicks = TimeSpan.TicksPerDay;
        internal UmbracoContext UmbContext;
        internal AppCaches AppCaches;
        internal readonly TimeSpan CacheLength = new TimeSpan(CacheLengthInTicks);

        internal UmbracoServiceBase(UmbracoContext umbContext, AppCaches appCaches)
        {
            UmbContext = umbContext;
            AppCaches = appCaches;
        }

        private static string CacheKeyId(string cacheKeyId, int id) => cacheKeyId + id;
        private static string CacheKeyXPath(string cacheKeyXPath, string xpath) => cacheKeyXPath + xpath;

        internal T GetApplicationCacheItem<T>(string cacheKeyXpath, string xpath, Func<T> getCacheItem) =>
            AppCaches.RuntimeCache.GetCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, CacheLength);

        internal T GetApplicationCacheItem<T>(string cacheKeyId, int id, Func<T> getCacheItem) =>
            AppCaches.RuntimeCache.GetCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, CacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyXpath, string xpath, Func<object> getCacheItem) =>
            AppCaches.RuntimeCache.InsertCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, CacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyId, int id, Func<object> getCacheItem) =>
            AppCaches.RuntimeCache.InsertCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, CacheLength);

        internal void ClearApplicationCacheItem(string cacheKey) =>
            throw new NotImplementedException(); // AppCaches.RuntimeCache.ClearCacheByKeySearch(cacheKey);

        T GetRequestCacheItem<T>(string cacheKeyXpath, string xpath) => (T)UmbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)];
        T GetRequestCacheItem<T>(string cacheKeyId, int id) => (T)UmbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)];

        void SetRequestCacheKey<T>(string cacheKeyXpath, string xpath, T value) =>
            UmbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)] = value;

        void SetRequestCacheKey<T>(string cacheKeyId, int id, T value) =>
            UmbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)] = value;

        private IPublishedContent GetPublished<T>(int id)
        {
            if (typeof(T) == typeof(IPublishedMediaCache))
            {
                return UmbContext.Media.GetById(id);
            }
            if (typeof(T) == typeof(IPublishedContentCache))
            {
                return UmbContext.Content.GetById(id);
            }

            return null;
        }

        internal IPublishedContent GetPublished<T>(string cacheKeyId, int id)
            where T : IPublishedCache
        {
            var published = GetRequestCacheItem<IPublishedContent>(cacheKeyId, id);
            if (published != null)
            {
                return published;
            }

            published = GetPublished<T>(id);
            if (published != null)
            {
                SetPublished<T>(cacheKeyId, published);
            }
            return published;
        }

        internal void SetPublished<T>(string cacheKeyId, IPublishedContent published) =>
            SetRequestCacheKey(cacheKeyId, published.Id, published);

        internal class PublishCache
        {
            public int Id;
            public string Name;
            public string Url;
            public string Path;
            private int? _parentId;
            public int? ParentId
            {
                get
                {
                    if (_parentId != null)
                    {
                        return _parentId;
                    }

                    var split = Path.Split(',');

                    if (split.Length < 2)
                    {
                        return null;
                    }

                    if (!int.TryParse(split[split.Length - 2], out var id))
                        return null;

                    return _parentId = id;
                }
            }
        }

        internal PublishCache GetPublishCache<T>(string cacheKeyId, int id) where T : IPublishedCache
        {
            if (id == UmbConsts.System.Root)
            {
                return null;
            }

            var cacheKey = cacheKeyId + id;

            return GetApplicationCacheItem<PublishCache>(cacheKeyId, id, () =>
            {
                var content = GetPublished<T>(cacheKey, id);

                if (content == null)
                {
                    return null;
                }

                return new PublishCache
                {
                    Id = id,
                    Name = content.Name,
                    Url = content.Url,
                    Path = content.Path
                };
            });
        }

        internal void SetPublishCache<T>(string cacheKeyId, IPublishedContent published) where T : IPublishedCache =>
            InsertApplicationCacheItem(cacheKeyId, published.Id, () => new PublishCache
            {
                Id = published.Id,
                Name = published.Name,
                Url = published.Url,
                Path = published.Path
            });

        internal string Url<T>(string cacheKeyId, int id) where T : IPublishedCache => GetPublishCache<T>(cacheKeyId, id)?.Url;

        internal string Name<T>(string cacheKeyId, int id) where T : IPublishedCache => GetPublishCache<T>(cacheKeyId, id)?.Name;

        internal int? ParentId<T>(string cacheKeyId, int id) where T : IPublishedCache => GetPublishCache<T>(cacheKeyId, id)?.ParentId;

        internal int? XPath<T>(string cacheKeyId, string cacheKeyXPath, string xpath) where T : IPublishedCache
        {
            if (string.IsNullOrWhiteSpace(xpath))
            {
                return null;
            }

            return GetApplicationCacheItem<int?>(cacheKeyXPath, xpath, () =>
            {
                IPublishedContent published = null;
                if (typeof(T).IsAssignableFrom(typeof(IPublishedContentCache)))
                {
                    published = UmbContext.Content.GetSingleByXPath(xpath);
                }
                else if (typeof(T).IsAssignableFrom(typeof(IPublishedMediaCache)))
                {
                    published = UmbContext.Media.GetSingleByXPath(xpath);
                }
                if (published == null)
                {
                    return null;
                }

                SetPublished<T>(cacheKeyId + published.Id, published);
                SetPublishCache<T>(cacheKeyId, published);
                return published.Id;
            });
        }

        public TV Value<T, TV>(string cacheKeyId, int id, string alias) where T : IPublishedCache
        {
            var published = GetPublished<T>(cacheKeyId, id);

            if (published == null || !published.HasProperty(alias))
            {
                return default(TV);
            }

            return published.Value<TV>(alias);
        }

        public object Value<T>(string cacheKeyId, int id, string alias) where T : IPublishedCache
        {
            var published = GetPublished<T>(cacheKeyId, id);

            if (published == null || !published.HasProperty(alias))
            {
                return null;
            }

            return published.Value(alias);
        }
    }
}
