using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// Base DTO
    /// </summary>
    public abstract class Dto : IDto
    {
        /// <inheritdoc />
        [Column(nameof(Key))]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid Key { get; set; }

        /// <inheritdoc />
        [Column(nameof(LastModifiedDateUtc))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LastModifiedDateUtc { get; internal set; }
    }
}
