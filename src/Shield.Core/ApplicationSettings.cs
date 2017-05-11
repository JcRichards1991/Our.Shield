namespace Shield.Core
{
    using System.Configuration;

    /// <summary>
    /// Class for returning values from the app settings section of the web.config
    /// </summary>
    public static class ApplicationSettings
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

                var url = GetAppKeyValue("umbracoPath", "/umbraco");

                if (url.StartsWith("~"))
                {
                    url = url.Remove(0, 1);
                }

                if (!url.StartsWith("/"))
                {
                    url = "/" + url;
                }

                return umbracoPath = url;
            }
        }
    }
}
