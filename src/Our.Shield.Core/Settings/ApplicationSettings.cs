using System.Configuration;
using Umbraco.Core;

namespace Our.Shield.Core.Settings
{
    /// <summary>
    /// Class for returning values from the app settings section of the web.config
    /// </summary>
    internal static class ApplicationSettings
    {
        private static string GetAppKeyValue(string key, string fallback = null)
        {
            return string.IsNullOrEmpty(ConfigurationManager.AppSettings[key])
                ? fallback
                : ConfigurationManager.AppSettings[key];
        }

        private static string _umbracoPath;
        /// <summary>
        /// Gets the Umbraco Path app setting value
        /// </summary>
        public static string UmbracoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_umbracoPath))
                    return _umbracoPath;

                var path = GetAppKeyValue("umbracoPath", "~/umbraco");
                path = Umbraco.Core.IO.IOHelper.ResolveUrl(path);
                return _umbracoPath = path.EnsureEndsWith('/');
            }
        }

        private static string _umbracoVersion;
        /// <summary>
        /// The current installed version of umbraco
        /// </summary>
        public static string UmbracoVersion
        {
            get
            {
                if (!string.IsNullOrEmpty(_umbracoVersion))
                    return _umbracoVersion;

                return _umbracoVersion = GetAppKeyValue("umbracoConfigurationStatus");
            }
        }
    }
}
