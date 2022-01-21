using Newtonsoft.Json;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistence.Data.Dto;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Configuration;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Environment;
using Our.Shield.Core.Persistence.Data.Migrations.Dto.Journal;
using System.Data;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Journal = Our.Shield.Core.Persistence.Data.Dto.Journal;

namespace Our.Shield.Core.Persistence.Data.Migrations.Versions
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

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration103(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            //  Environment
            Alter.Table<Environment103>().AddColumn(nameof(Environment103.SortOrder)).AsInt32().NotNullable().WithDefaultValue(999999);
            Alter.Table<Environment103>().AddColumn(nameof(Environment103.Enable)).AsBoolean().NotNullable().WithDefaultValue(true);
            Alter.Table<Environment103>().AddColumn(nameof(Environment103.ContinueProcessing)).AsBoolean().NotNullable().WithDefaultValue(true);
            Alter.Table<Environment103>().AddColumn(nameof(Environment103.ColorIndicator)).AsString(7).NotNullable().WithDefaultValue("#df7f48");

            //  Journal
            Delete.ForeignKey("FK_" + nameof(Shield) + "_" + nameof(Journal) + "_" + nameof(Dto.Configuration)).OnTable<Journal101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Journal103.Datestamp)).OnTable<Journal103>()
                .OnColumn(nameof(Journal103.Datestamp)).Ascending().WithOptions().NonClustered();

            //  Configuration
            Delete.Index("IX_" + nameof(Shield) + "_" + nameof(Configuration101.AppId)).OnTable<Configuration101>();
            Create.Index("IX_" + nameof(Shield) + "_" + nameof(Configuration103.AppId)).OnTable<Configuration103>()
                .OnColumn(nameof(Configuration103.AppId)).Ascending().WithOptions().NonClustered();

            //  Check if Backoffice Access has been installed, as we may
            //  need to update the configuration to match the new class
            var sql = new Sql();
            sql.Where<Configuration>(x => x.AppId == "BackofficeAccess");

            var config = _database.FirstOrDefault<Configuration>(sql);

            if (config == null)
                return;

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
                oldData.backendAccessUrl,
                oldData.ipAddressesAccess,
                oldData.ipAddresses,
                oldData.unauthorisedAction,
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
