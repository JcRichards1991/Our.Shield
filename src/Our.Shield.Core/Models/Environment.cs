using Newtonsoft.Json;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Persistance.Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Environment Class
    /// </summary>
    public class Environment : IEnvironment
    {
        /// <summary>
        /// The Id of the Environment
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The Name of the Environment
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Icon of the Environment
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// The Domains for the Environment
        /// </summary>
        [JsonProperty("domains")]
        public IEnumerable<IDomain> Domains { get; set; }

        [JsonProperty("sortOrder")]
        public int SortOrder { get; set; }

        [JsonProperty("enable")]
        public bool Enable { get; set; }

        [JsonProperty("continueProcessing")]
        public bool ContinueProcessing { get; set; }

        /// <summary>
        /// Checks whether or not two Environments are the same
        /// </summary>
        /// <param name="other">The object to test against</param>
        /// <returns>True if equals; Otherwise, False</returns>
        public override bool Equals(object other)
        {
            if (other is Environment)
            {
                return Id == ((Environment) other).Id;
            }
            if (other is int)
            {
                return Id == ((int) other);
            }
            if (other is string)
            {
                return Id.ToString().Equals(((string) other));
            }
            return false;
        }

        /// <summary>
        /// Gets the Hascode for the Environment
        /// </summary>
        /// <returns>The Environment Id</returns>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal Environment()
        {
        }

        /// <summary>
        /// Creates an Environment object
        /// </summary>
        /// <param name="data">The DTO object to create the Environment</param>
        internal Environment(Persistance.Data.Dto.Environment data)
        {
            Id = (int) data.Id;
            Name = data.Name;
            Icon = data.Icon;
            Domains = data.Domains.Select(x => new Domain(x));
            SortOrder = data.SortOrder;
            Enable = data.Enable;
            ContinueProcessing = data.ContinueProcessing;
        }
    }
}
