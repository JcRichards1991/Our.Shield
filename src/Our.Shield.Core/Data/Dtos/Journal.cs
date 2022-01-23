using NPoco;
using Our.Shield.Core.Models;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    [TableName(nameof(Shield) + "Journals")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    internal class Journal : Dto, IJournal
    {
        /// <inheritdoc />
        [Column(nameof(EnvironmentKey))]
        [ForeignKey(typeof(Environment), Name = "FK_" + nameof(Journal) + "_" + nameof(Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(EnvironmentKey))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid EnvironmentKey { get; set; }

        /// <inheritdoc />
        [Column(nameof(AppKey))]
        [ForeignKey(typeof(App), Name = "FK_" + nameof(Journal) + "_" + nameof(App))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(AppKey))]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? AppKey { get; set; }

        /// <inheritdoc />
        [Column(nameof(Message))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(4000)]
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Ignore]
        public DateTime DateStamp => LastModifiedDateUtc;
    }
}
