namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public interface IEnvironment
    {
        [JsonProperty("id")]
        int Id { get; }

        [JsonProperty("name")]
        string Name { get; }

        [JsonProperty("domains")]
        IEnumerable<IDomain> Domains { get; }
    }
}
