namespace Our.Shield.Core.Persistance.Data.Migrations.Dto.Domain
{
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// Defines the Domain table
    /// </summary>
    [TableName(nameof(Shield) + nameof(Data.Dto.Domain))]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Domain100
    {
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the Environment Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(Dto.Environment.Environment100), Name = "FK_" + nameof(Shield) + "_" + nameof(Domain100) + "_" + nameof(Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(EnvironmentId))]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// Url of the domain
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the umbracoDomain Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof (Data.Dto.UmbracoDomainDto), Name = "FK_" + nameof(Shield) + "_" + nameof(Domain100) + "_" + nameof(Data.Dto.UmbracoDomainDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(UmbracoDomainId))]
        public int? UmbracoDomainId { get; set; }

    }
}
