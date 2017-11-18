using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <summary>
    /// Domain class
    /// </summary>
    internal class Domain : IDomain
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// The name of the Domain
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("umbracoDomainId")]
        public int? UmbracoDomainId { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("environmentId")]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal Domain() { }

        /// <summary>
        /// Constructor for turning a Domain DTO to this Domain object
        /// </summary>
        /// <param name="data">The DTO object from the database</param>
        // ReSharper disable once SuggestBaseTypeForParameter
        internal Domain(Persistance.Data.Dto.Domain data)
        {
            Id = data.Id;
            Name = data.Name;
            UmbracoDomainId = data.UmbracoDomainId;
            EnvironmentId = data.EnvironmentId;
        }

        /// <summary>
        /// Checks whether or not two Domains are the same
        /// </summary>
        /// <param name="other">The object to test against</param>
        /// <returns>True if equals; Otherwise, False</returns>
        public override bool Equals(object other)
        {
            if (!(other is Domain otherDomain))
            {
                return false;
            }
            return Name == otherDomain.Name;
        }

        /// <summary>
        /// Gets the HashCode of the Domain
        /// </summary>
        /// <returns>The Domain's Name HashCode</returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Name.GetHashCode();
        }
    }
}
