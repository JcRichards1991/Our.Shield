using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Domain interface
    /// </summary>
    public interface IDomain
    {
        /// <summary>
        /// The Fully Qualified URL of the <see cref="Domain"/>
        /// </summary>
        [JsonProperty("fullyQualifiedUrl")]
        string FullyQualifiedUrl { get; }
    }
}
