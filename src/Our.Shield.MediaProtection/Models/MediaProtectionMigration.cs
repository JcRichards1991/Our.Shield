namespace Our.Shield.MediaProtection.Models
{
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Handles Creating/Editing the Configuration table.
    /// </summary>
    [Migration("1.0.0", 1, nameof(Shield) + nameof(MediaProtection))]
    internal class MediaProtectionMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        public bool AddMediaTypes = false;

        /// <summary>
        /// Default constructor for the Configuration Migration
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public MediaProtectionMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Sets the AddMediaTypes Flag
        /// </summary>
        public override void Up()
        {
            AddMediaTypes = true;
        }

        /// <summary>
        /// Placeholder Method
        /// </summary>
        public override void Down()
        {
        }
    }
}
