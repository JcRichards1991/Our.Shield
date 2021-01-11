using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDto
    {
        /// <summary>
        /// Unique Identifier of the <see cref="IDto"/>
        /// </summary>
        [Column(nameof(Key))]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        Guid Key { get; }

        /// <summary>
        /// Last Modified Date of the <see cref="IDto"/> 
        /// </summary>
        [Column(nameof(LastModifiedDateUtc))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        DateTime LastModifiedDateUtc { get; }
    }
}
