using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Domain interface
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// the Id of the Domain
        /// </summary>
        [JsonProperty("id")]
        int Id { get;}

        /// <summary>
        /// The name of the Domain
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// The Umbraco Domain Id from the UmbracoDomain Table
        /// </summary>
        [JsonProperty("umbracoDomainId")]
        int? UmbracoDomainId { get; }

        /// <summary>
        /// The Environment this Domain is for
        /// </summary>
        [JsonProperty("environmentId")]
        int EnvironmentId { get; }
    }
}
