using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Shield.Persistance.Dal
{
    [TableName("ShieldConfiguration")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    internal class Configuration
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("lastmodified")]
        public DateTime LastModified { get; set; }
        
        [Column("value")]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }
    }

    [Migration("1.0.0", 1, nameof(Configuration))]
    public class CreateConfigurationTable : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        public CreateConfigurationTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
          : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        public override void Up()
        {
            _schemaHelper.CreateTable<Configuration>(false);

            // Remember you can execute ANY code here and in Down().. 
            // Anything you can think of, go nuts (not really!)
        }

        public override void Down()
        {
            _schemaHelper.DropTable<Configuration>();
        }
    }
}
