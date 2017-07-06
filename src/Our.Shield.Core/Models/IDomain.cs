namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Domain interface
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// The name of the Domain
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }
    }
}
