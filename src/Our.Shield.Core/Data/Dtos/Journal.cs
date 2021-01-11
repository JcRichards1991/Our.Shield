using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    [TableName(nameof(Shield) + "Journals")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    internal class Journal : Dto
    {
        [Column(nameof(EnvironmentKey))]
        [ForeignKey(typeof(Environment), Name = "FK_" + nameof(Journal) + "_" + nameof(Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(EnvironmentKey))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid EnvironmentKey { get; set; }

        [Column(nameof(AppKey))]
        [ForeignKey(typeof(App), Name = "FK_" + nameof(Journal) + "_" + nameof(App))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(AppKey))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid AppKey { get; set; }

        [Column(nameof(MessageJson))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string MessageJson { get; set; }
    }
}
