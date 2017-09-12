using Newtonsoft.Json;
using Our.Shield.Core.Models;
using System.Data;
using System.Linq;
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
    [Migration("1.0.3", 1, nameof(Shield))]
    internal class Migration103 : MigrationBase
    {
        public class IpEntry103
        {
            /// <summary>
            /// Gets or set the IP Address
            /// </summary>
            [JsonProperty("ipAddress")]
            public string IpAddress { get; set; }

            /// <summary>
            /// Gets or sets a description for this IP Address 
            /// </summary>
            [JsonProperty("description")]
            public string Description { get; set; }
        }

        public class UrlType103
        {
            /// <summary>
            /// The selector for the url
            /// </summary>
            [JsonProperty("urlSelector")]
            public UmbracoUrlTypes UrlSelector { get; set; }

            /// <summary>
            /// The Url for the Url
            /// </summary>
            [JsonProperty("strUrl")]
            public string StrUrl { get; set; }

            /// <summary>
            /// The XPath to the content node for the Url
            /// </summary>
            [JsonProperty("xpathUrl")]
            public string XPathUrl { get; set; }

            /// <summary>
            /// The Id/UID to the content node for the Url
            /// </summary>
            [JsonProperty("contentPickerUrl")]
            public string ContentPickerUrl { get; set; }
        }

        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;
        private ISqlSyntaxProvider _sqlSyntax;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration103(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _sqlSyntax = sqlSyntax;
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, _sqlSyntax);
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            Alter.Table<Dto.Environment.Environment103>().AddColumn(nameof(Dto.Environment.Environment103.SortOrder)).AsInt32().NotNullable().WithDefaultValue(999999);
            Alter.Table<Dto.Environment.Environment103>().AddColumn(nameof(Dto.Environment.Environment103.Enable)).AsBoolean().NotNullable().WithDefaultValue(true);
            Alter.Table<Dto.Environment.Environment103>().AddColumn(nameof(Dto.Environment.Environment103.ContinueProcessing)).AsBoolean().NotNullable().WithDefaultValue(true);
            Alter.Table<Dto.Environment.Environment103>().AddColumn(nameof(Dto.Environment.Environment103.ColorIndicator)).AsString(7).NotNullable().WithDefaultValue("#df7f48");

            //  Journal
            Delete.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Dto.Configuration)).OnTable<Dto.Journal.Journal101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal103.Datestamp)).OnTable<Dto.Journal.Journal103>()
                .OnColumn(nameof(Dto.Journal.Journal103.Datestamp)).Ascending().WithOptions().NonClustered();

            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration101.AppId)).OnTable<Dto.Configuration.Configuration101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration103.AppId)).OnTable<Dto.Configuration.Configuration103>()
                .OnColumn(nameof(Dto.Configuration.Configuration103.AppId)).Ascending().WithOptions().NonClustered();

            //  Check if Backoffice Access has been installed, as we may
            //  need to update the configuration to match the new class
            var sql = new Sql();
            sql.Where<Data.Dto.Configuration>(x => x.AppId == "BackofficeAccess", _sqlSyntax);

            var config = _database.FirstOrDefault<Data.Dto.Configuration>(sql);

            if (config != null)
            {
                var definition = new
                {
                    backendAccessUrl = "",
                    ipAddressesAccess = 0,
                    ipAddresses = new IpEntry103[0],
                    unauthorisedAction = TransferTypes.Redirect,
                    unauthorisedUrlType = UmbracoUrlTypes.Url,
                    unauthorisedUrl = "",
                    unauthorisedUrlXPath = "",
                    unauthorisedUrlContentPicker = ""
                };

                //  Deserialize the current config to an anonymous object
                var oldData = JsonConvert.DeserializeAnonymousType(config.Value, definition);

                //  Copy the configuration to the new anonymous object
                var newData = new
                {
                    backendAccessUrl = oldData.backendAccessUrl,
                    ipAddressesAccess = oldData.ipAddressesAccess,
                    ipAddresses = oldData.ipAddresses,
                    unauthorisedAction = oldData.unauthorisedAction,
                    urlType = new UrlType103
                    {
                        UrlSelector = oldData.unauthorisedUrlType,
                        StrUrl = oldData.unauthorisedUrl,
                        XPathUrl = oldData.unauthorisedUrlXPath,
                        ContentPickerUrl = oldData.unauthorisedUrlContentPicker
                    }
                };

                //  serialize the new configuration to the db entry's value
                config.Value = JsonConvert.SerializeObject(newData, Formatting.None);

                //  Update the entry within the DB.
                _database.Update(config);
            }
        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
            //  Environment
            Delete.Column(nameof(Dto.Environment.Environment103.SortOrder)).FromTable<Dto.Environment.Environment103>();
            Delete.Column(nameof(Dto.Environment.Environment103.Enable)).FromTable<Dto.Environment.Environment103>();
            Delete.Column(nameof(Dto.Environment.Environment103.ContinueProcessing)).FromTable<Dto.Environment.Environment103>();
            Delete.Column(nameof(Dto.Environment.Environment103.ColorIndicator)).FromTable<Dto.Environment.Environment103>();

            //  Journal
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Journal.Journal100.Datestamp)).OnTable<Dto.Journal.Journal100>();
            Create.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Data.Dto.Journal) + "_" + nameof(Dto.Configuration))
                .FromTable<Dto.Configuration.Configuration101>().ForeignColumn(nameof(Dto.Configuration.Configuration101.AppId))
                .ToTable<Dto.Journal.Journal101>().PrimaryColumn(nameof(Dto.Journal.Journal101.AppId)).OnDeleteOrUpdate(Rule.None);

            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration103.AppId)).OnTable<Dto.Configuration.Configuration103>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Dto.Configuration.Configuration101.AppId)).OnTable<Dto.Configuration.Configuration101>()
                .OnColumn(nameof(Dto.Configuration.Configuration101.AppId)).Ascending().WithOptions().Unique();
        }
    }
}
