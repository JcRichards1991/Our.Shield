using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Shield.Core.Data.Dtos
{
    [TableName(nameof(Shield) + "Environments")]
    [PrimaryKey(nameof(Key), AutoIncrement = false)]
    [ExplicitColumns]
    internal class Environment : Dto
    {
        [Column(nameof(Name))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Name { get; set; }

        [Column(nameof(Icon))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Icon { get; set; }

        [Column(nameof(SortOrder))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int SortOrder { get; set; }

        [Column(nameof(Enabled))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Enabled { get; set; }

        [Column(nameof(ContinueProcessing))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool ContinueProcessing { get; set; }

        [Column(nameof(Domains))]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Domains { get; set; }
    }
}
