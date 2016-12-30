using Newtonsoft.Json;
using System;

namespace Shield.Persistance.UmbracoAccess
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration : Bal.JsonValues
    {
        [JsonProperty]
        public string BackendAccessUrl { get; set; }

        //public int StatusCode { get; set; }
    }
}
