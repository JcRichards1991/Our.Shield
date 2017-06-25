namespace Shield.Core.Persistance.Data.Dto
{
    using System;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// Defines the Journal database table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Journal))]
    public class Journal
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the App Name.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(256)]
        [ForeignKey (typeof (Operation.IApp), Name = "FK_" + nameof(Shield) + "_" + nameof(Journal) + "_" + nameof(Operation.IApp))]
        [IndexAttribute (IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(AppId))]
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the Domain Id.
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [ForeignKey (typeof (Domain), Name = "FK_" + nameof(Shield) + "_" + nameof(Journal) + "_" + nameof(Domain))]
        [IndexAttribute (IndexTypes.NonClustered, Name = "IX_" + nameof(Shield) + "_" + nameof(DomainId))]
        public int DomainId { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (Should be json).
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Value { get; set; }
    }
}
