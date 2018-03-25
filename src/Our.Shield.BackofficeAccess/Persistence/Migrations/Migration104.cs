using Our.Shield.BackofficeAccess.Models;
using Our.Shield.Core.Models;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.BackofficeAccess.Persistence.Migrations
{
    [Migration("1.0.0.4", 0, nameof(Shield) + nameof(BackofficeAccess))]
    public class Migration104 : MigrationBase
    {
        public Migration104(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var context = Core.Persistance.Business.DbContext.Instance.Configuration;
            context.ConfigMapper("BackofficeAccess", new Models.Configuration103(), dbData =>
            {
                var oldData = dbData as Models.Configuration103;
                return new BackofficeAccessConfiguration
                {
                    BackendAccessUrl = oldData.BackendAccessUrl,
                    IpAccessRules = new IpAccessControl
                    {
                        AccessType = oldData.IpAddressesAccess == 0 ? IpAccessControl.AccessTypes.AllowAll : IpAccessControl.AccessTypes.AllowNone,
                        Exceptions = oldData.IpAddresses.Select(x => new IpAccessControl.Entry { FromIpAddress = x.IpAddress, Description = x.Description, IpAddressType = IpAccessControl.IpAddressType.Single })
                    },
                    Unauthorized = new TransferUrl
                    {
                        TransferType = oldData.UnauthorisedAction,
                        Url = new UmbracoUrl
                        {
                            Type = oldData.UrlType.UrlSelector,
                            Value = oldData.UrlType.UrlSelector == UmbracoUrlTypes.Url ? oldData.UrlType.StrUrl :
                                (oldData.UrlType.UrlSelector == UmbracoUrlTypes.XPath ? oldData.UrlType.XPathUrl :
                                    oldData.UrlType.ContentPickerUrl)
                        }
                    }
                };
            });
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
