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
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the Last Modified date.
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (should be json).
        /// </summary>
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Enable {get; set; }
    }
}
