using Elmah;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Our.Shield.Elmah.Models
{
    public class ElmahApiJsonModel
    {
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("errors")]
        public IEnumerable<ElmahErrorJsonModel> Errors { get; set; }
    }

    public class ElmahErrorJsonModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }
    }
}
