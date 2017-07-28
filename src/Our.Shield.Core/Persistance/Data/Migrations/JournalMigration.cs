namespace Our.Shield.Core.Persistance.Data.Migrations
{
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Handles Creating/Editing the Journal table
    /// </summary>
    [Migration("1.0.1", 1, nameof(Shield))]
    internal class JournalMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Journal Migration
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public JournalMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Journal Table
        /// </summary>
        public override void Up()
        {
            _schemaHelper.CreateTable<Dto.Journal>(false);
        }

        /// <summary>
        /// Drops the Journal Table
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Dto.Journal>();
        }
    }
}
