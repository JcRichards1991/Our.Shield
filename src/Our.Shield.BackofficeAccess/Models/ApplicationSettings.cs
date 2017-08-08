using System.Configuration;
using Umbraco.Core;

namespace Our.Shield.BackofficeAccess.Models
{
    /// <summary>
    /// Class for returning values from the app settings section of the web.config
    /// </summary>
    internal static class ApplicationSettings
    {
        private static string GetAppKeyValue(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]))
            {
                return fallback;
            }

            return ConfigurationManager.AppSettings[key];
        }

        private static string umbracoPath;

        /// <summary>
        /// Gets the Umbraco Path app setting value
        /// </summary>
        public static string UmbracoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(umbracoPath))
                    return umbracoPath;

                var path = GetAppKeyValue("umbracoPath", "~/umbraco");
                path = Umbraco.Core.IO.IOHelper.ResolveUrl(path);
                return umbracoPath = path.EnsureEndsWith('/');
            }
        }

        private static string umbracoVersion;

        public static string UmbracoVersion
        {
            get
            {
                if (!string.IsNullOrEmpty(umbracoVersion))
                    return umbracoVersion;

                return umbracoVersion = GetAppKeyValue("umbracoConfigurationStatus");
            }
        }

        public static void SetUmbracoPath(string newUmbracoPath)
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", newUmbracoPath);
                        
            var path = Umbraco.Core.IO.IOHelper.ResolveUrl(newUmbracoPath);
            umbracoPath = path.EnsureEndsWith('/');
        }

        public static void SetUmbracoReservedPaths(string newUmbracoReservedPaths)
        {
            ConfigurationManager.AppSettings.Set("umbracoReservedPaths", newUmbracoReservedPaths);
        }
    }
}
