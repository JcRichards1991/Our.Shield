using Our.Shield.Core.Persistence.Data.Dto;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Configuration;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Domain;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Environment;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Journal;
using System.Data;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Constants = Our.Shield.Core.UI.Constants;

namespace Our.Shield.Core.Persistence.Data.Migrations.Versions
{
    /// <summary>
    /// Handles Creating/Editing the Configuration table
    /// </summary>
    [Migration("1.0.3", 1, nameof(Shield))]
    internal class Migration103Create : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration103Create(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            if (_schemaHelper.TableExist<Environment100>())
            {
                var sql = new Sql().Where("id != 1");
                _database.Delete<Environment100>(sql);

                Alter.Table<Environment103>().AddColumn(nameof(Environment103.SortOrder)).AsInt32().NotNullable().WithDefaultValue(Constants.Tree.DefaultEnvironmentSortOrder);
                Alter.Table<Environment103>().AddColumn(nameof(Environment103.Enable)).AsBoolean().NotNullable().WithDefaultValue(true);
                Alter.Table<Environment103>().AddColumn(nameof(Environment103.ContinueProcessing)).AsBoolean().NotNullable().WithDefaultValue(true);
                Alter.Table<Environment103>().AddColumn(nameof(Environment103.ColorIndicator)).AsString(7).NotNullable().WithDefaultValue("#df7f48");
            }
            else
            {
                _schemaHelper.CreateTable<Environment103>();
                _database.Insert(new Environment103
                {
                    Name = "Default",
                    Icon = "icon-firewall red",
                    Enable = true,
                    ContinueProcessing = true,
                    SortOrder = Constants.Tree.DefaultEnvironmentSortOrder,
                    ColorIndicator = "#df7f48"
                });
            }

            //  Domain
            if (!_schemaHelper.TableExist<Domain100>())
            {
                _schemaHelper.CreateTable<Domain100>();
            }

            //  Configuration
            if (!_schemaHelper.TableExist<Configuration100>())
            {
                _schemaHelper.CreateTable<Configuration103>();
            }

            //  Journal
            if (_schemaHelper.TableExist<Journal100>())
            {
                Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Journal100.Datestamp)).OnTable<Journal100>();
                Create.Index("IX_" + nameof(Shield) + "_" + nameof(Journal103.Datestamp)).OnTable<Journal103>()
                    .OnColumn(nameof(Journal103.Datestamp)).Ascending().WithOptions().NonClustered();
            }
            else
            {
                _schemaHelper.CreateTable<Journal103>();
            }
        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
            //  Environment
            Delete.Column(nameof(Environment103.SortOrder)).FromTable<Environment103>();
            Delete.Column(nameof(Environment103.Enable)).FromTable<Environment103>();
            Delete.Column(nameof(Environment103.ContinueProcessing)).FromTable<Environment103>();
            Delete.Column(nameof(Environment103.ColorIndicator)).FromTable<Environment103>();

            //  Journal
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Journal100.Datestamp)).OnTable<Journal100>();
            Create.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Journal) + "_" + nameof(Dto.Configuration))
                .FromTable<Configuration101>().ForeignColumn(nameof(Configuration101.AppId))
                .ToTable<Journal101>().PrimaryColumn(nameof(Journal101.AppId)).OnDeleteOrUpdate(Rule.None);

            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Configuration103.AppId)).OnTable<Configuration103>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Configuration101.AppId)).OnTable<Configuration101>()
                .OnColumn(nameof(Configuration101.AppId)).Ascending().WithOptions().Unique();
        }
    }
}
