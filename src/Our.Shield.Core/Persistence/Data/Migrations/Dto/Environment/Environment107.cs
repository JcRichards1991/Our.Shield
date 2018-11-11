using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Persistence.Data.Migrations.Dto.Environment
{
    /// <summary>
    /// Defines the Configuration table
    /// </summary>
    [TableName(nameof(Shield) + nameof(Environment))]
    [PrimaryKey("Id", autoIncrement = true)]
    public class Environment107
    {
        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Id { get; set; }

        /// <summary>
        /// Unique Identifier
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(256)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Icon
        /// </summary>
        [Length(256)]
        public string Icon { get; set; }
        
        [Ignore]
        public IEnumerable<Data.Dto.Domain> Domains { get; set; }

        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SortOrder { get; set; }
        
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enable { get; set; }
        
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool ContinueProcessing { get; set; }
        
        [Length(7)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ColorIndicator { get; set; }
    }
}