using Our.Shield.Core.Settings;
using System;
using System.Configuration;
using System.Web.Configuration;
using System.Xml;
using umbraco.cms.businesslogic.packager.standardPackageActions;
using umbraco.interfaces;
using Umbraco.Core.Logging;

namespace Our.Shield.Core.PackageActions
{
    public class AddShieldConfigurationSection : IPackageAction
    {
        public bool Execute(string packageName, XmlNode xmlData)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");

                if (config.Sections["shieldConfiguration"] == null)
                {
                    var configSection = new ShieldSection();
                    configSection.SectionInformation.ConfigSource = "config\\shield.config";

                    config.Sections.Add("shieldConfiguration", configSection);
                    configSection.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Full);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<AddShieldConfigurationSection>("Error at execute AddShieldConfigurationSection package action", ex);
            }

            return false;
        }

        public string Alias()
        {
            return nameof(AddShieldConfigurationSection);
        }

        public bool Undo(string packageName, XmlNode xmlData)
        {
            try
            {
                var config = WebConfigurationManager.OpenWebConfiguration("~");

                if (config.Sections["shieldConfiguration"] != null)
                {
                    config.Sections.Remove("shieldConfiguration");
                    config.Save(ConfigurationSaveMode.Full);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error<AddShieldConfigurationSection>("Error at undo AddShieldConfigurationSection package action", ex);
            }

            return false;
        }

        public XmlNode SampleXml()
        {
            var sample = "<Action runat=\"install\" undo=\"true\" alias=\"AddShieldConfigurationSection\" />";
            return helper.parseStringToXmlNode(sample);
        }

    }
}
