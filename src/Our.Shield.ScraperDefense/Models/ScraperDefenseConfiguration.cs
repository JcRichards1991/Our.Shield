using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.ScraperDefense.Models
{
    public class ScraperDefenseConfiguration : Configuration
    {
        [JsonProperty("requestPerMinute")]
        public int RequestPerMinute { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }
}
