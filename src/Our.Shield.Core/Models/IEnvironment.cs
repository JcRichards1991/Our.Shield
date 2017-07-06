namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// Environment interface 
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// The Id of the Environment
        /// </summary>
        [JsonProperty("id")]
        int Id { get; }

        /// <summary>
        /// The Name of the Environment
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// The Domains for the Environment
        /// </summary>
        [JsonProperty("domains")]
        IEnumerable<IDomain> Domains { get; }
    }
}
