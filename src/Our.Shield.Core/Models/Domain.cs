namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Domain class
    /// </summary>
    internal class Domain : IDomain
    {
        /// <summary>
        /// The name of the Domain
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="data">
        /// The DTO object from the database
        /// </param>
        internal Domain(Persistance.Data.Dto.Domain data)
        {
            Name = data.Name;
        }

        /// <summary>
        /// Checks whether or not two Domains are the same
        /// </summary>
        /// <param name="other">
        /// The object to test against
        /// </param>
        /// <returns>
        /// True if equals; Otherwise, False
        /// </returns>
        public override bool Equals(object other)
        {
            var otherDomain = other as Domain;
            if (otherDomain == null)
            {
                return false;
            }
            return Name == otherDomain.Name;
        }

        /// <summary>
        /// Gets the HashCode of the Domain
        /// </summary>
        /// <returns>
        /// The Domain's Name HashCode
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }
}
