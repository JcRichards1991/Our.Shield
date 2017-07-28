namespace Our.Shield.Core.Persistance.Data.Migrations
{
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Handles Creating/Editing the Environment table
    /// </summary>
    [Migration("1.0.1", 1, nameof(Shield))]
    internal class EnvironmentMigration : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Environment Migration
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public EnvironmentMigration(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Environment table & adds a default Environment
        /// </summary>
        public override void Up()
        {
            _schemaHelper.CreateTable<Dto.Environment>(false);

            Context.Database.Insert(new Dto.Environment
            {
                Name = "Default",
                Icon = "icon-firewall red"
            });
        }

        /// <summary>
        /// Drops the Environment table
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Dto.Environment>();
        }
    }
}
