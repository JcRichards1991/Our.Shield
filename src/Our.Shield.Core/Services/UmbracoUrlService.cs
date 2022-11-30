using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class UmbracoUrlService : IUmbracoUrlService
    {
        private readonly IUmbracoContentService _umbracoContentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly AppCaches _appCaches;
        private readonly ILogger _logger;

        private const string CacheKeyUrl = "c3ee352b-e80f-4db1-9d13-d74a9a5a532d:";
        private const string CacheKeyIsUmbracoUrl = "524ecafb-adc6-1054-a867-171e57f0e76c:";
        private TimeSpan CacheDuration = new TimeSpan(TimeSpan.TicksPerMinute * 10);

        internal UmbracoUrlService()
        {
            _umbracoContentService = Umbraco.Core.Composing.Current.Factory.GetInstance<IUmbracoContentService>();
            _umbracoContextFactory = Umbraco.Core.Composing.Current.Factory.GetInstance<IUmbracoContextFactory>();
            _appCaches = Umbraco.Core.Composing.Current.AppCaches;
            _logger = Umbraco.Core.Composing.Current.Logger;
        }

        /// <inheritdoc />
        public string Url(UmbracoUrl umbracoUrl)
        {
            if (string.IsNullOrEmpty(umbracoUrl.Value))
            {
                _logger.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);

                return null;
            }

            if (umbracoUrl.Type == UmbracoUrlType.Url)
            {
                return umbracoUrl.Value;
            }

            return _appCaches.RuntimeCache.GetCacheItem(CacheKeyUrl + umbracoUrl.Value, () =>
            {
                switch (umbracoUrl.Type)
                {
                    case UmbracoUrlType.XPath:
                        var xpathId = _umbracoContentService.XPath(umbracoUrl.Value);
                        if (xpathId != null)
                        {
                            return _umbracoContentService.Url((int)xpathId);
                        }

                        _logger.Error<UmbracoUrlService>($"Error: Unable to find content using XPath of '{umbracoUrl.Value}'", null);
                        break;

                    case UmbracoUrlType.ContentPicker:
                        if (GuidUdi.TryParse(umbracoUrl.Value, out var udi))
                        {
                            return _umbracoContentService.Url(udi);
                        }

                        _logger.Error<UmbracoUrlService>("Error: Unable to parse the selected unauthorized URL content picker item. Please ensure a valid content node is selected", null);
                        break;

                    default:
                        _logger.Error<UmbracoUrlService>("Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected", null);
                        break;
                }
                return null;
            }, CacheDuration);
        }

        /// <inheritdoc />
        public bool IsUmbracoUrl(UmbracoUrl umbracoUrl)
        {
            if (string.IsNullOrEmpty(umbracoUrl.Value))
            {
                _logger.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);

                return false;
            }

            if (umbracoUrl.Type == UmbracoUrlType.XPath || umbracoUrl.Type == UmbracoUrlType.ContentPicker)
            {
                return true;
            }

            return _appCaches.RuntimeCache.GetCacheItem(
                CacheKeyIsUmbracoUrl + umbracoUrl.Value,
                () =>
                {
                    using (var umbContextRef = _umbracoContextFactory.EnsureUmbracoContext())
                    {
                        return umbContextRef.UmbracoContext.Content.GetByRoute(umbracoUrl.Value) != null;
                    }
                },
                CacheDuration);
        }
    }
}
