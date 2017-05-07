using System.Configuration;

namespace Shield.UmbracoAccess
{
    public static class ApplicationSettings
    {
        public static string GetUmbracoPath()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["umbracoPath"]))
                return Constants.Defaults.BackendAccessUrl;


            var url = ConfigurationManager.AppSettings["umbracoPath"];

            if(url.StartsWith("~"))
            {
                url = url.Remove(0, 1);
            }

            if(!url.StartsWith("/"))
            {
                url = "/" + url;
            }

            return url;
        }
    }
}
