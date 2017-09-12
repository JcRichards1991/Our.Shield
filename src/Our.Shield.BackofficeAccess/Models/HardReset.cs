using Our.Shield.Core.Models;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Logging;
using System.Threading;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.BackofficeAccess.Models.HardReset), nameof(Our.Shield.BackofficeAccess.Models.HardReset.Start))]
namespace Our.Shield.BackofficeAccess.Models
{
    internal class HardReset
    {
        public static void Start()
        {
            var resetter = new HardResetFileHandler();

            if (resetter.SoftLocation == null)
            {
                return;
            }

            var curUmbVersion = Umbraco.Core.Configuration.UmbracoVersion.GetSemanticVersion().ToString();

            if(!curUmbVersion.Equals(ApplicationSettings.UmbracoVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (!Directory.Exists(resetter.HardLocation))
            {
                LogHelper.Error<HardReset>($"Unable to find directory at location {resetter.HardLocation} for renaming to {Path.GetFileName(resetter.SoftLocation)}", null);
                return;
            }

            if (Directory.Exists(resetter.SoftLocation))
            {
                try
                {
                    DeleteDirectory(resetter.SoftLocation);
                }
                catch(Exception ex)
                {
                    LogHelper.Error<HardReset>($"Unable to delete directory {resetter.SoftLocation}", ex);
                    return;
                }
            }

            var webConfig = new WebConfigFileHandler();

            webConfig.UmbracoPath = "~/" + Path.GetFileName(resetter.SoftLocation);
            ConfigurationManager.AppSettings.Set("umbracoPath", webConfig.UmbracoPath);

            var paths = webConfig.UmbracoReservedPaths;

            var regex = new Regex("^(~?)(/?)" + Path.GetFileName(resetter.HardLocation) + "(/?)$", RegexOptions.IgnoreCase);

            for (var i = 0; i < paths.Length; i++)
            {
                if (regex.IsMatch(paths[i]))
                {
                    paths[i] = regex.Replace(paths[i], webConfig.UmbracoPath);
                }
            }

            webConfig.UmbracoReservedPaths = paths;
            ConfigurationManager.AppSettings.Set("umbracoReservedPaths", string.Join(",", paths));

            webConfig.SetLocationPath(Path.GetFileName(resetter.HardLocation).Trim('/'), webConfig.UmbracoPath.TrimStart('~', '/').TrimEnd('/'));

            try
            {
                Directory.Move(resetter.HardLocation, resetter.SoftLocation);
            }
            catch(Exception ex)
            {
                LogHelper.Error<HardReset>($"Unable to rename directory from {resetter.SoftLocation} to {resetter.HardLocation}", ex);
                return;
            }

            try
            {
                resetter.Delete();
            }
            catch(Exception ex)
            {
                LogHelper.Error<HardReset>($"Unable to delete the Hard Resetter File located at {resetter.FilePath}", ex);
                return;
            }

            try
            {
                webConfig.Save();
            }
            catch(Exception ex)
            {
                LogHelper.Error<HardReset>($"Failed to save changes to the website's web.config file", ex);
                Directory.Move(resetter.SoftLocation, resetter.HardLocation);
            }
        }

        private static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            try
            {
                Thread.Sleep(1);
                Directory.Delete(target_dir, true);
            }
            catch (Exception)
            {
                //  Swallow exception, we will try and delete folder another day
            }
        }

        internal class WebConfigFileHandler
        {
            private const string file = "web.config";

            private string filePath
            {
                get
                {
                    return AppDomain.CurrentDomain.BaseDirectory + file;
                }
            }

            private XDocument webConfig;

            public WebConfigFileHandler()
            {
                webConfig = XDocument.Load(filePath);
            }


            public string UmbracoPath
            {
                get
                {
                    return webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoPath']").Attribute("value").Value;
                }
                set
                {
                    webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoPath']").Attribute("value").Value = value;
                }
            }
            public string[] UmbracoReservedPaths
            {
                get
                {
                    return webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoReservedPaths']").Attribute("value").Value.Split(',');
                }
                set
                {
                    webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoReservedPaths']").Attribute("value").Value = string.Join(",", value);
                }
            }

            public void SetLocationPath(string currentLocation, string newLocation)
            {
                var element = webConfig.XPathSelectElement("/configuration/location[@path='" + currentLocation + "']");
                if (element != null)
                {
                    element.Attribute("path").Value = newLocation;
                }
            }

            public void Save()
            {
                using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    fileStream.SetLength(0);
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + webConfig.ToString());
                    }
                }
            }
        }
    }
}
