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
    [Migration("1.0.2", 1, nameof(Shield))]
    internal class Migration102 : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration102(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.SortOrder)).AsInt32().NotNullable().WithDefaultValue(0);
            Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.Enable)).AsBoolean().NotNullable().WithDefaultValue(true);
            Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.ContinueProcessing)).AsBoolean().NotNullable().WithDefaultValue(false);

            //  Journal
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal102.Datestamp)).OnTable<Dto.Journal.Journal102>()
                .OnColumn(nameof(Dto.Journal.Journal102.Datestamp)).Ascending().WithOptions().Clustered();

        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
            //  Environment
            Delete.Column(nameof(Dto.Environment.Environment102.SortOrder)).FromTable<Dto.Environment.Environment102>();
            Delete.Column(nameof(Dto.Environment.Environment102.Enable)).FromTable<Dto.Environment.Environment102>();
            Delete.Column(nameof(Dto.Environment.Environment102.ContinueProcessing)).FromTable<Dto.Environment.Environment102>();

            //  Journal
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>();
        }
    }
}
