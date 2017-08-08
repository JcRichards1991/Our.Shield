using System.Data;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.Core.Persistance.Data.Migrations.Versions
{
    /// <summary>
    /// Handles Creating/Editing the Configuration table
    /// </summary>
    [Migration("1.0.1", 1, nameof(Shield))]
    internal class Migration101 : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration101(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration100.AppId)).OnTable<Dto.Configuration.Configuration100>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration101.AppId)).OnTable<Dto.Configuration.Configuration101>()
                .OnColumn(nameof(Dto.Configuration.Configuration101.AppId)).Unique();            

            //  Journal
            Create.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Dto.Configuration))
                .FromTable<Dto.Configuration.Configuration101>().ForeignColumn(nameof(Dto.Configuration.Configuration101.AppId))
                .ToTable<Dto.Journal.Journal101>().PrimaryColumn(nameof(Dto.Journal.Journal101.AppId)).OnDeleteOrUpdate(Rule.None);
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>();
        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration101.AppId)).OnTable<Dto.Configuration.Configuration101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration100.AppId)).OnTable<Dto.Configuration.Configuration100>()
                .OnColumn(nameof(Dto.Configuration.Configuration100.AppId)).Ascending().WithOptions().NonClustered();

            //  Journal
            Delete.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Dto.Configuration)).OnTable<Dto.Journal.Journal101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>()
                .OnColumn(nameof(Dto.Journal.Journal100.Datestamp)).Ascending().WithOptions().Clustered();
        }
    }
}
