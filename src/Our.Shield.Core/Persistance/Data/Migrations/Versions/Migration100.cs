namespace Our.Shield.Core.Persistance.Data.Migrations.Versions
{
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Persistence;
    using Umbraco.Core.Persistence.Migrations;
    using Umbraco.Core.Persistence.SqlSyntax;

    /// <summary>
    /// Handles Creating/Editing the Configuration table
    /// </summary>
    [Migration("1.0.0", 1, nameof(Shield))]
    internal class Migration100 : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration100(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            _schemaHelper.CreateTable<Dto.Environment.Environment100>(false);
            if(!Business.DbContext.Instance.Environment.Read().Any())
            {
                Context.Database.Insert(new Dto.Environment.Environment100
                {
                    Name = "Default",
                    Icon = "icon-firewall red"
                });
            }

            //  Domain
            _schemaHelper.CreateTable<Dto.Domain.Domain100>(false);

            //  Configuration
            _schemaHelper.CreateTable<Dto.Configuration.Configuration100>(false);

            //  Journal
            _schemaHelper.CreateTable<Dto.Journal.Journal100>(false);

        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
            _schemaHelper.DropTable<Dto.Journal.Journal100>();
            _schemaHelper.DropTable<Dto.Configuration.Configuration100>();
            _schemaHelper.DropTable<Dto.Domain.Domain100>();
            _schemaHelper.DropTable<Dto.Environment.Environment100>();
        }
    }
}
