using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shield.Core.Models
{
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
