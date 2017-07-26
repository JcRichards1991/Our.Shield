﻿namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Domain interface
    /// </summary>
    public interface IDomain
    {
        [JsonProperty("id")]
        int Id { get;}

        /// <summary>
        /// The name of the Domain
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("umbracoDomainId")]
        int? UmbracoDomainId { get; }
    }
}
