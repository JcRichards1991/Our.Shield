using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Persistance.Data.Migrations.Dto.Configuration
{
    /// <summary>
    /// Defines the Configuration table
    /// </summary>
    [TableName(nameof(Shield) + nameof(Data.Dto.Configuration))]
    [PrimaryKey("Id", autoIncrement = true)]
    internal class Configuration103
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
        /// Gets or sets the Environment Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey(typeof(Dto.Environment.Environment103), Name = "FK_" + nameof(Shield) + "_" + nameof(Configuration) + "_" + nameof(Data.Dto.Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(EnvironmentId))]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets the Environment Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the Environment Id
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the Value (should be json)
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(4000)]
        public string Value { get; set; }
    }
}
