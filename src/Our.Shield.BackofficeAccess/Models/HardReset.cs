[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Our.Shield.BackofficeAccess.Models.HardReset), nameof(Our.Shield.BackofficeAccess.Models.HardReset.Start))]
namespace Our.Shield.BackofficeAccess.Models
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using Core.Models;
    using Core.Persistance.Business;

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

                var webConfig = new WebConfigFileHandler();
                var curUmbVersion = Umbraco.Core.Configuration.UmbracoVersion.GetSemanticVersion().ToString();

                if(!curUmbVersion.Equals(webConfig.UmbracoVersion, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }

                if (!Directory.Exists(resetter.HardLocation))
                {
                    if(resetter.EnvironmentId.HasValue)
                    {
                        var journal = new JournalMessage($"Unable to Rename and/or move directory from: {resetter.HardLocation} to: {resetter.SoftLocation}\nThe directory {resetter.HardLocation} cannot be found");
                        DbContext.Instance.Journal.Write(resetter.EnvironmentId.Value, nameof(BackofficeAccess), journal);
                    }

                    return;
                }

                if(Directory.Exists(resetter.SoftLocation))
                {
                    Directory.Delete(resetter.SoftLocation, true);
                }

                Directory.Move(resetter.HardLocation, resetter.SoftLocation);

                webConfig.UmbracoPath = "~/" + Path.GetFileName(resetter.SoftLocation);
                var paths = webConfig.UmbracoReservedPaths;

                var regex = new Regex("(~?)(/?)" + Path.GetFileName(resetter.HardLocation) + "(/?)", RegexOptions.IgnoreCase);
                
                for (var i = 0; i < paths.Length; i++)
                {
                    if(regex.IsMatch(paths[i]))
                    {
                        paths[i] = regex.Replace(paths[i], webConfig.UmbracoPath);
                    }
                }

                webConfig.UmbracoReservedPaths = paths;

                resetter.Delete();
                webConfig.Save();

                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
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
            private const string File = "web.config";

            private string FilePath
            {
                get
                {
                    return AppDomain.CurrentDomain.BaseDirectory + File;
                }
            }

            private XDocument webConfig;


            public string UmbracoPath
            {
                get
                {
                    if(webConfig == null)
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
                    if(webConfig == null)
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

            public string UmbracoVersion
            {
                get
                {
                    if(webConfig == null)
                    {
                        Load();
                    }

                    return webConfig.XPathSelectElement("/configuration/appSettings/add[@key='umbracoConfigurationStatus']").Attribute("value").Value;
                }
            }

            private void Load()
            {
                webConfig = XDocument.Load(FilePath);
            }

            public void Save()
            {
                webConfig.Save(FilePath);
            }
        }
    }
}
