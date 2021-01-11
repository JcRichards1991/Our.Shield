using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    internal abstract class Dto
    {
        [Column(nameof(Key))]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid Key { get; set; }

        [Column(nameof(LastModifiedDateUtc))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime LastModifiedDateUtc { get; set; }
    }
}
