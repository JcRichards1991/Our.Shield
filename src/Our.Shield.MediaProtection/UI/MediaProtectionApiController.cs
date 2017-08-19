using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Shield.MediaProtection.UI
{
    [PluginController(Core.UI.Constants.App.Alias)]
    public class MediaProtectionApiController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public string[] GetDirectories()
        {
            var retList = new List<string>();
            var rootPath = HttpRuntime.AppDomainAppPath;
            var directories = Directory.GetDirectories(rootPath);

            for (var i = 0; i < directories.Length; i++)
            {
                directories[i] = $"/{Path.GetFileName(directories[i])}/";
            }

            return directories;
        }
    }
}
