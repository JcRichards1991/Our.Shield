using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Our.Shield.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Our.Shield.Core.Services
{
	public class UmbracoUrlService
	{
		private IRuntimeCacheProvider cache = null;
		private string CacheKeyUrl = "c3ee352b-e80f-4db1-9d13-d74a9a5a532d:";
		private string CacheKeyIsUmbracoUrl = "524ecafb-adc6-1054-a867-171e57f0e76c:";
		private TimeSpan CacheDuration = new TimeSpan(TimeSpan.TicksPerMinute * 10);

		public void EnsureUmbracoContext(HttpContext context)
		{
			if (UmbracoContext.Current == null)
			{
				var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest(context.Request.Url.AbsolutePath, "", new StringWriter())));
				UmbracoContext.EnsureContext(
					dummyHttpContext,
					ApplicationContext.Current,
					new WebSecurity(dummyHttpContext, ApplicationContext.Current),
					UmbracoConfig.For.UmbracoSettings(),
					UrlProviderResolver.Current.Providers,
					false);
			}
		}

		/// <summary>
		/// Gets the Url from the UmbracoUrl type
		/// </summary>
		/// <param name="umbracoUrl">The umbraco url object from the app's config</param>
		/// <returns>The Unauthorised Url, or null</returns>
		public string Url(UmbracoUrl umbracoUrl)
		{
			if (string.IsNullOrEmpty(umbracoUrl.Value))
			{
				LogHelper.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);
				return null;
			}

			if (umbracoUrl.Type == UmbracoUrlTypes.Url)
			{
				return umbracoUrl.Value;
			}
			if (cache == null)
			{
				cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
			}

			return cache.GetCacheItem<string>(CacheKeyUrl + umbracoUrl.Value, () =>
			{
				EnsureUmbracoContext(HttpContext.Current);
				var umbContext = UmbracoContext.Current;
				if (umbContext == null)
				{
					LogHelper.Error<UmbracoUrlService>("Need to run this method from within a valid HttpContext request", null);
					return null;
				}

				var umbracoContentService = new UmbracoContentService(umbContext);
				switch (umbracoUrl.Type)
				{
					case UmbracoUrlTypes.XPath:
						var xpathId = umbracoContentService.XPath(umbracoUrl.Value);
						if (xpathId != null)
							return umbracoContentService.Url((int)xpathId);

						LogHelper.Error<UmbracoUrlService>($"Error: Unable to find content using xpath of '{umbracoUrl.Value}'", null);
						break;

					case UmbracoUrlTypes.ContentPicker:
						if (int.TryParse(umbracoUrl.Value, out var id))
							return umbracoContentService.Url(id);

						LogHelper.Error<UmbracoUrlService>("Error: Unable to parse the selected unauthorized URL content picker item. Please ensure a valid content node is selected", null);
						break;

					default:
						LogHelper.Error<UmbracoUrlService>("Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected", null);
						break;
				}
				return null;
			}, CacheDuration);
		}


		/// <summary>
		/// States whether an Url is handled by Umbraco
		/// </summary>
		/// <param name="umbracoUrl">The umbraco url object from the app's config</param>
		/// <returns>Url is an Ubraco Url</returns>
		public bool IsUmbracoUrl(UmbracoUrl umbracoUrl)
		{
			if (string.IsNullOrEmpty(umbracoUrl.Value))
			{
				LogHelper.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);
				return false;
			}

			if (umbracoUrl.Type == UmbracoUrlTypes.XPath || umbracoUrl.Type == UmbracoUrlTypes.ContentPicker)
			{
				return true;
			}
			if (cache == null)
			{
				cache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
			}

			return cache.GetCacheItem<bool>(CacheKeyIsUmbracoUrl + umbracoUrl.Value, () =>
			{
				EnsureUmbracoContext(HttpContext.Current);
				var umbContext = UmbracoContext.Current;

				return umbContext.ContentCache.GetByRoute(umbracoUrl.Value) != null;
			}, CacheDuration);
		}
	}
}
