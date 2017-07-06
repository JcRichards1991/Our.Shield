namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;

    public interface IDomain
    {
        [JsonProperty("name")]
        string Name { get; }
    }
}
