using Newtonsoft.Json;

namespace Shield.Core.Models
{
    public interface IDomain
    {
        [JsonProperty("name")]
        string Name { get; }
    }
}
