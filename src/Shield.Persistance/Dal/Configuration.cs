using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Shield.Persistance.Dal
{
    /// <summary>
    /// Defines the Configuration table.
    /// </summary>
    [TableName("ShieldConfiguration")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    internal class Configuration
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Last Modified date.
        /// </summary>
        [Column("lastmodified")]
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the Value (should be json).
        /// </summary>
        [Column("value")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }
    }

    /// <summary>
    /// Handles Creating/Editing the Configuration table.
    /// </summary>
    [Migration("1.0.0", 1, "Shield")]
    public class ConfigurationMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">
        /// The SQL Syntax.
        /// </param>
        /// <param name="logger">
        /// The Logger.
        /// </param>
        public ConfigurationMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger)
          : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table.
        /// </summary>
        public override void Up()
        {
            _schemaHelper.CreateTable<Configuration>(false);
        }

        /// <summary>
        /// Drops the Configurations table.
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Configuration>();
        }
    }
}
