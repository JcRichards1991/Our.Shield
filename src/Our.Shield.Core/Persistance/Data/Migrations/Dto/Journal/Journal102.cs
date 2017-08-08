using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Persistance.Data.Migrations.Dto.Journal
{
    /// <summary>
    /// Defines the Journal database table
    /// </summary>
    [TableName(nameof(Shield) + nameof(Data.Dto.Journal))]
    [PrimaryKey("Id", autoIncrement = true)]
    internal class Journal102
    {
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the App Name
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(256)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(AppId))]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the Domain Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(Dto.Environment.Environment102), Name = "FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(EnvironmentId))]
        public int EnvironmentId { get; set; }
        
        /// <summary>
        /// The Date stamp of the journal
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(Datestamp))]
        public DateTime Datestamp { get; set; }

        /// <summary>
        /// Gets or sets the Value (Should be json)
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(4000)]
        public string Value { get; set; }
    }
}
