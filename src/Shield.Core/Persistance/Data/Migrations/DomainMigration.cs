namespace Shield.Core.Persistance.Data.Migrations
{
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Handles Creating/Editing the Domain table.
    /// </summary>
    [Migration("1.0.0", 1, nameof(Shield))]
    public class DomainMigration : MigrationBase
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
        public DomainMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table.
        /// </summary>
        public override void Up()
        {
            _schemaHelper.CreateTable<Dto.Domain>(false);
        }

        /// <summary>
        /// Drops the Configurations table.
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Dto.Domain>();
        }
    }
}
