using Newtonsoft.Json;
using Our.Shield.Core.Attributes;

namespace Our.Shield.Core.Models.AppTabs
{
    /// <summary>
    /// The Configuration Tab for an <see cref="IApp"/> to render it's <see cref="IAppConfiguration"/>
    /// </summary>
    internal class AppConfigurationTab : Tab, ITab
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AppConfigurationTab"/>
        /// </summary>
        /// <param name="attr"></param>
        public AppConfigurationTab(AppEditorAttribute attr) : base(attr)
        {
            ConfigurationView = attr.AppView;
        }

        /// <summary>
        /// The view for the <see cref="IAppConfiguration"/>
        /// </summary>
        [JsonProperty("configView")]
        public string ConfigurationView { get; set; }
    }
}
