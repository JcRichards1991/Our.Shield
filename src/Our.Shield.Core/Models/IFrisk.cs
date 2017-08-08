using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Fisk interface
    /// </summary>
    public interface IFrisk
    {
        /// <summary>
        /// The Id of Frisk
        /// </summary>
        [JsonProperty("id")]
        string Id { get; }
    }
}
