using Our.Shield.Core.Settings;
using System;
using System.Configuration;
using System.Web.Configuration;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.PackageActions;

namespace Our.Shield.Core.PackageActions
{
    /// <summary>
    /// Package action to add Shield's Configuration when installed via Umbraco's Package Manager
    /// </summary>
    public class AddShieldConfigurationSection : IPackageAction
    {
        private readonly ILogger _logger;

        public AddShieldConfigurationSection(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public bool Execute(string packageName, XElement xmlData)
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
                _logger.Error<AddShieldConfigurationSection>("Error at execute AddShieldConfigurationSection package action", ex);
            }

            return false;
        }

        /// <inheritdoc />
        public string Alias()
        {
            return nameof(AddShieldConfigurationSection);
        }

        /// <inheritdoc />
        public bool Undo(string packageName, XElement xmlData)
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
                _logger.Error<AddShieldConfigurationSection>("Error at undo AddShieldConfigurationSection package action", ex);
            }

            return false;
        }
    }
}
