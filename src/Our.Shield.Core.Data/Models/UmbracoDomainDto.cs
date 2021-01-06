using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Models
{
    [TableName("umbracoDomains")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UmbracoDomainDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }
        
        [Column("domainName")]
        public string DomainName { get; set; }
    }
}
