[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.BackofficeAccess.Models.HardReset), nameof(Our.Shield.BackofficeAccess.Models.HardReset.Start))]
namespace Our.Shield.BackofficeAccess.Models
{
    using Core.Models;
    using Core.Persistance.Business;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;

    internal class HardReset
    {
        public static void Start()
        {
            var resetter = new HardResetFileHandler();

            try
            {
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
                    if(resetter.EnvironmentId.HasValue)
                    {
                        var journal = new JournalMessage($"Unable to Rename directory from: {resetter.HardLocation} to: {resetter.SoftLocation}\nThe directory {resetter.HardLocation} cannot be found");
                        DbContext.Instance.Journal.Write(resetter.EnvironmentId.Value, nameof(BackofficeAccess), journal);
                    }

                    return;
                }

                if(Directory.Exists(resetter.SoftLocation))
                {
                    Directory.Delete(resetter.SoftLocation, true);
                }

                Directory.Move(resetter.HardLocation, resetter.SoftLocation);

                var webConfig = new WebConfigFileHandler();

                webConfig.UmbracoPath = "~/" + Path.GetFileName(resetter.SoftLocation);
                ApplicationSettings.SetUmbracoPath(webConfig.UmbracoPath);

                var paths = webConfig.UmbracoReservedPaths;

                var regex = new Regex("^(~?)(/?)" + Path.GetFileName(resetter.HardLocation) + "(/?)$", RegexOptions.IgnoreCase);
                
                for (var i = 0; i < paths.Length; i++)
                {
                    if(regex.IsMatch(paths[i]))
                    {
                        paths[i] = regex.Replace(paths[i], webConfig.UmbracoPath);
                    }
                }

                webConfig.UmbracoReservedPaths = paths;
                ApplicationSettings.SetUmbracoReservedPaths(string.Join(",", paths));

                resetter.Delete();
                webConfig.Save();
            }
            catch (Exception ex)
            {
                if(resetter.EnvironmentId.HasValue)
                {
                    var journal = new JournalMessage($"Unexpected error occurred, exception:\n{ex.Message}");
                    DbContext.Instance.Journal.Write(resetter.EnvironmentId.Value, nameof(BackofficeAccess), journal);
                }
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


            public string UmbracoPath
            {
                get
                {
                    if (webConfig == null)
                    {
                        Load();
                    }

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
                    if (webConfig == null)
                    {
                        Load();
                    }

                    return webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoReservedPaths']").Attribute("value").Value.Split(',');
                }
                set
                {
                    webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoReservedPaths']").Attribute("value").Value = string.Join(",", value);
                }
            }

            private void Load()
            {
                webConfig = XDocument.Load(filePath);
            }

            public void Save()
            {
                webConfig.Save(filePath);
            }
        }
    }
}
