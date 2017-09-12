using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistance.Business;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Our.Shield.Core.Operation
{
    public class UmbracoUrlService
    {
        /// <summary>
        /// Gets the Url from the UmbracoUrl type
        /// </summary>
        /// <param name="urlType">The Url Type object from the app's config</param>
        /// <returns>The Unauthorised Url, or null</returns>
        public string Url(UmbracoUrl url)
        {
            if (string.IsNullOrEmpty(url.Value))
            {
                LogHelper.Error<UmbracoUrlService>("Error: No Unauthorized URL set in configuration", null);
                return null;
            }

            if (url.Type == UmbracoUrlTypes.Url)
            {
                return url.Value;
            }
            else
            {
                var umbContext = UmbracoContext.Current;
                if (umbContext == null)
                {
                    LogHelper.Error<UmbracoUrlService>("Need to run this method from within a valid HttpContext request", null);
                    return null;
                }

                var umbracoContentService = new UmbracoContentService(umbContext);
                switch (url.Type)
                {
                    case UmbracoUrlTypes.XPath:
                        var xpathId = umbracoContentService.XPath(url.Value);
                        if (xpathId == null)
                        {
                            LogHelper.Error<UmbracoUrlService>($"Error: Unable to find content using xpath of '{url.Value}'", null);
                            return null;
                        }
                        return umbracoContentService.Url((int) xpathId);

                    case UmbracoUrlTypes.ContentPicker:
                        int id;
                        if (!int.TryParse(url.Value, out id))
                        {
                            LogHelper.Error<UmbracoUrlService>("Error: Unable to parse the selected unauthorized URL content picker item. Please ensure a valid content node is selected", null);
                            return null;
                        }
                        return umbracoContentService.Url(id);
                }
            }
            LogHelper.Error<UmbracoUrlService>("Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected", null);
            return null;
        }
    }
}
