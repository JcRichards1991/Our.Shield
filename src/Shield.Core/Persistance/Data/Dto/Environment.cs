namespace Shield.Core.Persistance.Data.Dto
{
    using System;
    using System.Collections.Generic;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// Defines the Configuration table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Environment))]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Environment
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(4000)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Icon.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(4000)]
        public string Icon { get; set; }

        [Ignore]
        public IList<Domain> Domains { get; set; }
    }
}
