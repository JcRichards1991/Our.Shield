using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Shield.Core.Persistance.Dal
{
    /// <summary>
    /// Defines the Journal database table.
    /// </summary>
    [TableName(nameof(Shield) + nameof(Journal))]
    [ExplicitColumns]
    internal class Journal
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Column(nameof(Id))]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Configuration Id.
        /// </summary>
        [Column(nameof(ConfigurationId))]
        [ForeignKey(typeof(Configuration), Name = "FK_Journal_Configuration")]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_ConfigurationId")]
        public string ConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the Create Date of the Journal.
        /// </summary>
        [Column(nameof(CreateDate))]
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (Should be json).
        /// </summary>
        [Column(nameof(Value))]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }
    }
}
