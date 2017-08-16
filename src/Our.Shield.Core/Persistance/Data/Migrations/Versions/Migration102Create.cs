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
    [Migration("1.0.2", 1, nameof(Shield))]
    internal class Migration102Create : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration102Create(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            if (_schemaHelper.TableExist<Dto.Environment.Environment100>())
            {
                var sql = new Sql().Where("id != 1");
                _database.Delete<Dto.Environment.Environment100>(sql);

                Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.SortOrder)).AsInt32().NotNullable().WithDefaultValue(0);
                Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.Enable)).AsBoolean().NotNullable().WithDefaultValue(true);
                Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.ContinueProcessing)).AsBoolean().NotNullable().WithDefaultValue(true);
                Alter.Table<Dto.Environment.Environment102>().AddColumn(nameof(Dto.Environment.Environment102.ColorIndicator)).AsString(7).NotNullable().WithDefaultValue("#df7f48");
            }
            else
            {
                _schemaHelper.CreateTable<Dto.Environment.Environment102>();
                Context.Database.Insert(new Dto.Environment.Environment102
                {
                    Name = "Default",
                    Icon = "icon-firewall red",
                    Enable = true,
                    ContinueProcessing = true,
                    SortOrder = 0,
                    ColorIndicator = "#df7f48"
                });
            }

            //  Domain
            if (!_schemaHelper.TableExist<Dto.Domain.Domain100>())
            {
                _schemaHelper.CreateTable<Dto.Domain.Domain100>();
            }

            //  Configuration
            if (!_schemaHelper.TableExist<Dto.Configuration.Configuration100>())
            {
                _schemaHelper.CreateTable<Dto.Configuration.Configuration102>();
            }

            //  Journal
            if (_schemaHelper.TableExist<Dto.Journal.Journal100>())
            {
                Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>();
                Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal102.Datestamp)).OnTable<Dto.Journal.Journal102>()
                    .OnColumn(nameof(Dto.Journal.Journal102.Datestamp)).Ascending().WithOptions().NonClustered();
            }
            else
            {
                _schemaHelper.CreateTable<Dto.Journal.Journal102>();
            }
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
            Delete.Column(nameof(Dto.Environment.Environment102.ColorIndicator)).FromTable<Dto.Environment.Environment102>();

            //  Journal
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>();
            Create.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Dto.Configuration))
                .FromTable<Dto.Configuration.Configuration101>().ForeignColumn(nameof(Dto.Configuration.Configuration101.AppId))
                .ToTable<Dto.Journal.Journal101>().PrimaryColumn(nameof(Dto.Journal.Journal101.AppId)).OnDeleteOrUpdate(Rule.None);

            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration102.AppId)).OnTable<Dto.Configuration.Configuration102>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration101.AppId)).OnTable<Dto.Configuration.Configuration101>()
                .OnColumn(nameof(Dto.Configuration.Configuration101.AppId)).Ascending().WithOptions().Unique();
        }
    }
}
