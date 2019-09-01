using Our.Shield.Core.Models;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.FrontendAccess.Models;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.FrontendAccess.Persistence.Migrations
{
    [Migration("1.0.0.4", 0, nameof(Shield) + nameof(FrontendAccess))]
    public class Migration104 : MigrationBase
    {
        public Migration104(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var context = DbContext.Instance.Configuration;
            context.ConfigMapper("FrontendAccess", new Models.Configuration103(), dbData =>
            {
                var oldData = dbData as Models.Configuration103;
                return new FrontendAccessConfiguration
                {
                    UmbracoUserEnable = oldData.UmbracoUserEnable,
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
