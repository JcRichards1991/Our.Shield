using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
