using Our.Shield.Core.Persistence.Data.Migrations.Dto.Configuration;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Environment;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.Core.Persistence.Data.Migrations.Versions
{
    [Migration("1.0.7", 1, nameof(Shield))]
    internal class Migration107 : MigrationBase
    {
        public Migration107(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //  Environment
            Alter.Table<Environment107>().AddColumn(nameof(Environment107.Key)).AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);

            //  Configuration
            Alter.Table<Configuration107>().AddColumn(nameof(Configuration107.Key)).AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);
        }

        public override void Down()
        {
            //  Environment
            Delete.Column(nameof(Environment107.Key)).FromTable<Environment107>();

            //  Configuration
            Delete.Column(nameof(Configuration107.Key)).FromTable<Configuration107>();
        }
    }
}
