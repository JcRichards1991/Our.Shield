using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Our.Shield.Core.Services
{
    public class UmbracoServiceBase
    {
        private const long CacheLengthInTicks = TimeSpan.TicksPerDay;
        internal UmbracoContext UmbContext;
        internal readonly TimeSpan CacheLength = new TimeSpan(CacheLengthInTicks);

        private static string CacheKeyId(string cacheKeyId, int id) => cacheKeyId + id;
        private static string CacheKeyXPath(string cacheKeyXPath, string xpath) => cacheKeyXPath + xpath;

        internal T GetApplicationCacheItem<T>(string cacheKeyXpath, string xpath, Func<object> getCacheItem) => 
            (T) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, CacheLength);

        internal T GetApplicationCacheItem<T>(string cacheKeyId, int id, Func<object> getCacheItem) => 
            (T) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, CacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyXpath, string xpath, Func<object> getCacheItem) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, CacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyId, int id, Func<object> getCacheItem) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, CacheLength);

        internal static void ClearApplicationCacheItem(string cacheKey) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(cacheKey);

        T GetRequestCacheItem<T>(string cacheKeyXpath, string xpath) => (T) UmbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)];
        T GetRequestCacheItem<T>(string cacheKeyId, int id) => (T) UmbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)];

        void SetRequestCacheKey<T>(string cacheKeyXpath, string xpath, T value) => 
            UmbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)] = value;

        void SetRequestCacheKey<T>(string cacheKeyId, int id, T value) => 
            UmbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)] = value;

        internal UmbracoServiceBase(UmbracoContext umbContext)
        {
            UmbContext = umbContext;
        }

        private IPublishedContent GetPublished<T>(int id)
        {
            if (typeof(T) == typeof(IPublishedMediaCache))
			{
				return UmbContext.MediaCache.GetById(id);
			}
			if (typeof(T) == typeof(IPublishedContentCache))
			{
				return UmbContext.ContentCache.GetById(id);
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
            if (id == Constants.System.Root)
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
                switch (typeof(T))
                {
                    case IPublishedMediaCache _:
                        published = UmbContext.MediaCache.GetSingleByXPath(xpath);
                        break;
                    case IPublishedContentCache _:
                        published = UmbContext.ContentCache.GetSingleByXPath(xpath);
                        break;
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
            return published.GetPropertyValue<TV>(alias);
        }

        public object Value<T>(string cacheKeyId, int id, string alias) where T : IPublishedCache
        {
            var published = GetPublished<T>(cacheKeyId, id);
            if (published == null || !published.HasProperty(alias))
            {
                return null;
            }
            return published.GetProperty(alias).Value;
        }

    }
}
