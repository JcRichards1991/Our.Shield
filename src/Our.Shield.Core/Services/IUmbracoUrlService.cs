using Our.Shield.Core.Models;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUmbracoUrlService
    {
        /// <summary>
        /// Gets the Url from the UmbracoUrl type
        /// </summary>
        /// <param name="umbracoUrl">The umbraco url object from the app's config</param>
        /// <returns>The Unauthorised Url, or null</returns>
        string Url(UmbracoUrl umbracoUrl);

        /// <summary>
        /// States whether an Url is handled by Umbraco
        /// </summary>
        /// <param name="umbracoUrl">The umbraco url object from the app's config</param>
        /// <returns>Url is an Ubraco Url</returns>
        bool IsUmbracoUrl(UmbracoUrl umbracoUrl);
    }
}
