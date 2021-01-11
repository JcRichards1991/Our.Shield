using NPoco;
using System;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    [TableName(nameof(Shield) + "Apps")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    public class App : Dto
    {
        /// <summary>
        /// 
        /// </summary>
        [Column(nameof(AppId))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(AppId))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(nameof(EnvironmentKey))]
        [ForeignKey(typeof(Environment), Name = "FK_" + nameof(App) + "_" + nameof(Environment))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + nameof(EnvironmentKey))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid EnvironmentKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(nameof(Enabled))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(nameof(Configuration))]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Configuration { get; set; }
    }
}
