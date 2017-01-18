using Semver;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Web;

namespace Shield.Persistance.Dal
{
    /// <summary>
    /// Defines the Journal database table.
    /// </summary>
    [TableName("ShieldJournal")]
    [ExplicitColumns]
    internal class Journal
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Configuration Id.
        /// </summary>
        [Column("configuration")]
        [ForeignKey(typeof(Configuration), Name = "FK_Journal_Configuration")]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_ConfigurationId")]
        public Guid ConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the Create Date of the Journal.
        /// </summary>
        [Column("createdate")]
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (Should be json).
        /// </summary>
        [Column("value")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }
    }

    /// <summary>
    /// Handles Creating/Editing the Journal table.
    /// </summary>
    [Migration("1.0.0", 2, "Shield")]
    public class JournalMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Journal Migration.
        /// </summary>
        /// <param name="sqlSyntax">
        /// The SQL Syntax.
        /// </param>
        /// <param name="logger">
        /// The Logger.
        /// </param>
        public JournalMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger)
          : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Journal Table.
        /// </summary>
        public override void Up()
        {
            _schemaHelper.CreateTable<Journal>(false);
        }

        /// <summary>
        /// Drops the Journal Table.
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Journal>();
        }
    }
}
