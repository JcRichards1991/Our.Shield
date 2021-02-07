using NPoco;
using System.Collections.Generic;
using System.Diagnostics;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// DTO Model representation of <see cref="IEnvironment"/> interface
    /// </summary>
    [TableName(nameof(Shield) + "Environments")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    [DebuggerDisplay("Name: {Name}; Enabled: {Enabled}; Key: {Key}")]
    public class Environment : Dto, IEnvironment
    {
        /// <inheritdoc />
        [Column(nameof(Name))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; }

        /// <inheritdoc />
        [Column(nameof(Icon))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Icon { get; set; }

        /// <inheritdoc />
        [Column(nameof(SortOrder))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SortOrder { get; set; }

        /// <inheritdoc />
        [Column(nameof(Enabled))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enabled { get; set; }

        /// <inheritdoc />
        [Column(nameof(ContinueProcessing))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool ContinueProcessing { get; set; }

        /// <inheritdoc />
        [Column(nameof(Domains))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Domains { get; set; }
    }
}
