namespace Our.Shield.Core.Persistance.Data.Dto
{
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// 
    /// </summary>
    [TableName("umbracoDomains")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UmbracoDomainDto
    {
        /// <summary>
        /// 
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column("domainName")]
        public string DomainName { get; set; }
    }
}
