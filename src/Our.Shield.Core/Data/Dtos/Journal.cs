using NPoco;
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
    public class Journal : Dto, IJournal
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
        public Guid AppKey { get; set; }

        /// <inheritdoc />
        [Column(nameof(MessageJson))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string MessageJson { get; set; }
    }
}
