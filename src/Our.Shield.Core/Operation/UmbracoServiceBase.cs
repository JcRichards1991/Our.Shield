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
    public class UmbracoServiceBase
    {
        private const long CacheLengthInTicks = TimeSpan.TicksPerDay;
        internal UmbracoContext _umbContext;
        internal readonly TimeSpan cacheLength = new TimeSpan(CacheLengthInTicks);

        string CacheKeyId(string cacheKeyId, int id) => cacheKeyId + id.ToString();
        string CacheKeyXPath(string cacheKeyXPath, string xpath) => cacheKeyXPath + xpath;

        internal T GetApplicationCacheItem<T>(string cacheKeyXpath, string xpath, Func<object> getCacheItem) => 
            (T) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, cacheLength);

        internal T GetApplicationCacheItem<T>(string cacheKeyId, int id, Func<object> getCacheItem) => 
            (T) ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, cacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyXpath, string xpath, Func<object> getCacheItem) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(CacheKeyXPath(cacheKeyXpath, xpath), getCacheItem, cacheLength);

        internal void InsertApplicationCacheItem(string cacheKeyId, int id, Func<object> getCacheItem) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.InsertCacheItem(CacheKeyId(cacheKeyId, id), getCacheItem, cacheLength);

        static internal void ClearApplicationCacheItem(string cacheKey) => 
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(cacheKey);

        T GetRequestCacheItem<T>(string cacheKeyXpath, string xpath) => (T) _umbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)];
        T GetRequestCacheItem<T>(string cacheKeyId, int id) => (T) _umbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)];

        void SetRequestCacheKey<T>(string cacheKeyXpath, string xpath, T value) => 
            _umbContext.HttpContext.Items[CacheKeyXPath(cacheKeyXpath, xpath)] = value;

        void SetRequestCacheKey<T>(string cacheKeyId, int id, T value) => 
            _umbContext.HttpContext.Items[CacheKeyId(cacheKeyId, id)] = value;

        internal UmbracoServiceBase(UmbracoContext umbContext)
        {
            _umbContext = umbContext;
        }

        private IPublishedContent GetPublished<T>(int id)
        {
            if (typeof(T) is IPublishedMediaCache)
            {
                return _umbContext.MediaCache.GetById(id);
            }
            else if (typeof(T) is IPublishedContentCache)
            {
                return _umbContext.ContentCache.GetById(id);
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
            SetPublished<T>(cacheKeyId, published);
            return published;
        }

        internal void SetPublished<T>(string cacheKeyId, IPublishedContent published) => 
            SetRequestCacheKey<IPublishedContent>(cacheKeyId, published.Id, published);

        internal class PublishCache
        {
            public int Id;
            public string Name;
            public string Url;
            public string Path;
            private int? parentId = null;
            public int? ParentId
            {
                get
                {
                    if (parentId != null)
                    {
                        return parentId;
                    }

                    var split = Path.Split(',');
                    if (split.Length < 2)
                    {
                        return null;
                    }
                    int id;
                    if (int.TryParse(split[split.Length - 2], out id))
                    {
                        parentId = id;
                        return parentId;
                    }
                    return null;
                }
            }
        }

        internal PublishCache GetPublishCache<T>(string cacheKeyId, int id) where T : IPublishedCache
        {
            if (id == Umbraco.Core.Constants.System.Root)
            {
                return null;
            }
            var cacheKey = cacheKeyId + id.ToString();
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
            InsertApplicationCacheItem(cacheKeyId, published.Id, () => {
                return new PublishCache
                {
                    Id = published.Id,
                    Name = published.Name,
                    Url = published.Url,
                    Path = published.Path
                };
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
                if (typeof(T) is IPublishedMediaCache)
                {
                    published = _umbContext.MediaCache.GetSingleByXPath(xpath);
                }
                else if (typeof(T) is IPublishedContentCache)
                {
                    published = _umbContext.ContentCache.GetSingleByXPath(xpath);
                }
                if (published == null)
                {
                    return null;
                }

                SetPublished<T>(cacheKeyId + published.Id.ToString(), published);
                SetPublishCache<T>(cacheKeyId, published);
                return published.Id;
            });
        }

        public V Value<T, V>(string cacheKeyId, int id, string alias) where T : IPublishedCache
        {
            var published = GetPublished<T>(cacheKeyId, id);
            if (published == null || !published.HasProperty(alias))
            {
                return default(V);
            }
            return published.GetPropertyValue<V>(alias);
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
