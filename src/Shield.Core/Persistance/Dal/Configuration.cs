using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Shield.Core.Persistance.Dal
{
    /// <summary>
    /// Defines the Configuration table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Configuration))]
    [PrimaryKey("Id", autoIncrement = false)]
    internal class Configuration
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [PrimaryKeyColumn(Name = "PK_id", AutoIncrement = false)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(255)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Last Modified date.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (should be json).
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(4000)]
        public string Value { get; set; }
    }
}
