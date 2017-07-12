namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;

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
