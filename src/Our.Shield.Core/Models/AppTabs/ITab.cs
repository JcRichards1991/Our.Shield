using Newtonsoft.Json;

namespace Our.Shield.Core.Models.AppTabs
{
    /// <summary>
    /// Represents a Tab in the UI display
    /// </summary>
    public interface ITab
    {
        /// <summary>
        /// The id of the <see cref="ITab"/>
        /// </summary>
        [JsonProperty("id")]
        int Id { get; }

        /// <summary>
        /// The name for the <see cref="ITab"/>
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// Whether or not this <see cref="ITab"/> is the active tab
        /// </summary>
        [JsonProperty("active")]
        bool Active { get; }

        /// <summary>
        /// The view for the <see cref="ITab"/>
        /// </summary>
        [JsonProperty("view")]
        string View { get; }

        /// <summary>
        /// Icon for the <see cref="ITab"/>
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }
    }
}
