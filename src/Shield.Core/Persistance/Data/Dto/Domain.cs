namespace Shield.Core.Persistance.Data.Dto
{
    using System;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// Defines the Domain table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Domain))]
    internal class Domain
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the Environment Id.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey (typeof (Environment), Name = "FK_" + nameof(Shield) + "_" + nameof(Domain) + "_" + nameof(Environment))]
        [IndexAttribute (IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(EnvironmentId))]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// Url of the domain
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the umbracoDomain Id.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey (typeof (UmbracoDomainDto), Name = "FK_" + nameof(Shield) + "_" + nameof(Domain) + "_" + nameof(UmbracoDomainDto))]
        [IndexAttribute (IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(UmbracoDomainId))]
        public int? UmbracoDomainId { get; set; }

    }
}
