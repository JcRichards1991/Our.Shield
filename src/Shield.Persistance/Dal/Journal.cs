using Semver;
using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Web;

namespace Shield.Persistance.Dal
{
    [TableName("ShieldJournal")]
    [ExplicitColumns]
    internal class Journal
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }


        [Column("configuration")]
        [ForeignKey(typeof(Configuration), Name = "FK_Journal_Configuration")]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_ConfigurationId")]
        public Guid ConfigurationId { get; set; }

        [Column("createdate")]
        public DateTime CreateDate { get; set; }
        
        [Column("value")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Value { get; set; }
    }

    [Migration("1.0.0", 2, "Shield")]
    public class CreateJournalTable : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;

        public CreateJournalTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
          : base(sqlSyntax, logger)
        {
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, sqlSyntax);
        }

        public override void Up()
        {
            _schemaHelper.CreateTable<Journal>(false);

            // Remember you can execute ANY code here and in Down().. 
            // Anything you can think of, go nuts (not really!)
        }

        public override void Down()
        {
            _schemaHelper.DropTable<Journal>();
        }
    }
}
