using Newtonsoft.Json;
using Our.Shield.Core.Models;
using System.Data;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using System;

namespace Our.Shield.Core.Persistance.Data.Migrations.Versions
{
    /// <summary>
    /// Handles Creating/Editing the Configuration table
    /// </summary>
    [Migration("1.0.4", 1, nameof(Shield))]
    internal class Migration104 : MigrationBase
    {
        private readonly UmbracoDatabase _database = ApplicationContext.Current.DatabaseContext.Database;
        private readonly DatabaseSchemaHelper _schemaHelper;
        private ISqlSyntaxProvider _sqlSyntax;

        /// <summary>
        /// Default constructor for the Configuration Migration.
        /// </summary>
        /// <param name="sqlSyntax">The SQL Syntax</param>
        /// <param name="logger">The Logger</param>
        public Migration104(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
            _sqlSyntax = sqlSyntax;
            _schemaHelper = new DatabaseSchemaHelper(_database, logger, _sqlSyntax);
        }




        /// <summary>
        /// Creates the Configuration table
        /// </summary>
        public override void Up()
        {
            ConfigMapper("BackofficeAccess", new
                {
                    backendAccessUrl = "",
                    ipAddressesAccess = Enums.IpAddressesAccess.Unrestricted,
                    ipAddresses = new IpEntry[0],
                    unauthorisedAction = TransferTypes.Redirect,
                    urlType = new Migration103.UrlType
                    {
                        UrlSelector = UmbracoUrlTypes.Url,
                        StrUrl = "",
                        XPathUrl = "",
                        ContentPickerUrl = ""
                    }
                }, (oldData) => {
                    return new
                    {
                        backendAccessUrl = oldData.backendAccessUrl,
                        ipAddressesAccess = oldData.ipAddressesAccess,
                        ipAddresses = oldData.ipAddresses,
                        unauthorized = new TransferUrl
                        {
                            TransferType = oldData.unauthorisedAction,
                            Url = new UmbracoUrl
                            {
                                Type = oldData.urlType.UrlSelector,
                                Value = (oldData.urlType.UrlSelector == UmbracoUrlTypes.Url) ? oldData.urlType.StrUrl :
                                    ((oldData.urlType.UrlSelector == UmbracoUrlTypes.XPath) ? oldData.urlType.XPathUrl :
                                    oldData.urlType.ContentPickerUrl)
                            }
                        }
                    };
                }
            );

            ConfigMapper("FrontendAccess", new
                {
                    umbracoUserEnable = true,
                    ipAddressesAccess = Enums.IpAddressesAccess.Unrestricted,
                    ipAddresses = new IpEntry[0],
                    unauthorisedAction = TransferTypes.Redirect,
                    urlType = new Migration103.UrlType
                    {
                        UrlSelector = UmbracoUrlTypes.Url,
                        StrUrl = "",
                        XPathUrl = "",
                        ContentPickerUrl = ""
                    }
                }, (oldData) => {
                    return new
                    {
                        umbracoUserEnable = oldData.umbracoUserEnable,
                        ipAddressesAccess = oldData.ipAddressesAccess,
                        ipAddresses = oldData.ipAddresses,
                        unauthorized = new TransferUrl
                        {
                            TransferType = oldData.unauthorisedAction,
                            Url = new UmbracoUrl
                            {
                                Type = oldData.urlType.UrlSelector,
                                Value = (oldData.urlType.UrlSelector == UmbracoUrlTypes.Url) ? oldData.urlType.StrUrl :
                                    ((oldData.urlType.UrlSelector == UmbracoUrlTypes.XPath) ? oldData.urlType.XPathUrl :
                                    oldData.urlType.ContentPickerUrl)
                            }
                        }
                    };
                }
            );
        }

        /// <summary>
        /// Drops the Configurations table
        /// </summary>
        public override void Down()
        {
        }

        private void ConfigMapper(string appId, dynamic definition, Func<dynamic, dynamic> map)
        {
            var sql = new Sql();
            sql.Where<Data.Dto.Configuration>(x => x.AppId == appId, _sqlSyntax);

            var config = _database.FirstOrDefault<Data.Dto.Configuration>(sql);

            if (config != null)
            {
                //  Deserialize the current config to an anonymous object
                var oldData = JsonConvert.DeserializeAnonymousType(config.Value, definition);

                //  serialize the new configuration to the db entry's value
                config.Value = JsonConvert.SerializeObject(map(oldData), Formatting.None);

                //  Update the entry within the DB.
                _database.Update(config);
            }
        }
    }
}
