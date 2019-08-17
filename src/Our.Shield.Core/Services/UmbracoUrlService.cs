using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Our.Shield.Core.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Our.Shield.Core.Services
{
    public class UmbracoUrlService
    {
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
        public string Url(UmbracoUrl umbracoUrl, out bool isUmbracoContent)
        {
            isUmbracoContent = false;

            if (string.IsNullOrEmpty(umbracoUrl.Value))
            {
                LogHelper.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);
                return null;
            }

            if (umbracoUrl.Type == UmbracoUrlTypes.Url)
            {
                return umbracoUrl.Value;
            }
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
        }
    }
}
