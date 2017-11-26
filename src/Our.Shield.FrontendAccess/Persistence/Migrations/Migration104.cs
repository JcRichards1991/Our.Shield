using System.Linq;
using Our.Shield.Core.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Our.Shield.FrontendAccess.Persistence.Migrations
{
    [Migration("1.0.4", 0, nameof(Shield) + nameof(FrontendAccess))]
    public class Migration104 : MigrationBase
    {
        public Migration104(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var context = Core.Persistance.Business.DbContext.Instance.Configuration;
            context.ConfigMapper("FrontendAccess", new
                {
                    umbracoUserEnable = true,
                    ipAddressesAccess = 0,
                    ipAddresses = new Models.IpEntry103[0],
                    unauthorisedAction = TransferTypes.Redirect,
                    urlType = new Models.UrlType103
                    {
                        UrlSelector = UmbracoUrlTypes.Url,
                        StrUrl = "",
                        XPathUrl = "",
                        ContentPickerUrl = ""
                    }
                }, oldData => {
                    return new
                    {
                        oldData.umbracoUserEnable,
                        ipAccessRules = new IpAccessControl
                        {
                            AccessType = oldData.ipAddressesAccess == 0 ? IpAccessControl.AccessTypes.AllowAll : IpAccessControl.AccessTypes.AllowNone,
                            Exceptions = ((Models.IpEntry103[])oldData.ipAddresses).Select(x => new IpAccessControl.Entry { FromIPAddress = x.IpAddress, Description = x.Description, IPAddressType = IpAccessControl.IPAddressType.Single })
                        },
                        unauthorized = new TransferUrl
                        {
                            TransferType = oldData.unauthorisedAction,
                            Url = new UmbracoUrl
                            {
                                Type = oldData.urlType.UrlSelector,
                                Value = oldData.urlType.UrlSelector == UmbracoUrlTypes.Url ? oldData.urlType.StrUrl :
                                    (oldData.urlType.UrlSelector == UmbracoUrlTypes.XPath ? oldData.urlType.XPathUrl :
                                        oldData.urlType.ContentPickerUrl)
                            }
                        }
                    };
                }
            );
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
