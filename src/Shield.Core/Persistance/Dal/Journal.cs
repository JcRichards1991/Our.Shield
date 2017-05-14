namespace Shield.Core.Persistance.Dal
{
    using System;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.DatabaseAnnotations;

    /// <summary>
    /// Defines the Journal database table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Journal))]
    internal class Journal
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Configuration Id.
        /// </summary>
        [ForeignKey(typeof(Configuration), Name = "FK_Journal_Configuration")]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_ConfigurationId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ConfigurationId { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (Should be json).
        /// </summary>
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Value { get; set; }
    }
}
